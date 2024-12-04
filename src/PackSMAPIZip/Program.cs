using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using Mono.Cecil;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;

internal class Program
{
    const string StardewModdingAPIFileName = "StardewModdingAPI.dll";

    static string GetSMAPIVersion(string dllFilePath)
    {
        var assembly = AssemblyDefinition.ReadAssembly(dllFilePath);
        var constantsType = assembly.MainModule.Types.Single(t => t.FullName == "StardewModdingAPI.EarlyConstants");
        var RawApiVersionForAndroidField = constantsType.Fields.Single(p => p.Name == "RawApiVersionForAndroid");
        string version = RawApiVersionForAndroidField.Constant as string;
        return version;
    }

    private static void Main(string[] args)
    {

        //Create Folder SMAPI-x.x.x.x
        string SMAPIBinDir = GetParentDirectory(Directory.GetCurrentDirectory(), 4);
        SMAPIBinDir = Path.Combine(SMAPIBinDir, "SMAPI/bin/ARM64/Android Release");

        string SMAPIVersionName = GetSMAPIVersion(Path.Combine(SMAPIBinDir, StardewModdingAPIFileName)).ToString();
        string PackFolderName = $"SMAPI-{SMAPIVersionName}";
        Console.WriteLine("Start Pack: " + PackFolderName);
        string smapiOutputDir = Path.Combine(Directory.GetCurrentDirectory(), PackFolderName);
        if (Directory.Exists(smapiOutputDir))
            Directory.Delete(smapiOutputDir, true);
        Directory.CreateDirectory(smapiOutputDir);

        //clone dll files
        string[] dependencies = File.ReadAllLines("dependencies.txt");
        foreach (string dllFileName in dependencies)
        {
            string srcPath = Path.Combine(SMAPIBinDir, dllFileName);
            string destPath = Path.Combine(smapiOutputDir, dllFileName);
            Console.WriteLine("try add: " + srcPath);
            File.Copy(srcPath, destPath, true);
        }

        //build smapi-internal folder
        Console.WriteLine("try build smapi-internal folder");
        string smapiInternalDir = Path.Combine(smapiOutputDir, "smapi-internal");
        Directory.CreateDirectory(smapiInternalDir);
        CloneDirectory(Path.Combine(SMAPIBinDir, "i18n"), Path.Combine(smapiInternalDir, "i18n"));
        File.Copy(Path.Combine(SMAPIBinDir, "SMAPI.config.json"), Path.Combine(smapiInternalDir, "config.json"));
        File.Copy(Path.Combine(SMAPIBinDir, "SMAPI.metadata.json"), Path.Combine(smapiInternalDir, "metadata.json"));
        Console.WriteLine("done added smapi-internal");


        //Pack SMAPI-x.x.x.x.zip from directory SMAPI-x.x.x.x
        string outputZipFilePath = Path.Combine(Directory.GetCurrentDirectory(), PackFolderName + ".zip");
        Console.WriteLine("try pack SMPAI.zip output at " + outputZipFilePath);
        using var zipStream = File.Open(outputZipFilePath, FileMode.Create, FileAccess.ReadWrite);
        ZipFile.CreateFromDirectory(smapiOutputDir, zipStream, CompressionLevel.SmallestSize, true);
        Console.WriteLine("done pack & file size: " + zipStream.Length);
        zipStream.Close();

        //clean up
        Directory.Delete(smapiOutputDir, true);
        Console.WriteLine("done delete folder: " + smapiOutputDir);

        Console.WriteLine("Successfully Pack SMAPI Zip");
        Console.WriteLine("result file: " + new FileInfo(outputZipFilePath).Name);

        Console.ReadLine();
    }
    static string GetParentDirectory(string currentDir, int levelsUp)
    {
        string targetDir = currentDir;

        for (int i = 0; i < levelsUp; i++)
        {
            targetDir = Directory.GetParent(targetDir)?.FullName;
            if (targetDir == null)
                throw new InvalidOperationException("limit directory levels up");
        }

        return targetDir;
    }
    static void CloneDirectory(string sourceDir, string destDir)
    {
        // Ensure destination directory exists
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // Copy all files in the source directory to the destination directory
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true); // 'true' allows overwriting existing files
        }

        // Recursively copy subdirectories
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            CloneDirectory(subDir, destSubDir);
        }
    }
}
