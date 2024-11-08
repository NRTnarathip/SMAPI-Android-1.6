adb shell am force-stop com.nrt.fakestardewgame

adb push "bin/ARM64/Android Debug/StardewModdingAPI.dll" "/storage/emulated/0/Android/data/com.nrt.fakestardewgame/files/StardewModdingAPI.dll"

adb shell am start com.nrt.fakestardewgame/crc641b6952874c843248.SMainActivity
