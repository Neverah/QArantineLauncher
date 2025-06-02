using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls;
using System.Runtime.CompilerServices;

using QArantineLauncher.Code.LauncherGUI.StaticData;
using QArantineLauncher.Code.LauncherGUI.Models;
using QArantineLauncher.Code.Projects;
using QArantineLauncher.Code.LauncherGUI.Views;
using QArantine.Code.Test;
using QArantineLauncher.Code.Utils;
using QArantineLauncher.Code.Projects.Running;
using System.Diagnostics;

namespace QArantineLauncher.Code.LauncherGUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Window? projectCreationWindow;
        private Window? projectUpdateWindow;
        private Window? enabledTestsWindow;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand CreateNewProjectCommand { get; }
        public ICommand DeleteCurrentProjectCommand { get; }
        public ICommand ConfigureCurrentProjectCommand { get; }
        public ICommand ToggleRunExeOnProcessEndEnabledCommand { get; }
        public ICommand ToggleRunCmdEnabledCommand { get; }
        public ICommand OpenMainLogCommand { get; }
        public ICommand OpenRootDirectoryCommand { get; }
        public ICommand ToggleCleaningAutoScrollCommand { get; }
        public ICommand ToggleBuildAutoScrollCommand { get; }
        public ICommand TogglePublishingAutoScrollCommand { get; }
        public ICommand ToggleTestingAutoScrollCommand { get; }
        public ICommand ConfigureEnabledTestsCommand { get; }
        public ICommand RunOrAbortProcessCommand { get; }
        public ICommand TerminateSelectedProcessCommand { get; }
        public ICommand LaunchLastBuildVersionCommand { get; }
        public ICommand OpenBuildOutputDirectoryCommand { get; }
        public ICommand LaunchLastPackedVersionCommand { get; }
        public ICommand OpenPackedOutputDirectoryCommand { get; }
        private Project? _currentProject;
        private ProcessInfo? _selectedProcess;
        private ObservableCollection<GUILogLine> _cleaningLogLines;
        private ObservableCollection<GUILogLine> _buildLogLines;
        private ObservableCollection<GUILogLine> _publishingLogLines;
        private ObservableCollection<GUITestResult> _testingResultsData;
        private ScrollViewer? _cleaningLogScrollViewer;
        private ScrollViewer? _buildLogScrollViewer;
        private ScrollViewer? _publishingLogScrollViewer;
        private ScrollViewer? _testingResultsScrollViewer;
        private bool _isCleaningAutoScrollEnabled = true;
        private bool _isBuildAutoScrollEnabled = true;
        private bool _isPublishingAutoScrollEnabled = true;
        private bool _isTestingAutoScrollEnabled = true;
        private string _runExeOnProcessEndIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _runCmdIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _cleaningAutoScrollButtonIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _buildAutoScrollButtonIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _publishingAutoScrollButtonIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _testingAutoScrollButtonIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];
        private string _isProcessRunningButtonIcon = IconsDictionary.LauncherIconsDictionary["No_Image"];

        public static ObservableCollection<Project> ExistentProjects
        {
            get => ProjectManager.Instance.ExistentProjects;
        }

        public Project CurrentProject
        {
            get => _currentProject!;
            set
            {
                if (CurrentProject != value)
                {
                    _currentProject = value;
                }
            }
        }

        public ObservableCollection<ProcessInfo> CurrentProjectAliveProcesses
        { 
            get => CurrentProject.pRunner.GetAliveProcesses();
        }

        public ProcessInfo? SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                RaisePropertyChanged(nameof(SelectedProcess));
            }
        }

        public bool IsRunExeOnProcessEndEnabled
        {
            get => CurrentProject.IsRunExeOnProcessEndEnabled;
            set
            {
                if (IsRunExeOnProcessEndEnabled != value)
                {
                    CurrentProject.IsRunExeOnProcessEndEnabled = value;
                    RaisePropertyChanged(nameof(IsRunExeOnProcessEndEnabled));
                    UpdateRunExeOnProcessEndEnabledIcon();
                }
            }
        }

        public bool IsRunCmdEnabled
        {
            get => CurrentProject.IsRunCmdEnabled;
            set
            {
                if (IsRunCmdEnabled != value)
                {
                    CurrentProject.IsRunCmdEnabled = value;
                    RaisePropertyChanged(nameof(IsRunCmdEnabled));
                    UpdateRunCmdEnabledIcon();
                }
            }
        }

        public bool IsCleaningEnabled
        {
            get => CurrentProject != null && CurrentProject.IsCleaningEnabled;
            set
            {
                if (IsCleaningEnabled != value)
                {
                    CurrentProject.IsCleaningEnabled = value;
                    RaisePropertyChanged(nameof(IsCleaningEnabled));
                }
            }
        }

        public bool IsBuildEnabled
        {
            get => CurrentProject != null && CurrentProject.IsBuildEnabled;
            set
            {
                if (IsBuildEnabled != value)
                {
                    CurrentProject.IsBuildEnabled = value;
                    RaisePropertyChanged(nameof(IsBuildEnabled));
                }
            }
        }

        public bool IsPublishingEnabled
        {
            get => CurrentProject != null && CurrentProject.IsPublishingEnabled;
            set
            {
                if (IsPublishingEnabled != value)
                {
                    CurrentProject.IsPublishingEnabled = value;
                    RaisePropertyChanged(nameof(IsPublishingEnabled));
                }
            }
        }

        public bool IsTrimmingEnabled
        {
            get => CurrentProject != null && CurrentProject.IsTrimmingEnabled;
            set
            {
                if (IsTrimmingEnabled != value)
                {
                    CurrentProject.IsTrimmingEnabled = value;
                    RaisePropertyChanged(nameof(IsTrimmingEnabled));
                }
            }
        }

        public bool IsTestingEnabled
        {
            get => CurrentProject != null && CurrentProject.IsTestingEnabled;
            set
            {
                if (IsTestingEnabled != value)
                {
                    CurrentProject.IsTestingEnabled = value;
                    RaisePropertyChanged(nameof(IsTestingEnabled));
                }
            }
        }

        public bool IsCleaningAutoScrollEnabled
        {
            get => _isCleaningAutoScrollEnabled;
            set
            {
                if (_isCleaningAutoScrollEnabled != value)
                {
                    _isCleaningAutoScrollEnabled = value;
                    RaisePropertyChanged(nameof(IsCleaningAutoScrollEnabled));
                    CleaningAutoScrollButtonIcon = IsCleaningAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
                    if (_isCleaningAutoScrollEnabled)
                    {
                        ScrollCleaningLogToBottom();
                    }
                }
            }
        }

        public bool IsBuildAutoScrollEnabled
        {
            get => _isBuildAutoScrollEnabled;
            set
            {
                if (_isBuildAutoScrollEnabled != value)
                {
                    _isBuildAutoScrollEnabled = value;
                    RaisePropertyChanged(nameof(IsBuildAutoScrollEnabled));
                    BuildAutoScrollButtonIcon = IsBuildAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
                    if (_isBuildAutoScrollEnabled)
                    {
                        ScrollBuildLogToBottom();
                    }
                }
            }
        }

        public bool IsPublishingAutoScrollEnabled
        {
            get => _isPublishingAutoScrollEnabled;
            set
            {
                if (_isPublishingAutoScrollEnabled != value)
                {
                    _isPublishingAutoScrollEnabled = value;
                    RaisePropertyChanged(nameof(IsPublishingAutoScrollEnabled));
                    PublishingAutoScrollButtonIcon = IsPublishingAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
                    if (_isPublishingAutoScrollEnabled)
                    {
                        ScrollPublishingLogToBottom();
                    }
                }
            }
        }

        public bool IsTestingAutoScrollEnabled
        {
            get => _isTestingAutoScrollEnabled;
            set
            {
                if (_isTestingAutoScrollEnabled != value)
                {
                    _isTestingAutoScrollEnabled = value;
                    RaisePropertyChanged(nameof(IsTestingAutoScrollEnabled));
                    TestingAutoScrollButtonIcon = IsTestingAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
                    if (_isTestingAutoScrollEnabled)
                    {
                        ScrollTestingResultsListToBottom();
                    }
                }
            }
        }

        public bool IsProcessRunning
        {
            get =>  CurrentProject != null && CurrentProject.IsProcessRunning;
            set
            {
                if (IsProcessRunning != value)
                {
                    if (IsProcessRunning)
                    {
                        CurrentProject.AbortProcess();
                    }
                    else
                    {
                        CurrentProject.StartProcess();
                    }
                    
                    RaisePropertyChanged(nameof(IsProcessRunning));
                    UpdateProcessRunningIcon();
                }
            }
        }

        public string RunExeOnProcessEndIcon
        {
            get => _runExeOnProcessEndIcon;
            set
            {
                _runExeOnProcessEndIcon = value;
                RaisePropertyChanged(nameof(RunExeOnProcessEndIcon));
            }
        }

        public string RunCmdIcon
        {
            get => _runCmdIcon;
            set
            {
                _runCmdIcon = value;
                RaisePropertyChanged(nameof(RunCmdIcon));
            }
        }

        public string CleaningAutoScrollButtonIcon
        {
            get => _cleaningAutoScrollButtonIcon;
            set
            {
                _cleaningAutoScrollButtonIcon = value;
                RaisePropertyChanged(nameof(CleaningAutoScrollButtonIcon));
            }
        }

        public string BuildAutoScrollButtonIcon
        {
            get => _buildAutoScrollButtonIcon;
            set
            {
                _buildAutoScrollButtonIcon = value;
                RaisePropertyChanged(nameof(BuildAutoScrollButtonIcon));
            }
        }

        public string PublishingAutoScrollButtonIcon
        {
            get => _publishingAutoScrollButtonIcon;
            set
            {
                _publishingAutoScrollButtonIcon = value;
                RaisePropertyChanged(nameof(PublishingAutoScrollButtonIcon));
            }
        }

        public string TestingAutoScrollButtonIcon
        {
            get => _testingAutoScrollButtonIcon;
            set
            {
                _testingAutoScrollButtonIcon = value;
                RaisePropertyChanged(nameof(TestingAutoScrollButtonIcon));
            }
        }

        public string IsProcessRunningButtonIcon
        {
            get => _isProcessRunningButtonIcon;
            set
            {
                _isProcessRunningButtonIcon = value;
                RaisePropertyChanged(nameof(IsProcessRunningButtonIcon));
            }
        }

        public ObservableCollection<GUILogLine> CleaningLogLines
        {
            get => _cleaningLogLines;
            set
            {
                _cleaningLogLines = value;
                RaisePropertyChanged(nameof(CleaningLogLines));
            }
        }

        public ObservableCollection<GUILogLine> BuildLogLines
        {
            get => _buildLogLines;
            set
            {
                _buildLogLines = value;
                RaisePropertyChanged(nameof(BuildLogLines));
            }
        }

        public ObservableCollection<GUILogLine> PublishingLogLines
        {
            get => _publishingLogLines;
            set
            {
                _publishingLogLines = value;
                RaisePropertyChanged(nameof(PublishingLogLines));
            }
        }

        public ObservableCollection<GUITestResult> TestingResultsData
        {
            get => _testingResultsData;
            set
            {
                _testingResultsData = value;
                RaisePropertyChanged(nameof(TestingResultsData));
            }
        }

        public MainWindowViewModel()
        {
            _cleaningLogLines = [];
            _buildLogLines = [];
            _publishingLogLines = [];
            _testingResultsData = [];

            SetNewSelectedProject(ProjectManager.Instance.ExistentProjects.First());

            UpdateRunExeOnProcessEndEnabledIcon();
            UpdateRunCmdEnabledIcon();
            CleaningAutoScrollButtonIcon = IsCleaningAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
            BuildAutoScrollButtonIcon = IsBuildAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
            PublishingAutoScrollButtonIcon = IsPublishingAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
            TestingAutoScrollButtonIcon = IsTestingAutoScrollEnabled ? IconsDictionary.LauncherIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.LauncherIconsDictionary["AutoScroll_Disabled"];
            IsProcessRunningButtonIcon = IsProcessRunning ? IconsDictionary.LauncherIconsDictionary["Stop"] : IconsDictionary.LauncherIconsDictionary["Start"];

            CreateNewProjectCommand = new RelayCommand(CreateNewProject);
            DeleteCurrentProjectCommand = new RelayCommand(DeleteCurrentProject);
            ConfigureCurrentProjectCommand = new RelayCommand(ConfigureCurrentProject);
            
            RunOrAbortProcessCommand = new RelayCommand(RunOrAbortProcess);
            TerminateSelectedProcessCommand = new RelayCommand(TerminateSelectedProcess);

            ToggleRunExeOnProcessEndEnabledCommand = new RelayCommand(ToggleRunExeOnProcessEndEnabled);
            ToggleRunCmdEnabledCommand = new RelayCommand(ToggleRunCmdEnabled);

            OpenMainLogCommand = new RelayCommand(OpenMainLog);
            OpenRootDirectoryCommand = new RelayCommand(OpenRootDirectory);
            
            LaunchLastBuildVersionCommand = new RelayCommand(LaunchLastBuildVersion);
            OpenBuildOutputDirectoryCommand = new RelayCommand(OpenBuildOutputDirectory);

            LaunchLastPackedVersionCommand = new RelayCommand(LaunchLastPackedVersion);
            OpenPackedOutputDirectoryCommand = new RelayCommand(OpenPackedOutputDirectory);
            

            ToggleCleaningAutoScrollCommand = new RelayCommand(ToggleCleaningAutoScroll);
            ToggleBuildAutoScrollCommand = new RelayCommand(ToggleBuildAutoScroll);
            TogglePublishingAutoScrollCommand = new RelayCommand(TogglePublishingAutoScroll);
            ToggleTestingAutoScrollCommand = new RelayCommand(ToggleTestingAutoScroll);

            ConfigureEnabledTestsCommand = new RelayCommand(ConfigureEnabledTests);
        }

        public void ClearCleaningLogLines()
        {
            _cleaningLogLines.Clear();
            RaisePropertyChanged(nameof(CleaningLogLines));
        }

        public void ClearBuildLogLines()
        {
            _buildLogLines.Clear();
            RaisePropertyChanged(nameof(BuildLogLines));
        }

        public void ClearPublishingLogLines()
        {
            _publishingLogLines.Clear();
            RaisePropertyChanged(nameof(PublishingLogLines));
        }

        public void ClearTestingResultsData()
        {
            _testingResultsData.Clear();
            RaisePropertyChanged(nameof(TestingResultsData));
        }

        private void UpdateCurrentProjectAliveProcesses()
        {
            CurrentProject.pRunner.UpdateAliveProcesses();
            RaisePropertyChanged(nameof(CurrentProjectAliveProcesses));
        }

        public void ProcessSelectionComboBox_DropDownOpened(object? sender, EventArgs e)
        {
            UpdateCurrentProjectAliveProcesses();
        }

        public void ProcessSelectionComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentProjectAliveProcesses();

            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                SelectedProcess = comboBox.SelectedItem as ProcessInfo;
            }
        }

        public void ProjectSelectionComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ProjectManager.Instance.SaveExistentProjects();

            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                SetNewSelectedProject(comboBox.SelectedItem as Project);
            }
        }

        private void SetNewSelectedProject(Project? newProject)
        {
            if (newProject != null)
            {
                if (CurrentProject != null)
                {
                    DesuscribeFromOldProjectEvents();
                }

                ClearCleaningLogLines();
                ClearBuildLogLines();
                ClearPublishingLogLines();
                ClearTestingResultsData();

                CurrentProject = newProject;

                SuscribeToNewProjectEvents();

                _ = CurrentProject.pTester.OnSelectedProjectChanged();

                SelectedProcess = null;
                UpdateCurrentProjectAliveProcesses();

                RaisePropertyChanged(nameof(IsCleaningEnabled));
                RaisePropertyChanged(nameof(IsBuildEnabled));
                RaisePropertyChanged(nameof(IsPublishingEnabled));
                RaisePropertyChanged(nameof(IsTrimmingEnabled));
                RaisePropertyChanged(nameof(IsTestingEnabled));

                RaisePropertyChanged(nameof(IsRunExeOnProcessEndEnabled));
                RaisePropertyChanged(nameof(IsRunCmdEnabled));

                UpdateRunExeOnProcessEndEnabledIcon();
                UpdateRunCmdEnabledIcon();

                RaisePropertyChanged(nameof(CurrentProject));
            }
            else
            {
                LogFatalError($"The current selected project '{newProject}' is null");
            }
        }

        private void DesuscribeFromOldProjectEvents()
        {
            CurrentProject.pCleaner.LogLineAdded -= CleaningLog_LogLineAdded;
            CurrentProject.pCleaner.LogLinesClear -= CleaningLog_LogLineClear;
            CurrentProject.pCleaner.CleaningStarted -= Project_ProcessStateUpdated;
            CurrentProject.pCleaner.CleaningEnded -= Project_ProcessStateUpdated;

            CurrentProject.pBuilder.LogLineAdded -= BuilderLog_LogLineAdded;
            CurrentProject.pBuilder.LogLinesClear -= BuilderLog_LogLineClear;
            CurrentProject.pBuilder.BuildStarted -= Project_ProcessStateUpdated;
            CurrentProject.pBuilder.BuildEnded -= Project_ProcessStateUpdated;

            CurrentProject.pPublisher.LogLineAdded -= PublishingLog_LogLineAdded;
            CurrentProject.pPublisher.LogLinesClear -= PublishingLog_LogLineClear;
            CurrentProject.pPublisher.PublishingStarted -= Project_ProcessStateUpdated;
            CurrentProject.pPublisher.PublishingEnded -= Project_ProcessStateUpdated;

            CurrentProject.pTester.TestResultDataAdded -= TestingResultsData_TestResultDataAdded;
            CurrentProject.pTester.TestResultsDataClear -= TestingResultsData_TestResultsDataClear;
            CurrentProject.pTester.TestingStarted -= Project_ProcessStateUpdated;
            CurrentProject.pTester.TestingEnded -= Project_ProcessStateUpdated;

            CurrentProject.ProcessEnded -= Project_ProcessStateUpdated;
        }

        private void SuscribeToNewProjectEvents()
        {
            CurrentProject.pCleaner.LogLineAdded += CleaningLog_LogLineAdded;
            CurrentProject.pCleaner.LogLinesClear += CleaningLog_LogLineClear;
            CurrentProject.pCleaner.CleaningStarted += Project_ProcessStateUpdated;
            CurrentProject.pCleaner.CleaningEnded += Project_ProcessStateUpdated;

            CurrentProject.pBuilder.LogLineAdded += BuilderLog_LogLineAdded;
            CurrentProject.pBuilder.LogLinesClear += BuilderLog_LogLineClear;
            CurrentProject.pBuilder.BuildStarted += Project_ProcessStateUpdated;
            CurrentProject.pBuilder.BuildEnded += Project_ProcessStateUpdated;

            CurrentProject.pPublisher.LogLineAdded += PublishingLog_LogLineAdded;
            CurrentProject.pPublisher.LogLinesClear += PublishingLog_LogLineClear;
            CurrentProject.pPublisher.PublishingStarted += Project_ProcessStateUpdated;
            CurrentProject.pPublisher.PublishingEnded += Project_ProcessStateUpdated;

            CurrentProject.pTester.TestResultDataAdded += TestingResultsData_TestResultDataAdded;
            CurrentProject.pTester.TestResultsDataClear += TestingResultsData_TestResultsDataClear;
            CurrentProject.pTester.TestingStarted += Project_ProcessStateUpdated;
            CurrentProject.pTester.TestingEnded += Project_ProcessStateUpdated;

            CurrentProject.ProcessEnded += Project_ProcessStateUpdated;
        }

        private void CreateNewProject()
        {
            if (projectCreationWindow == null)
            {
                projectCreationWindow = new ProjectCreationWindow();
                projectCreationWindow.Closed += (sender, e) => projectCreationWindow = null;
                projectCreationWindow.Show();
            }
            else
            {
                projectCreationWindow.Activate();
            }
        }

        private async void DeleteCurrentProject()
        {
            await ConfirmationWindow.Show($"Confirm that you want to delete the '{CurrentProject.Name}' project", DeleteCurrentProjectCallback);
        }

        private void DeleteCurrentProjectCallback()
        {
            ExistentProjects.Remove(CurrentProject);
            Project newProject = ProjectManager.Instance.ExistentProjects.First();

            if (newProject == null)
                ProjectManager.Instance.InitEmptyExistentProjectsList();
            
            SetNewSelectedProject(newProject);
        }

        private void ConfigureCurrentProject()
        {
            if (projectUpdateWindow == null)
            {
                projectUpdateWindow = new ProjectUpdateWindow(CurrentProject);
                projectUpdateWindow.Closed += (sender, e) => projectUpdateWindow = null;
                projectUpdateWindow.Show();
            }
            else
            {
                projectUpdateWindow.Activate();
            }
        }

        private void ToggleRunExeOnProcessEndEnabled()
        {
            IsRunExeOnProcessEndEnabled = !IsRunExeOnProcessEndEnabled;
        }

        private void ToggleRunCmdEnabled()
        {
            IsRunCmdEnabled = !IsRunCmdEnabled;
        }

        private async void OpenMainLog()
        {
            string logFilePath = Path.Combine(CurrentProject.LastExecutionRootPath, "Log.html");
            if (!FileUtils.OpenFile(logFilePath)) await MessageBox.Show($"The file '{logFilePath}' doesn't exist", LogManager.LogLevel.Error);
        }

        private void ToggleCleaningAutoScroll()
        {
            IsCleaningAutoScrollEnabled = !IsCleaningAutoScrollEnabled;
        }

        private void ToggleBuildAutoScroll()
        {
            IsBuildAutoScrollEnabled = !IsBuildAutoScrollEnabled;
        }

        private void TogglePublishingAutoScroll()
        {
            IsPublishingAutoScrollEnabled = !IsPublishingAutoScrollEnabled;
        }

        private void ToggleTestingAutoScroll()
        {
            IsTestingAutoScrollEnabled = !IsTestingAutoScrollEnabled;
        }

        private void ConfigureEnabledTests()
        {
            if (enabledTestsWindow == null)
            {
                enabledTestsWindow = new EnabledTestsWindow(CurrentProject.pTester);
                enabledTestsWindow.Closed += (sender, e) => enabledTestsWindow = null;
                enabledTestsWindow.Show();
            }
            else
            {
                enabledTestsWindow.Activate();
            }
        }

        private void RunOrAbortProcess()
        {
            IsProcessRunning = !IsProcessRunning;
        }

        private void TerminateSelectedProcess()
        {
            if (SelectedProcess != null)
            {
                Process process = Process.GetProcessById(SelectedProcess.PID);
                process.Kill(true);
                UpdateCurrentProjectAliveProcesses();
            }
        }

        private void UpdateProcessRunningIcon()
        {
            IsProcessRunningButtonIcon = IsProcessRunning ? IconsDictionary.LauncherIconsDictionary["Stop"] : IconsDictionary.LauncherIconsDictionary["Start"];
        }

        private void UpdateRunExeOnProcessEndEnabledIcon()
        {
            RunExeOnProcessEndIcon = CurrentProject.IsRunExeOnProcessEndEnabled ? IconsDictionary.LauncherIconsDictionary["LaunchGame_Enabled"] : IconsDictionary.LauncherIconsDictionary["LaunchGame_Disabled"];
        }

        private void UpdateRunCmdEnabledIcon()
        {
            RunCmdIcon = IsRunCmdEnabled ? IconsDictionary.LauncherIconsDictionary["Cmd_Enabled"] : IconsDictionary.LauncherIconsDictionary["Cmd_Disabled"];
        }

        public void Project_ProcessStateUpdated(object? sender, EventArgs e)
        {
            UpdateProcessRunningIcon();
        }

        private void OpenRootDirectory()
        {
            FileUtils.OpenDirectory(CurrentProject.ProjectRootPath);
        }

        private void LaunchLastBuildVersion()
        {
            _ = CurrentProject.pRunner.RunProgram(Projects.Running.ProjectRunner.ExeType.Build, false, IsRunCmdEnabled);
        }

        private void OpenBuildOutputDirectory()
        {
            FileUtils.OpenDirectory(CurrentProject.BuildOutputPath);
        }

        private void LaunchLastPackedVersion()
        {
            _ = CurrentProject.pRunner.RunProgram(Projects.Running.ProjectRunner.ExeType.Publish, false, IsRunCmdEnabled);
        }

        private void OpenPackedOutputDirectory()
        {
            FileUtils.OpenDirectory(CurrentProject.PublishingOutputPath);
        }

        private void ScrollCleaningLogToBottom()
        {
            if (_isCleaningAutoScrollEnabled && _cleaningLogScrollViewer != null)
            {
                _cleaningLogScrollViewer.Offset = new Vector(_cleaningLogScrollViewer.Offset.X, _cleaningLogScrollViewer.Extent.Height);
            }
        }

        public void SetCleaningLogScrollViewerReference(ScrollViewer scrollViewer)
        {
            _cleaningLogScrollViewer = scrollViewer;
        }

        private void ScrollBuildLogToBottom()
        {
            if (_isBuildAutoScrollEnabled && _buildLogScrollViewer != null)
            {
                _buildLogScrollViewer.Offset = new Vector(_buildLogScrollViewer.Offset.X, _buildLogScrollViewer.Extent.Height);
            }
        }

        public void SetBuildLogScrollViewerReference(ScrollViewer scrollViewer)
        {
            _buildLogScrollViewer = scrollViewer;
        }

        private void ScrollPublishingLogToBottom()
        {
            if (_isPublishingAutoScrollEnabled && _publishingLogScrollViewer != null)
            {
                _publishingLogScrollViewer.Offset = new Vector(_publishingLogScrollViewer.Offset.X, _publishingLogScrollViewer.Extent.Height);
            }
        }

        private void ScrollTestingResultsListToBottom()
        {
            if (_isTestingAutoScrollEnabled && _testingResultsScrollViewer != null)
            {
                _testingResultsScrollViewer.Offset = new Vector(_testingResultsScrollViewer.Offset.X, _testingResultsScrollViewer.Extent.Height);
            }
        }

        public void SetPublishingLogScrollViewerReference(ScrollViewer scrollViewer)
        {
            _publishingLogScrollViewer = scrollViewer;
        }

        public void SetTestingResultsScrollViewerReference(ScrollViewer scrollViewer)
        {
            _testingResultsScrollViewer = scrollViewer;
        }

        private void CleaningLog_LogLineClear(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pCleaner.LogMutex.WaitOne();
                CleaningLogLines.Clear();
                CurrentProject.pCleaner.LogMutex.ReleaseMutex();
                ScrollCleaningLogToBottom();
            });
        }

        private void CleaningLog_LogLineAdded(object? sender, EventArgs newLine)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pCleaner.LogMutex.WaitOne();
                AddLogLine(CleaningLogLines, GetFinalLogLineFromBufferLogLine((BufferLogLine)newLine));
                CurrentProject.pCleaner.LogMutex.ReleaseMutex();
                ScrollCleaningLogToBottom();
            });
        }

        private void BuilderLog_LogLineClear(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pBuilder.LogMutex.WaitOne();
                BuildLogLines.Clear();
                CurrentProject.pBuilder.LogMutex.ReleaseMutex();
                ScrollBuildLogToBottom();
            });
        }

        private void BuilderLog_LogLineAdded(object? sender, EventArgs newLine)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pBuilder.LogMutex.WaitOne();
                AddLogLine(BuildLogLines, GetFinalLogLineFromBufferLogLine((BufferLogLine)newLine));
                CurrentProject.pBuilder.LogMutex.ReleaseMutex();
                ScrollBuildLogToBottom();
            });
        }

        private void PublishingLog_LogLineClear(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pPublisher.LogMutex.WaitOne();
                PublishingLogLines.Clear();
                CurrentProject.pPublisher.LogMutex.ReleaseMutex();
                ScrollPublishingLogToBottom();
            });
        }

        private void PublishingLog_LogLineAdded(object? sender, EventArgs newLine)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pPublisher.LogMutex.WaitOne();
                AddLogLine(PublishingLogLines, GetFinalLogLineFromBufferLogLine((BufferLogLine)newLine));
                CurrentProject.pPublisher.LogMutex.ReleaseMutex();
                ScrollPublishingLogToBottom();
            });
        }

        private void TestingResultsData_TestResultsDataClear(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pTester.ResultsDataListMutex.WaitOne();
                TestingResultsData.Clear();
                CurrentProject.pTester.ResultsDataListMutex.ReleaseMutex();
                ScrollTestingResultsListToBottom();
            });
        }

        private void TestingResultsData_TestResultDataAdded(object? sender, EventArgs testResult)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentProject.pTester.ResultsDataListMutex.WaitOne();
                TestingResultsData.Add(GetGUITestResultFromBackendTestResult((TestResult)testResult));
                CurrentProject.pTester.ResultsDataListMutex.ReleaseMutex();
                ScrollTestingResultsListToBottom();
            });
        }

        private GUILogLine GetFinalLogLineFromBufferLogLine(BufferLogLine bufferLogLine)
        {
            return new GUILogLine(bufferLogLine.Timestamp, bufferLogLine.TimestampForegroundHexCode, bufferLogLine.TestTag, bufferLogLine.TestTagForegroundHexCode, bufferLogLine.LogBody, bufferLogLine.LogBodyForegroundHexCode);
        }

        private GUITestResult GetGUITestResultFromBackendTestResult(TestResult testResult)
        {
            return new GUITestResult(testResult);
        }

        public void AddLogLine(ObservableCollection<GUILogLine> logLinesList, GUILogLine logLine)
        {
            logLinesList.Add(logLine);
        }

        public void AddLogLine(ObservableCollection<GUILogLine> logLinesList, string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            logLinesList.Add(new GUILogLine(timestamp, timestampForeground, testTag, testTagForeground, logBody, logBodyForeground ));
        }

        public void AddLogLine(ObservableCollection<GUILogLine> logLinesList, string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            logLinesList.Add(new GUILogLine(timestamp, timestampForegroundHexCode, testTag, testTagForegroundHexCode, logBody, logBodyForegroundHexCode));
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
