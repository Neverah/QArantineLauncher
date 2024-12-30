using System.Diagnostics;

using QArantineLauncher.Code.LauncherGUI.Models;
using QArantineLauncher.Code.LauncherGUI.StaticData;

namespace QArantineLauncher.Code.Projects.Cleaning
{
    public class ProjectCleaner 
    (
        string projectRootPath
    )
    {
        public string ProjectRootPath { get; set; } = projectRootPath;
        public bool IsCleaning = false;
        public bool WasLastProcessSuccesfull = false;
        public List<BufferLogLine> CleaningLogLines = []; 
        public event EventHandler? CleaningStarted;
        public event EventHandler? CleaningEnded;
        public event EventHandler? LogLineAdded;
        public event EventHandler? LogLinesClear;
        public readonly Mutex LogMutex = new();
        private Process? currentProcess;

        public async Task StartCleaning()
        {
            IsCleaning = true;
            
            CleaningStarted?.Invoke(this, EventArgs.Empty);

            CleanLogLines();

            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = "clean /p:CustomIdentifier=QArantineProjectCleaner",
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
            if (WasLastProcessSuccesfull)
            {
                AddLogLineWithTimestamp($"Cleaning completed successfully", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.OK]);
            }
            else
            {
                AddLogLineWithTimestamp($"Cleaning failed with exit code: {currentProcess.ExitCode}", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Error]);
            }

            EndCleaning();
        }

        private void EndCleaning()
        {
            AddLogLineWithTimestamp("CLEANING PROCESS FINISHED", LogColorsDictionary.LogColorMap[LogColorsDictionary.LogLevel.Debug]);
            IsCleaning = false;
            CleaningEnded?.Invoke(this, EventArgs.Empty);
        }

        public void AbortCleaning()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
            }
            IsCleaning = false;
        }

        private void AddLogLineWithTimestamp(string? logBody, string logBodyForegroundHexCode)
        {
            LogMutex.WaitOne();
            BufferLogLine newLine = new(DateTime.Now.ToString("HH:mm:ss"), "#A0A0A0", "", "#A064DC", logBody ?? "", logBodyForegroundHexCode);
            CleaningLogLines.Add(newLine);
            LogMutex.ReleaseMutex();

            LogLineAdded?.Invoke(this, newLine);
        }

        public void CleanLogLines()
        {
            CleaningLogLines.Clear();
            LogLinesClear?.Invoke(this, EventArgs.Empty);
        }
    }
}