&larr; [mod build config](./mod-build-config.md)

## Release notes
# 4.3.2
Released 09 November 2024 for SMAPI 4.1.0 or later.

* Fixed `IgnoreModFilePaths` and `IgnoreModFilePatterns` not working correctly in recent versions.

## 4.3.1
Released 04 November 2024 for SMAPI 4.1.0 or later.

* Removed build warnings for implicit net field conversions, which were removed in Stardew Valley 1.6.9.
* Fixed zip structure on Linux/macOS after 4.3.0.

## 4.3.0
Released 06 October 2024 for SMAPI 3.13.0 or later.

* You can now [bundle content packs with your mod](mod-package.md#bundle-content-packs) (thanks to abhiaagarwal!).

### 4.2.0
Released 05 September 2024 for SMAPI 3.13.0 or later.

* Added support for `i18n` subfolders (thanks to spacechase0!).
* Updated dependencies.

### 4.1.1
Released 24 June 2023 for SMAPI 3.13.0 or later.

* Replaced `.pdb` files with embedded symbols by default. This fixes logged errors not having line numbers on Linux/macOS.

### 4.1.0
Released 08 January 2023 for SMAPI 3.13.0 or later.

* Added `manifest.json` format validation on build (thanks to tylergibbs2!).
* Fixed game DLLs not excluded from the release zip when they're referenced explicitly but `BundleExtraAssemblies` isn't set.

### 4.0.2
Released 09 October 2022 for SMAPI 3.13.0 or later.

* Switched to the newer crossplatform `portable` debug symbols (thanks to lanturnalis!).
* Fixed `BundleExtraAssemblies` option being partly case-sensitive.
* Fixed `BundleExtraAssemblies` not applying `All` value to game assemblies.

### 4.0.1
Released 14 April 2022 for SMAPI 3.13.0 or later.

* Added detection for Xbox app game folders.
* Fixed "_conflicts between different versions of Microsoft.Win32.Registry_" warnings in recent SMAPI versions.
* Internal refactoring.

### 4.0.0
Released 30 November 2021 for SMAPI 3.13.0 or later.

* Updated for Stardew Valley 1.5.5 and SMAPI 3.13.0. (Older versions are no longer supported.)
* Added `IgnoreModFilePaths` option to ignore literal paths.
* Added `BundleExtraAssemblies` option to copy bundled DLLs into the mod zip/folder.
* Removed the `GameExecutableName` and `GameFramework` options (since they now have the same value
  on all platforms).
* Removed the `CopyModReferencesToBuildOutput` option (superseded by `BundleExtraAssemblies`).
* Improved analyzer performance by enabling parallel execution.

**Migration guide for mod authors:**
1. See [_migrate to 64-bit_](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows) and
   [_migrate to Stardew Valley 1.5.5_](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.5.5).
2. Possible changes in your `.csproj` or `.targets` files:
   * Replace `$(GameExecutableName)` with `Stardew Valley`.
   * Replace `$(GameFramework)` with `MonoGame` and remove any XNA Framework-specific logic.
   * Replace `<CopyModReferencesToBuildOutput>true</CopyModReferencesToBuildOutput>` with
     `<BundleExtraAssemblies>Game</BundleExtraAssemblies>`.
   * If you need to bundle extra DLLs besides your mod DLL, see the [`BundleExtraAssemblies`
     documentation](#configure).

### 3.3.0
Released 30 March 2021 for SMAPI 3.0.0 or later.

* Added a build warning when the mod isn't compiled for `Any CPU`.
* Added a `GameFramework` build property set to `MonoGame` or `Xna` based on the platform. This can
  be overridden to change which framework it references.
* Added support for building mods against the 64-bit Linux version of the game on Windows.
* The package now suppresses the misleading 'processor architecture mismatch' warnings.

### 3.2.2
Released 23 September 2020 for SMAPI 3.0.0 or later.

* Reworked and streamlined how the package is compiled.
* Added [SMAPI-ModTranslationClassBuilder](https://github.com/Pathoschild/SMAPI-ModTranslationClassBuilder)
  files to the ignore list.

### 3.2.1
Released 11 September 2020 for SMAPI 3.0.0 or later.

* Added more detailed logging.
* Fixed _path's format is not supported_ error when using default `Mods` path in 3.2.

### 3.2.0
Released 07 September 2020 for SMAPI 3.0.0 or later.

* Added option to change `Mods` folder path.
* Rewrote documentation to make it easier to read.

### 3.1.0
Released 01 February 2020 for SMAPI 3.0.0 or later.

* Added support for semantic versioning 2.0.
* `0Harmony.dll` is now ignored if the mod references Harmony directly (it's bundled with SMAPI).

### 3.0.0
Released 26 November 2019 for SMAPI 3.0.0 or later.

* Updated for SMAPI 3.0 and Stardew Valley 1.4.
* Added automatic support for `assets` folders.
* Added `$(GameExecutableName)` MSBuild variable.
* Added support for projects using the simplified `.csproj` format.
* Added option to disable game debugging config.
* Added `.pdb` files to builds by default (to enable line numbers in error stack traces).
* Added optional Harmony reference.
* Fixed `Newtonsoft.Json.pdb` included in release zips when Json.NET is referenced directly.
* Fixed `<IgnoreModFilePatterns>` not working for `i18n` files.
* Dropped support for older versions of SMAPI and Visual Studio.
* Migrated package icon to NuGet's new format.

### 2.2.0
Released 28 October 2018.

* Added support for SMAPI 2.8+ (still compatible with earlier versions).
* Added default game paths for 32-bit Windows.
* Fixed valid manifests marked invalid in some cases.

### 2.1.0
Released 27 July 2018.

* Added support for Stardew Valley 1.3.
* Added support for non-mod projects.
* Added C# analyzers to warn about implicit conversions of Netcode fields in Stardew Valley 1.3.
* Added option to ignore files by regex pattern.
* Added reference to new SMAPI DLL.
* Fixed some game paths not detected by NuGet package.

### 2.0.2
Released 01 November 2017.

* Fixed compatibility issue on Linux.

### 2.0.1
Released 11 October 2017.

* Fixed mod deploy failing to create subfolders if they don't already exist.

### 2.0.0
Released 11 October 2017.

* Added: mods are now copied into the `Mods` folder automatically (configurable).
* Added: release zips are now created automatically in your build output folder (configurable).
* Added: mod deploy and release zips now exclude Json.NET automatically, since it's provided by SMAPI.
* Added mod's version to release zip filename.
* Improved errors to simplify troubleshooting.
* Fixed release zip not having a mod folder.
* Fixed release zip failing if mod name contains characters that aren't valid in a filename.

### 1.7.1
Released 28 July 2017.

* Fixed issue where i18n folders were flattened.
* The manifest/i18n files in the project now take precedence over those in the build output if both
  are present.

### 1.7.0
Released 28 July 2017.

* Added option to create release zips on build.
* Added reference to XNA's XACT library for audio-related mods.

### 1.6.2
Released 10 July 2017.

* Further improved crossplatform game path detection.
* Removed undocumented `GamePlatform` build property.

### 1.6.1
Released 09 July 2017.

* Improved crossplatform game path detection.

### 1.6.0
Released 05 June 2017.

* Added support for deploying mod files into `Mods` automatically.
* Added a build error if a game folder is found, but doesn't contain Stardew Valley or SMAPI.

### 1.5.0
Released 23 January 2017.

* Added support for setting a custom game path globally.
* Added default GOG path on macOS.

### 1.4.0
Released 11 January 2017.

* Fixed detection of non-default game paths on 32-bit Windows.
* Removed support for SilVerPLuM (discontinued).
* Removed support for overriding the target platform (no longer needed since SMAPI crossplatforms
  mods automatically).

### 1.3.0
Released 31 December 2016.

* Added support for non-default game paths on Windows.

### 1.2.0
Released 24 October 2016.

* Exclude game binaries from mod build output.

### 1.1.0
Released 21 October 2016.

* Added support for overriding the target platform.

### 1.0.0
Released 21 October 2016.

* Initial release.
* Added support for detecting the game path automatically.
* Added support for injecting XNA/MonoGame references automatically based on the OS.
* Added support for mod builders like SilVerPLuM.
