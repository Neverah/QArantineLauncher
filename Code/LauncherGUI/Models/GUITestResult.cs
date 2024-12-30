using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

using QArantine.Code.Test;
using QArantine.Code.FrameworkModules.Logs;

using QArantineLauncher.Code.LauncherGUI.ViewModels;
using QArantineLauncher.Code.LauncherGUI.Views;
using QArantineLauncher.Code.Utils;

namespace QArantineLauncher.Code.LauncherGUI.Models
{
    public class GUITestResult : INotifyPropertyChanged
    {    
        public event PropertyChangedEventHandler? PropertyChanged;
        public TestResult ExecutionResult;
        private string _testName;
        private bool _success;
        private int _testedTestCases;
        private int _foundErrorsAmmount;
        private string _completionTime;
        private long _completionTimeAsSeconds;
        private string _startTimestamp;
        private string _endTimestamp;
        private string _outputDirectoryPath;
        private string _logFilePath;
        private IBrush _successForegroundColor;

        public string TestName { get => _testName; set { _testName = value; RaisePropertyChanged(nameof(TestName)); } }
        public bool Success { get => _success; set { _success = value; _successForegroundColor = GetColorFromSuccessState(_success); RaisePropertyChanged(nameof(Success)); } }
        public int TestedTestCases { get => _testedTestCases; set { _testedTestCases = value; RaisePropertyChanged(nameof(TestedTestCases)); } }
        public int FoundErrorsAmmount { get => _foundErrorsAmmount; set { _foundErrorsAmmount = value; RaisePropertyChanged(nameof(FoundErrorsAmmount)); } }
        public string CompletionTime { get => _completionTime; set { _completionTime = value; RaisePropertyChanged(nameof(CompletionTime)); } }
        public long CompletionTimeAsSeconds { get => _completionTimeAsSeconds; set { _completionTimeAsSeconds = value; RaisePropertyChanged(nameof(CompletionTimeAsSeconds)); } }
        public string StartTimestamp { get => _startTimestamp; set { _startTimestamp = value; RaisePropertyChanged(nameof(StartTimestamp)); } }
        public string EndTimestamp { get => _endTimestamp; set { _endTimestamp = value; RaisePropertyChanged(nameof(EndTimestamp)); } }
        public string OutputDirectoryPath { get => _outputDirectoryPath; set { _outputDirectoryPath = value; RaisePropertyChanged(nameof(OutputDirectoryPath)); } }
        public IBrush SuccessForegroundColor { get => _successForegroundColor; private set { _successForegroundColor = value; RaisePropertyChanged(nameof(SuccessForegroundColor)); } }
        public ICommand? OpenLogCommand { get; private set; }
        public ICommand? OpenReportWindowCommand { get; private set; }

        public GUITestResult(TestResult testResult)
        {
            ExecutionResult = testResult;
            _testName = testResult.TestName;
            _success = testResult.Success;
            _testedTestCases = testResult.TestedTestCases;
            _foundErrorsAmmount = testResult.FoundErrorsAmmount;
            _completionTime = testResult.CompletionTime;
            _completionTimeAsSeconds = testResult.CompletionTimeAsSeconds;
            _startTimestamp = testResult.StartTimestamp;
            _endTimestamp = testResult.EndTimestamp;
            _outputDirectoryPath = testResult.OutputDirectoryPath;
            _logFilePath = Path.Combine(testResult.OutputDirectoryPath, "Log.html");
            _successForegroundColor = GetColorFromSuccessState(testResult.Success);

            OpenLogCommand = new RelayCommand(OpenLog);
            OpenReportWindowCommand =  new RelayCommand(OpenReportWindow);
        }

        private static SolidColorBrush GetColorFromSuccessState(bool success)
        {
            if (success) return new SolidColorBrush(Color.Parse(GUILogHandler.LogColorConsoleWindowMap[LogManager.LogLevel.OK]));
            else return new SolidColorBrush(Color.Parse(GUILogHandler.LogColorConsoleWindowMap[LogManager.LogLevel.Error]));
        }

        private async void OpenLog()
        {
            if (!FileUtils.OpenFile(_logFilePath)) await MessageBox.Show($"The file '{_logFilePath}' doesn't exist", LogManager.LogLevel.Error);
        }

        private void OpenReportWindow()
        {
            TestReportWindow reportWindow = new() { DataContext = new TestReportWindowViewModel(ExecutionResult)};
            if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) reportWindow.ShowDialog(desktop.MainWindow!);
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return TestName;
        }
    }
}