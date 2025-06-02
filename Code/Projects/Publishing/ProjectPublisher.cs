using System.Diagnostics;
using System.Text.RegularExpressions;

using QArantineLauncher.Code.LauncherGUI.Models;
using QArantineLauncher.Code.LauncherGUI.StaticData;
using QArantineLauncher.Code.Utils;

namespace QArantineLauncher.Code.Projects.Publishing
{
    public partial class ProjectPublisher 
    (
        string projectRootPath,
        string publishingOutputPath,
        string additionalCopyFiles,
        string ignoredCopyFiles
    )
    {
        public static readonly string ExpectedBasePath = "bin/Release/net8.0/win-x64/publish";
        public string ProjectRootPath { get; set; } = projectRootPath;
        public string PublishingOutputPath { get; set; } = publishingOutputPath;
        public string AdditionalCopyFiles { get; set; } = additionalCopyFiles;
        public string IgnoredCopyFiles { get; set; } = ignoredCopyFiles;
        public bool IsPublishing = false;
        public bool WasLastProcessSuccesfull = false;
        public event EventHandler? PublishingStarted;
        public event EventHandler? PublishingEnded;
        public event EventHandler? LogLineAdded;
        public event EventHandler? LogLinesClear;
        public readonly Mutex LogMutex = new();
        private Process? currentProcess;

        public async Task StartPublishing(bool useTrimming)
        {
            IsPublishing = true;

            PublishingStarted?.Invoke(this, EventArgs.Empty);

            CleanLogLines();

            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = "publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:CustomIdentifier=QArantineProjectPublisher" + (useTrimming ? " /p:PublishTrimmed=true /p:TrimMode=copyused" : ""),
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
                AddLogLineWithTimestamp($"Publishing completed successfully", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.OK]);
            }
            else
            {
                AddLogLineWithTimestamp($"Publishing failed with exit code: {currentProcess.ExitCode}", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);
            }

            EndPublishing();
        }

        private void OutputCopy()
        {
            if(WasLastProcessSuccesfull && !string.IsNullOrWhiteSpace(PublishingOutputPath))
            {
                try
                {
                    if (Directory.Exists(PublishingOutputPath)) Directory.Delete(PublishingOutputPath, true);
                    FileUtils.CopyDirectory(Path.Combine(ProjectRootPath, ExpectedBasePath), PublishingOutputPath);
                    FileUtils.CopyFilesMatchingRegex(ProjectRootPath, PublishingOutputPath, SplitRegexPatterns(AdditionalCopyFiles), SplitRegexPatterns(IgnoredCopyFiles));
                    AddLogLineWithTimestamp($"Files copied to the output directory", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
                }
                catch (Exception e)
                {
                    AddLogLineWithTimestamp($"Error detected when trying to copy the output directory:\n{e.Message}", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);
                    WasLastProcessSuccesfull = false;
                }
            }
        }

        private void EndPublishing()
        {
            AddLogLineWithTimestamp("PUBLISHING PROCESS FINISHED", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
            IsPublishing = false;
            PublishingEnded?.Invoke(this, EventArgs.Empty);
        }

        public void AbortPublishing()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
            }
            IsPublishing = false;
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