using System.Text.Json.Serialization;

using QArantineLauncher.Code.Projects.Cleaning;
using QArantineLauncher.Code.Projects.Building;
using QArantineLauncher.Code.Projects.Publishing;
using QArantineLauncher.Code.Projects.Testing;
using QArantineLauncher.Code.Projects.Running;

namespace QArantineLauncher.Code.Projects
{
    public class Project 
    {
        public string Name { get; set; }
        public string ProjectRootPath { get; set; }
        public string ExeFileName { get; set; }
        public string TestsPath { get => pTester.TestsPath; set { pTester.TestsPath = value; } }
        public string TestsOutputPath { get => pTester.TestsOutputPath; set { pTester.TestsOutputPath = value; } }
        private string _runParams;
        private string _buildExePath;
        private string _publishingExePath;
        public string RunParams { get => _runParams; set { _runParams = value; pRunner.RunParams = value; } }
        public string BuildOutputPath { get => pBuilder.BuildOutputPath; set { pBuilder.BuildOutputPath = value; } }
        public string PublishingOutputPath { get => pPublisher.PublishingOutputPath; set { pPublisher.PublishingOutputPath = value; } }
        public string AdditionalCopyFiles { get => pBuilder.AdditionalCopyFiles; set { pBuilder.AdditionalCopyFiles = value; pPublisher.AdditionalCopyFiles = value; } }
        public string BuildExePath { get => _buildExePath; set { _buildExePath = value; pRunner.BuildExePath = value; } }
        public string PublishingExePath { get => _publishingExePath; set { _publishingExePath = value; pRunner.PublishingExePath = value; } }
        public string LastExecutionRootPath { get => pRunner.LastExecutionRootPath; set { pRunner.LastExecutionRootPath = value; } }
        [JsonIgnore]
        public ProjectCleaner pCleaner;
        [JsonIgnore]
        public ProjectBuilder pBuilder;
        [JsonIgnore]
        public ProjectPublisher pPublisher;
        [JsonIgnore]
        public ProjectTester pTester;
        [JsonIgnore]
        public ProjectRunner pRunner;
        public event EventHandler? ProcessEnded;
        public ProjectRunner.ExeType LastExeType { get; set; }
        public string LastWorkingDirectory { get; set; }
        private bool _isProcessAborted = false;
        private bool _isCleaningEnabled;
        private bool _isBuildEnabled;
        private bool _isPublishingEnabled;
        private bool _isTestingEnabled;
        private bool _isRunExeOnProcessEndEnabled;
        private bool _isRunCmdEnabled;

        public bool IsCleaningEnabled { get => _isCleaningEnabled; set { _isCleaningEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }
        public bool IsBuildEnabled { get => _isBuildEnabled; set { _isBuildEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }
        public bool IsPublishingEnabled { get => _isPublishingEnabled; set { _isPublishingEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }
        public bool IsTestingEnabled { get => _isTestingEnabled; set { _isTestingEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }
        public bool IsRunExeOnProcessEndEnabled { get => _isRunExeOnProcessEndEnabled; set { _isRunExeOnProcessEndEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }
        public bool IsRunCmdEnabled { get => _isRunCmdEnabled; set { _isRunCmdEnabled = value; ProjectManager.Instance.SaveExistentProjects(); } }

        [JsonIgnore]
        public bool IsProcessRunning
        {
            get => pCleaner.IsCleaning || pBuilder.IsBuilding || pPublisher.IsPublishing || pTester.IsTesting;
        }

        public Project(
            string name,
            string projectRootPath,
            string exeFileName,
            string testsPath,
            string testsOutputPath,
            string buildOutputPath = "",
            string publishingOutputPath = "",
            string additionalCopyFiles = "",
            string runParams = "",
            string buildExePath = "",
            string publishingExePath = "",
            string lastExecutionRootPath = "",
            ProjectRunner.ExeType lastExeType = ProjectRunner.ExeType.None,
            string lastWorkingDirectory = "",
            bool isCleaningEnabled = true,
            bool isBuildEnabled = true,
            bool isPublishingEnabled = true,
            bool isTestingEnabled = true,
            bool isRunExeOnProcessEndEnabled = false,
            bool isRunCmdEnabled = false
        )
        {
            Name = name;
            ProjectRootPath = projectRootPath;
            ExeFileName = exeFileName;
            _runParams = runParams;
            _buildExePath = buildExePath;
            _publishingExePath = publishingExePath;
            LastExeType = lastExeType;
            LastWorkingDirectory = lastWorkingDirectory;
            _isCleaningEnabled = isCleaningEnabled;
            _isBuildEnabled = isBuildEnabled;
            _isPublishingEnabled = isPublishingEnabled;
            _isTestingEnabled = isTestingEnabled;
            _isRunExeOnProcessEndEnabled = isRunExeOnProcessEndEnabled;
            _isRunCmdEnabled = isRunCmdEnabled;
            pCleaner = new(projectRootPath);pBuilder = new(projectRootPath, buildOutputPath, additionalCopyFiles);
            pPublisher = new(projectRootPath, publishingOutputPath, additionalCopyFiles);
            pRunner = new(buildExePath, publishingExePath, lastExecutionRootPath, runParams);
            pTester = new(this, projectRootPath, testsPath, testsOutputPath);
        }

        public void StartProcess()
        {
            _isProcessAborted = false;
            CleanAllLogs();
            new Thread(StartProcessTask) { IsBackground = true }.Start();
        }

        private async void StartProcessTask()
        {
            if (IsCleaningEnabled && !_isProcessAborted)
                await pCleaner.StartCleaning();

            if (IsBuildEnabled && !_isProcessAborted)
            {
                await pBuilder.StartBuilding();
                UpdateBuildExePath();
            }

            if (IsPublishingEnabled && !_isProcessAborted)
            {
                await pPublisher.StartPublishing();
                UpdatePublishingExePath();
            }

            if (IsTestingEnabled && !_isProcessAborted)
                await pTester.StartTesting(_isRunCmdEnabled);

            EndProcess();
        }

        private void UpdateBuildExePath()
        {
            if (pBuilder.WasLastProcessSuccesfull)
            {
                if(!string.IsNullOrWhiteSpace(BuildOutputPath))
                {
                    BuildExePath = Path.Combine(BuildOutputPath, $"{ExeFileName}.exe");
                }
                else
                {
                    BuildExePath = Path.Combine(ProjectRootPath, $"{ProjectBuilder.ExpectedBasePath}/{ExeFileName}.exe");
                }
                LastExeType = ProjectRunner.ExeType.Build;
                LastWorkingDirectory = BuildOutputPath;
            }
        }

        private void UpdatePublishingExePath()
        {
            if (pPublisher.WasLastProcessSuccesfull)
            {
                if(!string.IsNullOrWhiteSpace(PublishingOutputPath))
                {
                    PublishingExePath = Path.Combine(PublishingOutputPath, $"{ExeFileName}.exe");
                }
                else
                {
                    PublishingExePath = Path.Combine(ProjectRootPath, $"{ProjectPublisher.ExpectedBasePath}/{ExeFileName}.exe");
                }
                LastExeType = ProjectRunner.ExeType.Publish;
                LastWorkingDirectory = PublishingOutputPath;
            }
        }

        private void EndProcess()
        {
            ProjectManager.Instance.SaveExistentProjects();
            ProcessEnded?.Invoke(this, EventArgs.Empty);
            RunExeOnEndIfActive();
        }

        private void RunExeOnEndIfActive()
        {
            if (IsRunExeOnProcessEndEnabled && LastExeType != ProjectRunner.ExeType.None)
            {
                _ = pRunner.RunProgram(LastExeType, false, IsRunCmdEnabled);
            }
        }

        public void AbortProcess()
        {
            _isProcessAborted = true;
            pCleaner.AbortCleaning();
            pBuilder.AbortBuilding();
            pPublisher.AbortPublishing();
            pTester.AbortTesting();
        }

        private void CleanAllLogs()
        {
            pCleaner.CleanLogLines();
            pBuilder.CleanLogLines();
            pPublisher.CleanLogLines();
            //pTester.CleanTestResultsData();
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public override bool Equals(object? obj)
        { 
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            return Name == ((Project)obj).Name;
        }
        
        public override int GetHashCode()
        {
            int hashCode = 17;

            hashCode = hashCode * 23 + (Name?.GetHashCode() ?? 0);
            
            return hashCode;
        }
    }
}