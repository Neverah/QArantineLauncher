using System.Diagnostics;
using System.Text.RegularExpressions;

using QArantineLauncher.Code.LauncherGUI.StaticData;
using QArantineLauncher.Code.LauncherGUI.Models;
using QArantineLauncher.Code.Utils;

namespace QArantineLauncher.Code.Projects.Building
{
    public partial class ProjectBuilder 
    (
        string projectRootPath,
        string buildOutputPath,
        string additionalCopyFiles
    )
    {
        public static readonly string ExpectedBasePath = "bin/Debug/net8.0";
        public string ProjectRootPath { get; set; } = projectRootPath;
        public string BuildOutputPath { get; set; } = buildOutputPath;
        public string AdditionalCopyFiles { get; set; } = additionalCopyFiles;
        public bool IsBuilding = false;
        public bool WasLastProcessSuccesfull = false;
        public event EventHandler? BuildStarted;
        public event EventHandler? BuildEnded;
        public event EventHandler? LogLineAdded;
        public event EventHandler? LogLinesClear;
        public readonly Mutex LogMutex = new();
        private Process? currentProcess;

        public async Task StartBuilding()
        {
            IsBuilding = true;

            BuildStarted?.Invoke(this, EventArgs.Empty);

            CleanLogLines();

            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = "build /p:CustomIdentifier=QArantineProjectBuilder",
                WorkingDirectory = ProjectRootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            currentProcess = new()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            currentProcess.OutputDataReceived += (sender, e) => AddLogLineWithTimestamp(e.Data, LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
            currentProcess.ErrorDataReceived += (sender, e) => AddLogLineWithTimestamp(e.Data, LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);

            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.BeginErrorReadLine();

            await currentProcess.WaitForExitAsync();

            WasLastProcessSuccesfull = currentProcess.ExitCode == 0;

            OutputCopy();

            if (WasLastProcessSuccesfull)
            {
                AddLogLineWithTimestamp($"Build completed successfully", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.OK]);
            }
            else
            {
                AddLogLineWithTimestamp($"Build failed with exit code: {currentProcess.ExitCode}", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);
            }

            EndBuilding();
        }

        private void OutputCopy()
        {
            if(WasLastProcessSuccesfull && !string.IsNullOrWhiteSpace(BuildOutputPath))
            {
                try
                {
                    if (Directory.Exists(BuildOutputPath)) Directory.Delete(BuildOutputPath, true);
                    FileUtils.CopyDirectory(Path.Combine(ProjectRootPath, ExpectedBasePath), BuildOutputPath);
                    FileUtils.CopyFilesMatchingRegex(ProjectRootPath, BuildOutputPath, SplitRegexPatterns(AdditionalCopyFiles));
                    AddLogLineWithTimestamp($"Files copied to the output directory", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
                }
                catch (Exception e)
                {
                    AddLogLineWithTimestamp($"Error detected when trying to copy the output directory:\n{e.Message}", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);
                    WasLastProcessSuccesfull = false;
                }
            }
        }

        private void EndBuilding()
        {
            AddLogLineWithTimestamp("BUILD PROCESS FINISHED", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
            IsBuilding = false;
            BuildEnded?.Invoke(this, EventArgs.Empty);
        }

        public void AbortBuilding()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
            }
            IsBuilding = false;
        }

        private void AddLogLineWithTimestamp(string? logBody, string logBodyForegroundHexCode)
        {
            LogLineAdded?.Invoke(this, new BufferLogLine(DateTime.Now.ToString("HH:mm:ss"), "#A0A0A0", "", "#A064DC", logBody ?? "", logBodyForegroundHexCode));
        }

        public void CleanLogLines()
        {
            LogLinesClear?.Invoke(this, EventArgs.Empty);
        }

        [GeneratedRegex(@"(?<!\\);")]
        private static partial Regex MyRegex();
        private static List<string> SplitRegexPatterns(string patterns)
        {
            string[] patternArray = MyRegex().Split(patterns);

            for (int i = 0; i < patternArray.Length; i++)
            {
                patternArray[i] = patternArray[i].Replace(@"\;", ";");
            }

            return new List<string>(patternArray);
        }
    }
}