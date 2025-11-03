using System;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.Toolkit.Framework.ModBlacklistData;

/// <summary>Handles access to SMAPI's internal 'malicious mods' blacklist.</summary>
public class ModBlacklist
{
    /*********
    ** Accessors
    *********/
    /// <summary>The underlying mod blacklist data.</summary>
    public ModBlacklistModel Blacklist { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an empty instance.</summary>
    public ModBlacklist()
        : this(new ModBlacklistModel([])) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="data">The underlying mod blacklist data.</param>
    public ModBlacklist(ModBlacklistModel data)
    {
        this.Blacklist = data;
    }

    /// <summary>Get the blacklist entry for a mod, if any.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="entryDllPath">The absolute path to the entry DLL, if this is a C# mod.</param>
    public ModBlacklistEntryModel? Get(string modId, string? entryDllPath)
    {
        string? entryDllHash = null;

        foreach (ModBlacklistEntryModel entry in this.Blacklist.Blacklist)
        {
            // check mod ID
            if (entry.Id != null && !string.Equals(modId, entry.Id, StringComparison.OrdinalIgnoreCase))
                continue;

            // check entry DLL hash
            if (entry.EntryDllHash != null)
            {
                if (entryDllPath is null)
                    continue;

                entryDllHash ??= FileUtilities.GetFileHash(entryDllPath);
                if (!string.Equals(entryDllHash, entry.EntryDllHash, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            return entry;
        }

        return null;
    }
}
