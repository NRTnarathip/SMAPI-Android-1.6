using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace StardewModdingAPI.Toolkit.Framework.Clients.CompatibilityRepo;

/// <summary>A set of changes which can be applied to a mod data field.</summary>
public class ChangeDescriptor
{
    /*********
    ** Accessors
    *********/
    /// <summary>The values to add to the field.</summary>
    public ISet<string> Add { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The values to remove from the field.</summary>
    public ISet<string> Remove { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The values to replace in the field, if matched.</summary>
    public IDictionary<string, string> Replace { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether the change descriptor would make any changes.</summary>
    public bool HasChanges => this.Add.Count > 0 || this.Remove.Count > 0 || this.Replace.Count > 0;

    /// <summary>Format a raw value into a normalized form.</summary>
    public Func<string, string> FormatValue { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="formatValue">Format a raw value into a normalized form.</param>
    public ChangeDescriptor(Func<string, string> formatValue)
    {
        this.FormatValue = formatValue;
    }

    /// <summary>Add a change to this descriptor.</summary>
    /// <param name="from">The specific value to replace with the `to` field. For a version number, this must match the exact formatting before the version is parsed.</param>
    /// <param name="to">The value to use instead of the `from` value.</param>
    public void AddChange(string? from, string? to)
    {
        from = from != null
            ? this.FormatValue(from)
            : null;

        to = to != null
            ? this.FormatValue(to)
            : null;

        if (from != null && to != null)
            this.Replace[from] = to;
        else if (from != null)
            this.Remove.Add(from);
        else if (to != null)
            this.Add.Add(to);
    }

    /// <summary>Apply the change descriptors to a comma-delimited field.</summary>
    /// <param name="rawField">The raw field text.</param>
    /// <returns>Returns the modified field.</returns>
#if NET6_0_OR_GREATER
    [return: NotNullIfNotNull("rawField")]
#endif
    public string? ApplyToCopy(string? rawField)
    {
        // get list
        List<string> values = !string.IsNullOrWhiteSpace(rawField)
            ? new List<string>(
                from field in rawField.Split(',')
                let value = field.Trim()
                where value.Length > 0
                select value
            )
            : new List<string>();

        // apply changes
        this.Apply(values);

        // format
        if (rawField == null && !values.Any())
            return null;
        return string.Join(", ", values);
    }

    /// <summary>Apply the change descriptors to the given field values.</summary>
    /// <param name="values">The field values.</param>
    /// <returns>Returns the modified field values.</returns>
    public void Apply(List<string> values)
    {
        // replace/remove values
        if (this.Replace.Any() || this.Remove.Any())
        {
            for (int i = values.Count - 1; i >= 0; i--)
            {
                string value = this.FormatValue(values[i].Trim());

                if (this.Remove.Contains(value))
                    values.RemoveAt(i);

                else if (this.Replace.TryGetValue(value, out string? newValue))
                    values[i] = newValue;
            }
        }

        // add values
        if (this.Add.Any())
        {
            HashSet<string> curValues = new HashSet<string>(values.Select(p => p.Trim()), StringComparer.OrdinalIgnoreCase);
            foreach (string add in this.Add)
            {
                if (!curValues.Contains(add))
                {
                    values.Add(add);
                    curValues.Add(add);
                }
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!this.HasChanges)
            return string.Empty;

        List<string> descriptors = new List<string>(this.Add.Count + this.Remove.Count + this.Replace.Count);
        foreach (string add in this.Add)
            descriptors.Add($"+{add}");
        foreach (string remove in this.Remove)
            descriptors.Add($"-{remove}");
        foreach (var pair in this.Replace)
            descriptors.Add($"{pair.Key} → {pair.Value}");

        return string.Join(", ", descriptors);
    }

    /// <summary>Parse a raw change descriptor string into a <see cref="ChangeDescriptor"/> model.</summary>
    /// <param name="descriptor">The raw change descriptor.</param>
    /// <param name="errors">The human-readable error message describing any invalid values that were ignored.</param>
    /// <param name="formatValue">Format a raw value into a normalized form if needed.</param>
    public static ChangeDescriptor Parse(string? descriptor, out string[] errors, Func<string, string>? formatValue = null)
    {
        var parsed = new ChangeDescriptor(formatValue ?? (p => p));

        // parse each change in the descriptor
        if (!string.IsNullOrWhiteSpace(descriptor))
        {
            List<string> rawErrors = new List<string>();
            foreach (string rawEntry in descriptor.Split(','))
            {
                // normalize entry
                string entry = rawEntry.Trim();
                if (entry == string.Empty)
                    continue;

                // parse as replace (old value → new value)
                if (entry.Contains('→'))
                {
                    string[] parts = entry.Split(new[] { '→' }, 2);
                    string oldValue = parts[0].Trim();
                    string newValue = parts[1].Trim();

                    if (oldValue == string.Empty)
                    {
                        rawErrors.Add($"Failed parsing '{rawEntry}': can't map from a blank old value. Use the '+value' format to add a value.");
                        continue;
                    }

                    if (newValue == string.Empty)
                    {
                        rawErrors.Add($"Failed parsing '{rawEntry}': can't map to a blank value. Use the '-value' format to remove a value.");
                        continue;
                    }

                    parsed.AddChange(oldValue, newValue);
                }

                // else as remove
                else if (entry.StartsWith("-"))
                {
                    entry = entry.Substring(1).Trim();
                    parsed.AddChange(entry, null);
                }

                // else as add
                else
                {
                    if (entry.StartsWith("+"))
                        entry = entry.Substring(1).Trim();
                    parsed.AddChange(null, entry);
                }
            }

            errors = rawErrors.ToArray();
        }
        else
            errors = [];

        // build model
        return parsed;
    }
}
