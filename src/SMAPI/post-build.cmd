
set AppName=abc.smapi.gameloader

adb shell am force-stop %AppName%

adb push "bin/ARM64/Android Release/StardewModdingAPI.dll" "/storage/emulated/0/Android/data/"%AppName%"/files/StardewModdingAPI.dll"

adb shell am start %AppName%"/crc644389b739a03c2b33.EntryActivity"
