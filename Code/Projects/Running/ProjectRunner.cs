using System.Collections.ObjectModel;
using System.Diagnostics;

using QArantineLauncher.Code.LauncherGUI.Views;
using QArantineLauncher.Code.Projects.Testing;

namespace QArantineLauncher.Code.Projects.Running
{
    public class ProjectRunner
    (
        string buildExePath,
        string publishingExePath,
        string lastExecutionRootPath,
        string runParams
    )
    {
        public enum ExeType
        {
            None,
            Build,
            Publish,
        }

        public string BuildExePath { get; set; } = buildExePath;
        public string PublishingExePath { get; set; } = publishingExePath;
        public string LastExecutionRootPath { get; set; } = lastExecutionRootPath;
        public string RunParams { get; set; } = runParams;
        public bool WasLastProcessSuccesfull = false;
        private Process? currentProcess;
        private readonly ObservableCollection<ProcessInfo> aliveProcesses = [];

        public async Task RunProgram(ExeType exeType, bool waitForExit = false, bool keepCmdOpen = false, string testName = "")
        {
            string exePath;
            if (exeType == ExeType.Build) exePath = BuildExePath;
            else if (exeType == ExeType.Publish) exePath = PublishingExePath;
            else { await MessageBox.Show($"Invalid exeType enum value '{exeType}' provided to run the current project", LogManager.LogLevel.Error); return; };

            LastExecutionRootPath = Path.GetDirectoryName(exePath) ?? "";

            if (string.IsNullOrEmpty(exePath))
            {
                await MessageBox.Show("The current project exe path is null or empty. Please, try to build or publish the project first to generate some exe file", LogManager.LogLevel.Error);
                return;
            }

            string runParams = RunParams;
            runParams = runParams.Replace(ProjectTester.TestNameKeyword, testName);

            string fileName;
            string arguments;
            if (keepCmdOpen)
            {
                fileName = "cmd.exe";
                arguments = $"/K \"{exePath}\" {runParams} & pause";
            }
            else
            {
                fileName = exePath;
                arguments = runParams;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = keepCmdOpen,
                CreateNoWindow = !keepCmdOpen,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };

            try
            {
                currentProcess = new()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                currentProcess.Start();
                aliveProcesses.Add(new(currentProcess.Id, currentProcess.ProcessName));

                if (waitForExit)
                {
                    await currentProcess.WaitForExitAsync();
                    WasLastProcessSuccesfull = currentProcess.ExitCode == 0;
                }

            }
            catch (Exception ex)
            {
                await MessageBox.Show($"Error when trying to run the current project: {ex.Message}", LogManager.LogLevel.Error);
            }
        }

        public void AbortProgram()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
            }
        }

        public ObservableCollection<ProcessInfo> GetAliveProcesses()
        {
            return aliveProcesses;
        }

        public void UpdateAliveProcesses()
        {
            for (int i = aliveProcesses.Count - 1; i >= 0; i--)
            {
                ProcessInfo processInfo = aliveProcesses[i];
                try
                {
                    Process process = Process.GetProcessById(processInfo.PID);
                    if (process.HasExited)
                    {
                        aliveProcesses.RemoveAt(i);
                    }
                }
                catch (ArgumentException)
                {
                    // Process does not exist
                    aliveProcesses.RemoveAt(i);
                }
            }
        }
    }
}