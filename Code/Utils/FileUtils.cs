using System.Diagnostics;
using System.Text.RegularExpressions;

namespace QArantineLauncher.Code.Utils
{
    public static class FileUtils
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            DirectoryInfo dir = new(sourceDir);

            if (!dir.Exists) throw new DirectoryNotFoundException($"The source directory to copy doesn't exist: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, recursive);
                }
            }
        }

        public static void CopyFilesMatchingRegex(string sourceDir, string destinationDir, List<string> regexPatterns)
        {
            List<Regex> regexPatternObjects = [];
            foreach (string regexPattern in regexPatterns)
            {
                regexPatternObjects.Add(new Regex(regexPattern.Replace("\\", "/").Replace("/", "[\\\\/]+"), RegexOptions.IgnoreCase));
            }

            Directory.CreateDirectory(destinationDir);

            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);

                foreach(Regex regexPattern in regexPatternObjects)
                {
                    if (regexPattern.IsMatch(relativePath))
                    {
                        string targetFilePath = Path.Combine(destinationDir, relativePath);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        File.Copy(file, targetFilePath, true);

                        break;
                    }
                }
            }
        }

        public static bool OpenDirectory(string directoryPath)
        {
            if(Directory.Exists(directoryPath))
            {
                Process.Start("explorer.exe", directoryPath);
                return true;
            }
            return false;
        }

        public static bool OpenFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                return true;
            }
            return false;
        }
    }
}