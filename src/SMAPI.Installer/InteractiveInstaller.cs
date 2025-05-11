using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using StardewModdingApi.Installer.Enums;
using StardewModdingAPI.Installer.Framework;
using StardewModdingAPI.Internal.ConsoleWriting;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework;
using StardewModdingAPI.Toolkit.Framework.GameScanning;
using StardewModdingAPI.Toolkit.Framework.ModScanning;
using StardewModdingAPI.Toolkit.Utilities;

namespace StardewModdingApi.Installer;

/// <summary>Interactively performs the install and uninstall logic.</summary>
internal class InteractiveInstaller
{
    /*********
    ** Fields
    *********/
    /// <summary>The absolute path to the directory containing the files to copy into the game folder.</summary>
    private readonly string BundlePath;

    /// <summary>The mod IDs which the installer should allow as bundled mods.</summary>
    private readonly string[] BundledModIds = ["SMAPI.SaveBackup", "SMAPI.ConsoleCommands"];

    /// <summary>Get the absolute file or folder paths to remove when uninstalling SMAPI.</summary>
    /// <param name="installDir">The folder for Stardew Valley and SMAPI.</param>
    /// <param name="modsDir">The folder for SMAPI mods.</param>
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "These are valid file names.")]
    private IEnumerable<string> GetUninstallPaths(DirectoryInfo installDir, DirectoryInfo modsDir)
    {
        string GetInstallPath(string path) => Path.Combine(installDir.FullName, path);

        // installed files
        yield return GetInstallPath("StardewModdingAPI");          // Linux/macOS only
        yield return GetInstallPath("StardewModdingAPI.deps.json");
        yield return GetInstallPath("StardewModdingAPI.dll");
        yield return GetInstallPath("StardewModdingAPI.exe");
        yield return GetInstallPath("StardewModdingAPI.exe.config");
        yield return GetInstallPath("StardewModdingAPI.exe.mdb");  // before 3.18.4 (Linux/macOS only)
        yield return GetInstallPath("StardewModdingAPI.pdb");      // before 3.18.4 (Windows only)
        yield return GetInstallPath("StardewModdingAPI.runtimeconfig.json");
        yield return GetInstallPath("StardewModdingAPI.xml");
        yield return GetInstallPath("smapi-internal");
        yield return GetInstallPath("steam_appid.txt");

        // obsolete files
        yield return GetInstallPath("libgdiplus.dylib");                 // before 3.13 (macOS only)
        yield return GetInstallPath(Path.Combine("Mods", ".cache"));     // 1.3-1.4
        yield return GetInstallPath(Path.Combine("Mods", "ErrorHandler")); // before 4.0 (no longer needed)
        yield return GetInstallPath(Path.Combine("Mods", "TrainerMod")); // before 2.0 (renamed to ConsoleCommands)
        yield return GetInstallPath("Mono.Cecil.Rocks.dll");             // 1.3-1.8
        yield return GetInstallPath("StardewModdingAPI-settings.json");  // 1.0-1.4
        yield return GetInstallPath("StardewModdingAPI.AssemblyRewriters.dll"); // 1.3-2.5.5
        yield return GetInstallPath("0Harmony.dll");                    // moved in 2.8
        yield return GetInstallPath("0Harmony.pdb");                    // moved in 2.8
        yield return GetInstallPath("Mono.Cecil.dll");                  // moved in 2.8
        yield return GetInstallPath("Newtonsoft.Json.dll");             // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.config.json");   // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.crash.marker");  // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.metadata.json"); // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.update.marker"); // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.dll");   // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.pdb");   // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.xml");   // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.CoreInterfaces.dll"); // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.CoreInterfaces.pdb"); // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI.Toolkit.CoreInterfaces.xml"); // moved in 2.8
        yield return GetInstallPath("StardewModdingAPI-x64.exe");         // before 3.13

        // old log files
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "ErrorLogs");
    }

    /// <summary>Handles writing text to the console.</summary>
    private IConsoleWriter ConsoleWriter;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="bundlePath">The absolute path to the directory containing the files to copy into the game folder.</param>
    public InteractiveInstaller(string bundlePath)
    {
        this.BundlePath = bundlePath;
        this.ConsoleWriter = new ColorfulConsoleWriter(EnvironmentUtility.DetectPlatform());
    }

    /// <summary>Run the install or uninstall script.</summary>
    /// <param name="args">The command line arguments.</param>
    /// <remarks>
    /// Initialization flow:
    ///     1. Collect information (mainly OS and install path) and validate it.
    ///     2. Ask the user whether to install or uninstall.
    ///
    /// Uninstall logic:
    ///     1. On Linux/macOS: if a backup of the launcher exists, delete the launcher and restore the backup.
    ///     2. Delete all files and folders in the game directory matching one of the values returned by <see cref="GetUninstallPaths"/>.
    ///
    /// Install flow:
    ///     1. Run the uninstall flow.
    ///     2. Copy the SMAPI files from package/Windows or package/Mono into the game directory.
    ///     3. On Linux/macOS: back up the game launcher and replace it with the SMAPI launcher. (This isn't possible on Windows, so the user needs to configure it manually.)
    ///     4. Create the 'Mods' directory.
    ///     5. Copy the bundled mods into the 'Mods' directory (deleting any existing versions).
    ///     6. Move any mods from app data into game's mods directory.
    /// </remarks>
    public void Run(string[] args)
    {
        /*********
        ** Step 1: initial setup
        *********/
        /****
        ** Get basic info & set window title
        ****/
        ModToolkit toolkit = new();
        var context = new InstallerContext();
        Console.Title = $"SMAPI {context.GetInstallerVersion()} installer on {context.Platform} {context.PlatformName}";
        Console.WriteLine();

        /****
        ** read command-line arguments
        ****/
        // get input mode
        bool allowUserInput = !args.Contains("--no-prompt");

        // get action
        bool installArg = args.Contains("--install");
        bool uninstallArg = args.Contains("--uninstall");
        if (installArg && uninstallArg)
        {
            this.PrintError("You can't specify both --install and --uninstall command-line flags.");
            this.AwaitConfirmation(allowUserInput);
            return;
        }
        if (!allowUserInput && !installArg && !uninstallArg)
        {
            this.PrintError("You must specify --install or --uninstall when running with --no-prompt.");
            return;
        }

        // get game path from CLI
        string? gamePathArg = null;
        {
            int pathIndex = Array.LastIndexOf(args, "--game-path") + 1;
            if (pathIndex >= 1 && args.Length >= pathIndex)
                gamePathArg = args[pathIndex];
        }

        /****
        ** Check if correct installer
        ****/
#if SMAPI_FOR_WINDOWS
        if (context.IsUnix)
        {
            this.PrintError($"This is the installer for Windows. Run the 'install on {context.Platform}.{(context.Platform == Platform.Mac ? "command" : "sh")}' file instead.");
            this.AwaitConfirmation(allowUserInput);
            return;
        }
#else
        if (context.IsWindows)
        {
            this.PrintError($"This is the installer for Linux/macOS. Run the 'install on Windows.exe' file instead.");
            this.AwaitConfirmation(allowUserInput);
            return;
        }
#endif


        /*********
        ** Step 2: choose a theme (can't auto-detect on Linux/macOS)
        *********/
        MonitorColorScheme scheme = MonitorColorScheme.AutoDetect;
        if (context.IsUnix && allowUserInput)
        {
            /****
            ** print header
            ****/
            this.PrintPlain("Hi there! I'll help you install or remove SMAPI. Just a few questions first.");
            this.PrintPlain("----------------------------------------------------------------------------");
            Console.WriteLine();

            /****
            ** show theme selector
            ****/
            // get theme writers
            ColorfulConsoleWriter lightBackgroundWriter = new(context.Platform, ColorfulConsoleWriter.GetDefaultColorSchemeConfig(MonitorColorScheme.LightBackground));
            ColorfulConsoleWriter darkBackgroundWriter = new(context.Platform, ColorfulConsoleWriter.GetDefaultColorSchemeConfig(MonitorColorScheme.DarkBackground));

            // print question
            this.PrintPlain("Which text looks more readable?");
            Console.WriteLine();
            Console.Write("   [1] ");
            lightBackgroundWriter.WriteLine("Dark text on light background", ConsoleLogLevel.Info);
            Console.Write("   [2] ");
            darkBackgroundWriter.WriteLine("Light text on dark background", ConsoleLogLevel.Info);
            Console.WriteLine();

            // handle choice
            string choice = this.InteractivelyChoose("Type 1 or 2, then press enter.", ["1", "2"], printLine: Console.WriteLine);
            switch (choice)
            {
                case "1":
                    scheme = MonitorColorScheme.LightBackground;
                    this.ConsoleWriter = lightBackgroundWriter;
                    break;
                case "2":
                    scheme = MonitorColorScheme.DarkBackground;
                    this.ConsoleWriter = darkBackgroundWriter;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected action key '{choice}'.");
            }
        }
        Console.Clear();


        /*********
        ** Step 3: find game folder
        *********/
        InstallerPaths paths;
        {
            /****
            ** print header
            ****/
            this.PrintInfo("Hi there! I'll help you install or remove SMAPI. Just a few questions first.");
            this.PrintDebug($"Color scheme: {this.GetDisplayText(scheme)}");
            this.PrintDebug("----------------------------------------------------------------------------");
            Console.WriteLine();

            /****
            ** collect details
            ****/
            // get game path
            DirectoryInfo? installDir = this.InteractivelyGetInstallPath(toolkit, context, gamePathArg);
            if (installDir == null)
            {
                this.PrintError("Failed finding your game path.");
                this.AwaitConfirmation(allowUserInput);
                return;
            }

            // get folders
            DirectoryInfo bundleDir = new(this.BundlePath);
            paths = new InstallerPaths(bundleDir, installDir);
        }


        /*********
        ** Step 4: validate assumptions
        *********/
        // executable exists
        if (!File.Exists(paths.GameDllPath))
        {
            this.PrintError("The detected game install path doesn't contain a Stardew Valley executable.");
            this.AwaitConfirmation(allowUserInput);
            return;
        }
        Console.Clear();


        /*********
        ** Step 5: ask what to do
        *********/
        ScriptAction action;
        {
            /****
            ** print header
            ****/
            this.PrintInfo("Hi there! I'll help you install or remove SMAPI. Just one question first.");
            this.PrintDebug($"Game path: {paths.GamePath}");
            this.PrintDebug($"Color scheme: {this.GetDisplayText(scheme)}");
            this.PrintDebug("----------------------------------------------------------------------------");
            Console.WriteLine();

            /****
            ** ask what to do
            ****/
            if (installArg)
                action = ScriptAction.Install;
            else if (uninstallArg)
                action = ScriptAction.Uninstall;
            else
            {
                this.PrintInfo("What do you want to do?");
                Console.WriteLine();
                this.PrintInfo("[1] Install SMAPI.");
                this.PrintInfo("[2] Uninstall SMAPI.");
                Console.WriteLine();

                string choice = this.InteractivelyChoose("Type 1 or 2, then press enter.", ["1", "2"]);
                switch (choice)
                {
                    case "1":
                        action = ScriptAction.Install;
                        break;
                    case "2":
                        action = ScriptAction.Uninstall;
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected action key '{choice}'.");
                }
            }
        }
        Console.Clear();


        /*********
        ** Step 6: apply
        *********/
        {
            /****
            ** print header
            ****/
            this.PrintInfo($"That's all I need! I'll {action.ToString().ToLower()} SMAPI now.");
            this.PrintDebug($"Game path: {paths.GamePath}");
            this.PrintDebug($"Color scheme: {this.GetDisplayText(scheme)}");
            this.PrintDebug("----------------------------------------------------------------------------");
            Console.WriteLine();

            /****
            ** Back up user settings
            ****/
            if (File.Exists(paths.ApiUserConfigPath))
                File.Copy(paths.ApiUserConfigPath, paths.BundleApiUserConfigPath);

            /****
            ** Always uninstall old files
            ****/
            // restore game launcher
            if (context.IsUnix && File.Exists(paths.BackupLaunchScriptPath))
            {
                this.PrintDebug("Removing SMAPI launcher...");
                this.InteractivelyDelete(paths.VanillaLaunchScriptPath, allowUserInput);
                File.Move(paths.BackupLaunchScriptPath, paths.VanillaLaunchScriptPath);
            }

            // remove old files
            string[] removePaths = this.GetUninstallPaths(paths.GameDir, paths.ModsDir)
                .Where(path => Directory.Exists(path) || File.Exists(path))
                .ToArray();
            if (removePaths.Any())
            {
                this.PrintDebug(action == ScriptAction.Install ? "Removing previous SMAPI files..." : "Removing SMAPI files...");
                foreach (string path in removePaths)
                    this.InteractivelyDelete(path, allowUserInput);
            }

            // move global save data folder (changed in 3.2)
            {
                string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley");
                DirectoryInfo oldDir = new(Path.Combine(dataPath, "Saves", ".smapi"));
                DirectoryInfo newDir = new(Path.Combine(dataPath, ".smapi"));

                if (oldDir.Exists)
                {
                    if (newDir.Exists)
                        this.InteractivelyDelete(oldDir.FullName, allowUserInput);
                    else
                        oldDir.MoveTo(newDir.FullName);
                }
            }

            /****
            ** Install new files
            ****/
            if (action == ScriptAction.Install)
            {
                // copy SMAPI files to game dir
                this.PrintDebug("Adding SMAPI files...");
                foreach (FileSystemInfo sourceEntry in paths.BundleDir.EnumerateFileSystemInfos().Where(this.ShouldCopy))
                {
                    this.InteractivelyDelete(Path.Combine(paths.GameDir.FullName, sourceEntry.Name), allowUserInput);
                    this.RecursiveCopy(sourceEntry, paths.GameDir);
                }

                // replace mod launcher (if possible)
                if (context.IsUnix)
                {
                    this.PrintDebug("Safely replacing game launcher...");

                    // back up & remove current launcher
                    if (File.Exists(paths.VanillaLaunchScriptPath))
                    {
                        if (!File.Exists(paths.BackupLaunchScriptPath))
                            File.Move(paths.VanillaLaunchScriptPath, paths.BackupLaunchScriptPath);
                        else
                            this.InteractivelyDelete(paths.VanillaLaunchScriptPath, allowUserInput);
                    }

                    // add new launcher
                    File.Move(paths.NewLaunchScriptPath, paths.VanillaLaunchScriptPath);

                    // mark files executable
                    // (MSBuild doesn't keep permission flags for files zipped in a build task.)
                    foreach (string path in new[] { paths.VanillaLaunchScriptPath, paths.UnixSmapiExecutablePath })
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "chmod",
                                Arguments = $"755 \"{path}\"",
                                CreateNoWindow = true
                            }
                        }.Start();
                    }
                }

                // copy the game's deps.json file
                // (This is needed to resolve native DLLs like libSkiaSharp.)
                File.Copy(
                    sourceFileName: Path.Combine(paths.GamePath, "Stardew Valley.deps.json"),
                    destFileName: Path.Combine(paths.GamePath, "StardewModdingAPI.deps.json"),
                    overwrite: true
                );

                // create mods directory (if needed)
                if (!paths.ModsDir.Exists)
                {
                    this.PrintDebug("Creating mods directory...");
                    paths.ModsDir.Create();
                }

                // add or replace bundled mods
                DirectoryInfo bundledModsDir = new(Path.Combine(paths.BundlePath, "Mods"));
                if (bundledModsDir.Exists && bundledModsDir.EnumerateDirectories().Any())
                {
                    this.PrintDebug("Adding bundled mods...");

                    ModFolder[] targetMods = toolkit.GetModFolders(paths.ModsPath, useCaseInsensitiveFilePaths: true).ToArray();
                    foreach (ModFolder sourceMod in toolkit.GetModFolders(bundledModsDir.FullName, useCaseInsensitiveFilePaths: true))
                    {
                        // validate source mod
                        if (sourceMod.Manifest == null)
                        {
                            this.PrintWarning($"   ignored invalid bundled mod {sourceMod.DisplayName}: {sourceMod.ManifestParseError}");
                            continue;
                        }
                        if (!this.BundledModIds.Contains(sourceMod.Manifest.UniqueID))
                        {
                            this.PrintWarning($"   ignored unknown '{sourceMod.DisplayName}' mod in the installer folder. To add mods, put them here instead: {paths.ModsPath}");
                            continue;
                        }

                        // get mod info
                        string modId = sourceMod.Manifest.UniqueID;
                        string modName = sourceMod.Manifest.Name;
                        DirectoryInfo fromDir = sourceMod.Directory;

                        // get target path
                        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract -- avoid error if the Mods folder has invalid mods, since they're not validated yet
                        ModFolder? targetMod = targetMods.FirstOrDefault(p => p.Manifest?.UniqueID?.Equals(modId, StringComparison.OrdinalIgnoreCase) == true);
                        DirectoryInfo targetDir = new(Path.Combine(paths.ModsPath, fromDir.Name)); // replace existing folder if possible

                        // if we found the mod in a custom location, replace that copy
                        if (targetMod != null && targetMod.Directory.FullName != targetDir.FullName)
                        {
                            targetDir = targetMod.Directory;
                            DirectoryInfo parentDir = targetDir.Parent!;

                            this.PrintDebug($"   adding {modName} to {Path.Combine(paths.ModsDir.Name, PathUtilities.GetRelativePath(paths.ModsPath, targetDir.FullName))}...");

                            if (targetDir.Name != fromDir.Name)
                            {
                                // in case the user does weird things like swap folder names, rename the bundled
                                // mod in a unique staging folder within the temporary package:
                                string stagingPath = Path.Combine(fromDir.Parent!.Parent!.FullName, $"renamed-mod-{fromDir.Name}");
                                Directory.CreateDirectory(stagingPath);
                                fromDir.MoveTo(
                                    Path.Combine(stagingPath, targetDir.Name)
                                );
                            }

                            this.InteractivelyDelete(targetDir.FullName, allowUserInput);
                            this.RecursiveCopy(fromDir, parentDir, filter: this.ShouldCopy);
                        }

                        // else add it to default location
                        else
                        {
                            DirectoryInfo parentDir = targetDir.Parent!;

                            this.PrintDebug($"   adding {modName}...");

                            if (targetDir.Exists)
                                this.InteractivelyDelete(targetDir.FullName, allowUserInput);

                            this.RecursiveCopy(fromDir, parentDir, filter: this.ShouldCopy);
                        }
                    }
                }

                // set SMAPI's color scheme if defined
                if (scheme != MonitorColorScheme.AutoDetect)
                {
                    string text = File
                        .ReadAllText(paths.ApiConfigPath)
                        .Replace(@"""UseScheme"": ""AutoDetect""", $@"""UseScheme"": ""{scheme}""");
                    File.WriteAllText(paths.ApiConfigPath, text);
                }
            }
        }
        Console.WriteLine();
        Console.WriteLine();


        /*********
        ** Step 7: final instructions
        *********/
        if (context.IsWindows)
        {
            if (action == ScriptAction.Install)
            {
                this.PrintSuccess("SMAPI is installed! If you use Steam, set your launch options to enable achievements (see smapi.io/install):");
                this.PrintSuccess($"    \"{Path.Combine(paths.GamePath, "StardewModdingAPI.exe")}\" %command%");
                Console.WriteLine();
                this.PrintSuccess("If you don't use Steam, launch StardewModdingAPI.exe in your game folder to play with mods.");
            }
            else
                this.PrintSuccess("SMAPI is removed! If you configured Steam to launch SMAPI, don't forget to clear your launch options.");
        }
        else
        {
            this.PrintSuccess(action == ScriptAction.Install
                ? "SMAPI is installed! Launch the game the same way as before to play with mods."
                : "SMAPI is removed! Launch the game the same way as before to play without mods."
            );
        }

        this.AwaitConfirmation(allowUserInput);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the display text for a color scheme.</summary>
    /// <param name="scheme">The color scheme.</param>
    private string GetDisplayText(MonitorColorScheme scheme)
    {
        switch (scheme)
        {
            case MonitorColorScheme.AutoDetect:
                return "auto-detect";
            case MonitorColorScheme.DarkBackground:
                return "light text on dark background";
            case MonitorColorScheme.LightBackground:
                return "dark text on light background";
            default:
                return scheme.ToString();
        }
    }

    /// <summary>Print a message without formatting.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintPlain(string text)
    {
        Console.WriteLine(text);
    }

    /// <summary>Print a debug message.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintDebug(string text)
    {
        this.ConsoleWriter.WriteLine(text, ConsoleLogLevel.Debug);
    }

    /// <summary>Print a debug message.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintInfo(string text)
    {
        this.ConsoleWriter.WriteLine(text, ConsoleLogLevel.Info);
    }

    /// <summary>Print a warning message.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintWarning(string text)
    {
        this.ConsoleWriter.WriteLine(text, ConsoleLogLevel.Warn);
    }

    /// <summary>Print a warning message.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintError(string text)
    {
        this.ConsoleWriter.WriteLine(text, ConsoleLogLevel.Error);
    }

    /// <summary>Print a success message.</summary>
    /// <param name="text">The text to print.</param>
    private void PrintSuccess(string text)
    {
        this.ConsoleWriter.WriteLine(text, ConsoleLogLevel.Success);
    }

    /// <summary>Interactively delete a file or folder path, and block until deletion completes.</summary>
    /// <param name="path">The file or folder path.</param>
    /// <param name="allowUserInput">Whether the installer can ask for user input from the terminal.</param>
    private void InteractivelyDelete(string path, bool allowUserInput)
    {
        while (true)
        {
            try
            {
                FileUtilities.ForceDelete(Directory.Exists(path) ? new DirectoryInfo(path) : new FileInfo(path));
                break;
            }
            catch (Exception ex)
            {
                this.PrintError($"Oops! The installer couldn't delete {path}: [{ex.GetType().Name}] {ex.Message}.");
                this.PrintError("Try rebooting your computer and then run the installer again. If that doesn't work, try deleting it yourself then press any key to retry.");
                this.AwaitConfirmation(allowUserInput);
            }
        }
    }

    /// <summary>Recursively copy a directory or file.</summary>
    /// <param name="source">The file or folder to copy.</param>
    /// <param name="targetFolder">The folder to copy into.</param>
    /// <param name="filter">A filter which matches directories and files to copy, or <c>null</c> to match all.</param>
    private void RecursiveCopy(FileSystemInfo source, DirectoryInfo targetFolder, Func<FileSystemInfo, bool>? filter = null)
    {
        if (filter != null && !filter(source))
            return;

        if (!targetFolder.Exists)
            targetFolder.Create();

        switch (source)
        {
            case FileInfo sourceFile:
                sourceFile.CopyTo(Path.Combine(targetFolder.FullName, sourceFile.Name));
                break;

            case DirectoryInfo sourceDir:
                DirectoryInfo targetSubfolder = new(Path.Combine(targetFolder.FullName, sourceDir.Name));
                foreach (FileSystemInfo entry in sourceDir.EnumerateFileSystemInfos())
                    this.RecursiveCopy(entry, targetSubfolder, filter);
                break;

            default:
                throw new NotSupportedException($"Unknown filesystem info type '{source.GetType().FullName}'.");
        }
    }

    /// <summary>Interactively ask the user to choose a value.</summary>
    /// <param name="printLine">A callback which prints a message to the console.</param>
    /// <param name="message">The message to print.</param>
    /// <param name="options">The allowed options (not case sensitive).</param>
    /// <param name="indent">The indentation to prefix to output.</param>
    private string InteractivelyChoose(string message, string[] options, string indent = "", Action<string>? printLine = null)
    {
        printLine ??= this.PrintInfo;

        while (true)
        {
            printLine(indent + message);
            Console.Write(indent);
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input == null || !options.Contains(input))
            {
                printLine($"{indent}That's not a valid option.");
                continue;
            }
            return input;
        }
    }

    /// <summary>Interactively locate the game install path to update.</summary>
    /// <param name="toolkit">The mod toolkit.</param>
    /// <param name="context">The installer context.</param>
    /// <param name="specifiedPath">The path specified as a command-line argument (if any), which should override automatic path detection.</param>
    private DirectoryInfo? InteractivelyGetInstallPath(ModToolkit toolkit, InstallerContext context, string? specifiedPath)
    {
        // use specified path
        if (specifiedPath != null)
        {
            string errorPrefix = $"You specified --game-path \"{specifiedPath}\", but";

            var dir = new DirectoryInfo(specifiedPath);
            if (!dir.Exists)
            {
                this.PrintError($"{errorPrefix} that folder doesn't exist.");
                return null;
            }

            GameFolderType type = context.GetGameFolderType(dir);
            switch (type)
            {
                case GameFolderType.Valid:
                    return dir;

                default:
                    foreach (string message in this.GetInvalidFolderWarning(type))
                        this.PrintWarning(message);
                    return null;
            }
        }

        // get valid install paths & log invalid ones
        List<DirectoryInfo> defaultPaths = [];
        foreach ((DirectoryInfo dir, GameFolderType type) in this.DetectGameFolders(toolkit, context))
        {
            if (type is GameFolderType.Valid)
            {
                defaultPaths.Add(dir);
                continue;
            }

            this.PrintDebug($"Ignored game folder: {dir.FullName}");
            foreach (string message in this.GetInvalidFolderWarning(type))
                this.PrintDebug(message);
            this.PrintDebug("\n");
        }

        // let user choose detected path
        if (defaultPaths.Any())
        {
            this.PrintInfo("Where do you want to add or remove SMAPI?");
            Console.WriteLine();
            for (int i = 0; i < defaultPaths.Count; i++)
                this.PrintInfo($"[{i + 1}] {defaultPaths[i].FullName}");
            this.PrintInfo($"[{defaultPaths.Count + 1}] Enter a custom game path.");
            Console.WriteLine();

            string[] validOptions = Enumerable.Range(1, defaultPaths.Count + 1).Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray();
            string choice = this.InteractivelyChoose("Type the number next to your choice, then press enter.", validOptions);
            int index = int.Parse(choice, CultureInfo.InvariantCulture) - 1;

            if (index < defaultPaths.Count)
                return defaultPaths[index];
        }
        else
            this.PrintInfo("Oops, couldn't find the game automatically.");

        // let user enter manual path
        while (true)
        {
            // get path from user
            Console.WriteLine();
            this.PrintInfo($"Type the file path to the game directory (the one containing '{Constants.GameDllName}'), then press enter.");
            string? path = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                this.PrintWarning("You must specify a directory path to continue.");
                continue;
            }

            // normalize path
            path = context.IsWindows
                ? path.Replace("\"", "") // in Windows, quotes are used to escape spaces and aren't part of the file path
                : path.Replace("\\ ", " "); // in Linux/macOS, spaces in paths may be escaped if copied from the command line
            if (path.StartsWith("~/"))
            {
                string home = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE")!;
                path = Path.Combine(home, path.Substring(2));
            }

            // get directory
            DirectoryInfo directory = new(path);
            if (!directory.Exists && (path.EndsWith(".dll") || path.EndsWith(".exe") || File.Exists(path)) && directory.Parent is { Exists: true })
                directory = directory.Parent;

            // validate path
            if (!directory.Exists)
            {
                this.PrintWarning("That directory doesn't seem to exist.");
                continue;
            }

            GameFolderType type = context.GetGameFolderType(directory);
            switch (type)
            {
                case GameFolderType.Valid:
                    this.PrintInfo("   OK!");
                    return directory;

                default:
                    foreach (string message in this.GetInvalidFolderWarning(type))
                        this.PrintWarning(message);
                    continue;
            }
        }
    }

    /// <summary>Get the possible game paths to update.</summary>
    /// <param name="toolkit">The mod toolkit.</param>
    /// <param name="context">The installer context.</param>
    private IEnumerable<(DirectoryInfo, GameFolderType)> DetectGameFolders(ModToolkit toolkit, InstallerContext context)
    {
        HashSet<string> foundPaths = [];

        // game folder which contains the installer, if any
        {
            DirectoryInfo? curPath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (curPath?.Parent != null) // must be in a folder (not at the root)
            {
                if (context.GetGameFolderType(curPath) == GameFolderType.Valid)
                {
                    foundPaths.Add(curPath.FullName);
                    yield return (curPath, GameFolderType.Valid);
                    break;
                }

                curPath = curPath.Parent;
            }
        }

        // game paths detected by toolkit
        foreach ((DirectoryInfo, GameFolderType) pair in toolkit.GetGameFoldersIncludingInvalid())
        {
            if (foundPaths.Add(pair.Item1.FullName))
                yield return pair;
        }
    }

    private string[] GetInvalidFolderWarning(GameFolderType type)
    {
        switch (type)
        {
            case GameFolderType.Valid:
                return ["OK!"]; // should never happen

            case GameFolderType.LegacyVersion:
                return
                [
                    "That directory seems to have Stardew Valley 1.5.6 or earlier.",
                    "Please update your game to the latest version to use SMAPI."
                ];

            case GameFolderType.LegacyCompatibilityBranch:
                return
                [
                    "That directory seems to have the Stardew Valley legacy 'compatibility' branch.",
                    "Unfortunately SMAPI is only compatible with the modern version of the game.",
                    "Please update your game to the main branch to use SMAPI."
                ];

            case GameFolderType.NoGameFound:
                return ["That directory doesn't contain a Stardew Valley executable."];

            default:
                return ["That directory doesn't seem to contain a valid game install."];
        }
    }

    /// <summary>Get whether a file or folder should be copied from the installer files.</summary>
    /// <param name="entry">The file or folder info.</param>
    private bool ShouldCopy(FileSystemInfo entry)
    {
        return entry.Name switch
        {
            "mcs" => false, // ignore macOS symlink
            "Mods" => false, // Mods folder handled separately
            _ => true
        };
    }

    /// <summary>Wait until the user presses enter to confirm, if user input is allowed.</summary>
    /// <param name="allowUserInput">Whether the installer can ask for user input from the terminal.</param>
    private void AwaitConfirmation(bool allowUserInput)
    {
        if (allowUserInput)
            Console.ReadLine();
    }
}
