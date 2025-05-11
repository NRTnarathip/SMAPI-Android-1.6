using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Framework;

/// <summary>Validates manifest fields.</summary>
public static class ManifestValidator
{
    /// <summary>Validate a manifest's fields.</summary>
    /// <param name="manifest">The manifest to validate.</param>
    /// <param name="error">The error message indicating why validation failed, if applicable.</param>
    /// <returns>Returns whether all manifest fields validated successfully.</returns>
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = "This is the method that ensures those annotations are respected.")]
    public static bool TryValidateFields(IManifest manifest, out string error)
    {
        //
        // Note: SMAPI assumes that it can grammatically append the returned sentence in the
        // form "failed loading <mod> because its <error>". Any errors returned should be valid
        // in that format, unless the SMAPI call is adjusted accordingly.
        //

        bool hasDll = !string.IsNullOrWhiteSpace(manifest.EntryDll);
        bool isContentPack = manifest.ContentPackFor != null;

        // validate use of EntryDll vs ContentPackFor fields
        if (hasDll == isContentPack)
        {
            error = hasDll
                ? $"manifest sets both {nameof(IManifest.EntryDll)} and {nameof(IManifest.ContentPackFor)}, which are mutually exclusive."
                : $"manifest has no {nameof(IManifest.EntryDll)} or {nameof(IManifest.ContentPackFor)} field; must specify one.";
            return false;
        }

        // validate EntryDll/ContentPackFor format
        if (hasDll)
        {
            if (manifest.EntryDll!.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                error = $"manifest has invalid filename '{manifest.EntryDll}' for the {nameof(IManifest.EntryDll)} field.";
                return false;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(manifest.ContentPackFor!.UniqueID))
            {
                error = $"manifest declares {nameof(IManifest.ContentPackFor)} without its required {nameof(IManifestContentPackFor.UniqueID)} field.";
                return false;
            }
        }

        // validate required fields
        {
            List<string> missingFields = new(3);

            if (string.IsNullOrWhiteSpace(manifest.Name))
                missingFields.Add(nameof(IManifest.Name));
            if (manifest.Version == null || manifest.Version.ToString() == "0.0.0")
                missingFields.Add(nameof(IManifest.Version));
            if (string.IsNullOrWhiteSpace(manifest.UniqueID))
                missingFields.Add(nameof(IManifest.UniqueID));

            if (missingFields.Any())
            {
                error = $"manifest is missing required fields ({string.Join(", ", missingFields)}).";
                return false;
            }
        }

        // validate ID format
        if (!PathUtilities.IsSlug(manifest.UniqueID))
        {
            error = "manifest specifies an invalid ID (IDs must only contain letters, numbers, underscores, periods, or hyphens).";
            return false;
        }

        // validate dependency format
        foreach (IManifestDependency? dependency in manifest.Dependencies)
        {
            if (dependency is null)
            {
                error = $"manifest has a null entry under {nameof(IManifest.Dependencies)}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dependency.UniqueID))
            {
                error = $"manifest has a {nameof(IManifest.Dependencies)} entry with no {nameof(IManifestDependency.UniqueID)} field.";
                return false;
            }

            if (!PathUtilities.IsSlug(dependency.UniqueID))
            {
                error = $"manifest has a {nameof(IManifest.Dependencies)} entry with an invalid {nameof(IManifestDependency.UniqueID)} field (IDs must only contain letters, numbers, underscores, periods, or hyphens).";
                return false;
            }
        }

        error = "";
        return true;
    }
}
