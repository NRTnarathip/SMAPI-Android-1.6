#
#
# This is the PowerShell equivalent of ../unix/prepare-install-package.sh, *except* that it doesn't
# set Linux permissions, create the install.dat files, or create the final zip (unless you specify
# --windows-only). Due to limitations in PowerShell, the final changes are handled by the
# windows/finalize-install-package.sh file in WSL.
#
# When making changes, make sure to update ../unix/prepare-install-package.ps1 too.
#
#

. "$PSScriptRoot/lib/in-place-regex.ps1"


##########
## Fetch values
##########
# paths
$gamePath = "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley"
$bundleModNames = "ConsoleCommands", "SaveBackup"

# build configuration
$buildConfig = "Release"
$framework = "net6.0"
$folders = "linux", "macOS", "windows"
$runtimes = @{ linux = "linux-x64"; macOS = "osx-x64"; windows = "win-x64" }
$msBuildPlatformNames = @{ linux = "Unix"; macOS = "OSX"; windows = "Windows_NT" }

# version number
$version = $args[0]
if (!$version) {
    $version = Read-Host "SMAPI release version (like '4.0.0')"
}

# Windows-only build
$windowsOnly = $false
foreach ($arg in $args) {
    if ($arg -eq "--windows-only") {
        $windowsOnly = $true
        $folders = "windows"
        $runtimes = @{ windows = "win-x64" }
        $msBuildPlatformNames = @{ windows = "Windows_NT" }
    }
}


##########
## Move to SMAPI root
##########
cd "$PSScriptRoot/../.."


##########
## Clear old build files
##########
echo "Clearing old builds..."
echo "-------------------------------------------------"

foreach ($path in (dir -Recurse -Include ('bin', 'obj'))) {
    echo "$path"
    rm -Recurse -Force "$path"
}
echo ""


##########
## Compile files
##########
. "$PSScriptRoot/set-smapi-version.ps1" "$version"
foreach ($folder in $folders) {
    $runtime = $runtimes[$folder]
    $msbuildPlatformName = $msBuildPlatformNames[$folder]

    echo "Compiling SMAPI for $folder..."
    echo "-------------------------------------------------"
    dotnet publish src/SMAPI --configuration $buildConfig -v minimal --runtime "$runtime" --framework "$framework" -p:OS="$msbuildPlatformName" -p:TargetFrameworks="$framework" -p:GamePath="$gamePath" -p:CopyToGameFolder="false" --self-contained true
    echo ""
    echo ""

    echo "Compiling installer for $folder..."
    echo "-------------------------------------------------"
    dotnet publish src/SMAPI.Installer --configuration $buildConfig -v minimal --runtime "$runtime" --framework "$framework" -p:OS="$msbuildPlatformName" -p:TargetFrameworks="$framework" -p:GamePath="$gamePath" -p:CopyToGameFolder="false" --self-contained true
    echo ""
    echo ""

    foreach ($modName in $bundleModNames) {
        echo "Compiling $modName for $folder..."
        echo "-------------------------------------------------"
        dotnet publish src/SMAPI.Mods.$modName --configuration $buildConfig -v minimal --runtime "$runtime" --framework "$framework" -p:OS="$msbuildPlatformName" -p:TargetFrameworks="$framework" -p:GamePath="$gamePath" -p:CopyToGameFolder="false" --self-contained false
        echo ""
        echo ""
    }
}


##########
## Prepare install package
##########
echo "Preparing install package..."
echo "----------------------------"

# init paths
$installAssets = "src/SMAPI.Installer/assets"
$packagePath = "bin/SMAPI installer"
$packageDevPath = "bin/SMAPI installer for developers"

# init structure
foreach ($folder in $folders) {
    mkdir "$packagePath/internal/$folder/bundle/smapi-internal" > $null
}

# copy base installer files
foreach ($name in @("install on Linux.sh", "install on macOS.command", "install on Windows.bat", "README.txt")) {
    if ($windowsOnly -and ($name -eq "install on Linux.sh" -or $name -eq "install on macOS.command")) {
        continue;
    }

    cp "$installAssets/$name" "$packagePath"
}

# copy per-platform files
foreach ($folder in $folders) {
    $runtime = $runtimes[$folder]

    # get paths
    $smapiBin = "src/SMAPI/bin/$buildConfig/$runtime/publish"
    $internalPath = "$packagePath/internal/$folder"
    $bundlePath = "$internalPath/bundle"

    # installer files
    cp "src/SMAPI.Installer/bin/$buildConfig/$runtime/publish/*" "$internalPath" -Recurse
    rm -Recurse -Force "$internalPath/assets"

    # runtime config for SMAPI
    # This is identical to the one generated by the build, except that the min runtime version is
    # set to 5.0.0 (instead of whatever version it was built with) and rollForward is set to latestMinor instead of
    # minor.
    cp "$installAssets/runtimeconfig.json" "$bundlePath/StardewModdingAPI.runtimeconfig.json"

    # installer DLL config
    if ($folder -eq "windows") {
        cp "$installAssets/windows-exe-config.xml" "$packagePath/internal/windows/install.exe.config"
    }

    # bundle root files
    foreach ($name in @("StardewModdingAPI", "StardewModdingAPI.dll", "StardewModdingAPI.xml", "steam_appid.txt")) {
        if ($name -eq "StardewModdingAPI" -and $folder -eq "windows") {
            $name = "$name.exe"
        }

        cp "$smapiBin/$name" "$bundlePath"
    }

    # bundle i18n
    cp -Recurse "$smapiBin/i18n" "$bundlePath/smapi-internal"

    # bundle smapi-internal
    foreach ($name in @("0Harmony.dll", "0Harmony.xml", "Markdig.dll", "Mono.Cecil.dll", "Mono.Cecil.Mdb.dll", "Mono.Cecil.Pdb.dll", "MonoMod.Common.dll", "Newtonsoft.Json.dll", "Pathoschild.Http.Client.dll", "Pintail.dll", "TMXTile.dll", "SMAPI.Toolkit.dll", "SMAPI.Toolkit.xml", "SMAPI.Toolkit.CoreInterfaces.dll", "SMAPI.Toolkit.CoreInterfaces.xml", "System.Net.Http.Formatting.dll")) {
        cp "$smapiBin/$name" "$bundlePath/smapi-internal"
    }

    if ($folder -eq "windows") {
        cp "$smapiBin/VdfConverter.dll" "$bundlePath/smapi-internal"
    }

    cp "$smapiBin/SMAPI.config.json" "$bundlePath/smapi-internal/config.json"
    cp "$smapiBin/SMAPI.metadata.json" "$bundlePath/smapi-internal/metadata.json"
    if ($folder -eq "linux" -or $folder -eq "macOS") {
        cp "$installAssets/unix-launcher.sh" "$bundlePath"
    }
    else {
        cp "$installAssets/windows-exe-config.xml" "$bundlePath/StardewModdingAPI.exe.config"
    }

    # copy .NET dependencies
    if ($folder -eq "windows") {
        cp "$smapiBin/System.Management.dll" "$bundlePath/smapi-internal"
    }

    # copy bundled mods
    foreach ($modName in $bundleModNames) {
        $fromPath = "src/SMAPI.Mods.$modName/bin/$buildConfig/$runtime/publish"
        $targetPath = "$bundlePath/Mods/$modName"

        mkdir "$targetPath" > $null

        cp "$fromPath/$modName.dll" "$targetPath"
        cp "$fromPath/manifest.json" "$targetPath"
        if (Test-Path "$fromPath/i18n" -PathType Container) {
            cp -Recurse "$fromPath/i18n" "$targetPath"
        }
    }
}

# DISABLED: will be handled by Linux script
# mark scripts executable
#ForEach ($path in @("install on Linux.sh", "install on macOS.command", "bundle/unix-launcher.sh")) {
#    if (Test-Path "$packagePath/$path" -PathType Leaf) {
#        chmod 755 "$packagePath/$path"
#    }
#}

# split into main + for-dev folders
cp -Recurse "$packagePath" "$packageDevPath"
foreach ($folder in $folders) {
    # disable developer mode in main package
    In-Place-Regex -Path "$packagePath/internal/$folder/bundle/smapi-internal/config.json" -Search "`"DeveloperMode`": true" -Replace "`"DeveloperMode`": false"

    # convert bundle folder into final 'install.dat' files
    if ($windowsOnly)
    {
        foreach ($path in @("$packagePath/internal/$folder", "$packageDevPath/internal/$folder"))
        {
            Compress-Archive -Path "$path/bundle/*" -CompressionLevel Optimal -DestinationPath "$path/install.zip"
            mv "$path/install.zip" "$path/install.dat"
            rm -Recurse -Force "$path/bundle"
        }
    }
}


###########
### Create release zips
###########
# rename folders
mv "$packagePath" "bin/SMAPI $version installer"
mv "$packageDevPath" "bin/SMAPI $version installer for developers"

# package files
if ($windowsOnly)
{
    Compress-Archive -Path "bin/SMAPI $version installer" -DestinationPath "bin/SMAPI $version installer.zip" -CompressionLevel Optimal
    Compress-Archive -Path "bin/SMAPI $version installer for developers" -DestinationPath "bin/SMAPI $version installer for developers.zip" -CompressionLevel Optimal
}

echo ""
echo "Done! See docs/technical/smapi.md to create the release zips."
