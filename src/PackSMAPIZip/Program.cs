using System.IO.Compression;
using System.Net;

internal class Program
{
    const string PackFolderName = "SMAPI Android";
    const string PackFileName = PackFolderName + ".zip";
    private static void Main(string[] args)
    {

        string SMAPIBinDir = GetParentDirectory(Directory.GetCurrentDirectory(), 4);
        SMAPIBinDir = Path.Combine(SMAPIBinDir, "SMAPI/bin/ARM64/Android Release");
        Console.WriteLine(SMAPIBinDir);
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


        string outputZipFilePath = Path.Combine(Directory.GetCurrentDirectory(), PackFileName);
        Console.WriteLine("try pack SMPAI.zip output at " + outputZipFilePath);
        using var zipStream = File.Open(outputZipFilePath, FileMode.Create, FileAccess.ReadWrite);
        ZipFile.CreateFromDirectory(smapiOutputDir, zipStream, CompressionLevel.SmallestSize, true);
        Console.WriteLine("done pack & file size: " + zipStream.Length);
        zipStream.Close();

        Console.ReadLine();
    }
    static string GetParentDirectory(string currentDir, int levelsUp)
    {
        string targetDir = currentDir;

        for (int i = 0; i < levelsUp; i++)
        {
            // ใช้ Directory.GetParent เพื่อย้อนกลับไป 1 ชั้น
            targetDir = Directory.GetParent(targetDir)?.FullName;

            if (targetDir == null)
            {
                throw new InvalidOperationException("ไม่สามารถย้อนกลับไปได้เนื่องจากถึง root directory แล้ว");
            }
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
