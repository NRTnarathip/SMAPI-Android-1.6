
adb logcat -c
set AppName=abc.smapi.gameloader

adb shell am force-stop %AppName%

adb push "bin/ARM64/Android Release/StardewModdingAPI.dll" "/storage/emulated/0/Android/data/%AppName%/files/Stardew Assemblies/StardewModdingAPI.dll"

adb shell am start %AppName%"/crc64e91f1276c636690c.LauncherActivity"
