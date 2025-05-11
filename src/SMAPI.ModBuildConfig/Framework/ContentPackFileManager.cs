using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Toolkit.Serialization.Models;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingAPI.ModBuildConfig.Framework;

/// <summary>Manages the files that are part of a bundled content pack.</summary>
internal class ContentPackFileManager : IModFileManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The name of the manifest file.</summary>
    private readonly string ManifestFileName = "manifest.json";

    /// <summary>The files that are part of the package.</summary>
    private readonly Dictionary<string, FileInfo> Files = new(StringComparer.OrdinalIgnoreCase);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="projectDir">The folder containing the project files.</param>
    /// <param name="contentPackDir">The absolute or relative path to the content pack folder.</param>
    /// <param name="version">The mod version.</param>
    /// <param name="ignoreFilePaths">The custom relative file paths provided by the user to ignore.</param>
    /// <param name="ignoreFilePatterns">Custom regex patterns matching files to ignore when deploying or zipping the mod.</param>
    /// <param name="validateManifest">Whether to validate that the content pack's manifest is valid.</param>
    /// <exception cref="UserErrorException">The mod package isn't valid.</exception>
    public ContentPackFileManager(string projectDir, string contentPackDir, string version, string[] ignoreFilePaths, Regex[] ignoreFilePatterns, bool validateManifest)
    {
        // get folders
        DirectoryInfo projectDirInfo = new(Path.Combine(projectDir, contentPackDir));
        if (!projectDirInfo.Exists)
            throw GetError($"that folder doesn't exist at {projectDirInfo.FullName}");

        // collect files
        foreach (FileInfo entry in projectDirInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            string relativePath = PathUtilities.GetRelativePath(projectDirInfo.FullName, entry.FullName);
            FileInfo file = entry;

            if (!this.ShouldIgnore(file, relativePath, ignoreFilePaths, ignoreFilePatterns))
                this.Files[relativePath] = file;
        }

        // validate manifest
        if (validateManifest)
        {
            // get manifest file
            if (!this.Files.TryGetValue(this.ManifestFileName, out FileInfo manifestFile))
                throw GetError($"it has no {this.ManifestFileName} file");

            // parse file
            Manifest manifest;
            try
            {
                new JsonHelper().ReadJsonFileIfExists(manifestFile.FullName, out Manifest rawManifest);
                manifest = rawManifest;
            }
            catch (JsonReaderException ex)
            {
                throw GetError($"its {this.ManifestFileName} file isn't valid JSON: {ex.InnerException?.Message ?? ex.Message}");
            }

            // validate manifest fields
            if (!ManifestValidator.TryValidateFields(manifest, out string error))
                throw GetError($"its {this.ManifestFileName} file is invalid: {error}");

            // validate version
            if (version == null)
                throw GetError("no Version value was provided");
            if (!SemanticVersion.TryParse(version, out ISemanticVersion requiredVersion))
                throw GetError($"the provided Version value '{version}' isn't a valid semantic version");
            if (manifest.Version.CompareTo(requiredVersion) != 0)
                throw GetError($"its {this.ManifestFileName} has version '{manifest.Version}' instead of the required '{requiredVersion}'");
        }

        UserErrorException GetError(string reasonPhrase)
        {
            return new UserErrorException($"The content pack at '{contentPackDir}' can't be loaded because {reasonPhrase}.");
        }
    }

    ///<inheritdoc/>
    public IDictionary<string, FileInfo> GetFiles()
    {
        return new Dictionary<string, FileInfo>(this.Files, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Get whether a content file should be ignored.</summary>
    /// <param name="file">The file to check.</param>
    /// <param name="relativePath">The file's relative path in the package.</param>
    /// <param name="ignoreFilePaths">The custom relative file paths provided by the user to ignore.</param>
    /// <param name="ignoreFilePatterns">Custom regex patterns matching files to ignore when deploying or zipping the mod.</param>
    private bool ShouldIgnore(FileInfo file, string relativePath, string[] ignoreFilePaths, Regex[] ignoreFilePatterns)
    {
        // apply custom patterns
        if (ignoreFilePaths.Any(p => p == relativePath) || ignoreFilePatterns.Any(p => p.IsMatch(relativePath)))
            return true;

        // ignore special files
        return
            // release zips
            this.EqualsInvariant(file.Extension, ".zip")

            // OS metadata files
            || this.EqualsInvariant(file.Name, ".DS_Store")
            || this.EqualsInvariant(file.Name, "Thumbs.db");
    }

    // <summary>Get whether a string is equal to another case-insensitively.</summary>
    /// <param name="str">The string value.</param>
    /// <param name="other">The string to compare with.</param>
    private bool EqualsInvariant(string str, string other)
    {
        if (str == null)
            return other == null;
        return str.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
}
