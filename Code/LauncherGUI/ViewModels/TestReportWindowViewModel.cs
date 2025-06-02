using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AngleSharp.Common;

using QArantineLauncher.Code.LauncherGUI.Models;
using QArantine.Code.Test;
using QArantineLauncher.Code.JsonContexts;

namespace QArantineLauncher.Code.LauncherGUI.ViewModels
{
    public class TestReportWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public TestResult ExecutionResult;
        public ObservableCollection<TestError> ExecutionErrors { get; set; }
        public ObservableCollection<GUIErrorSection> ExecutionErrorSections { get; set; }

        public TestReportWindowViewModel(TestResult executionResult)
        {
            ExecutionResult = executionResult;
            ExecutionErrors = GetExecutionErrors();
            ExecutionErrorSections = GetExecutionErrorSections();
        }

        private ObservableCollection<TestError> GetExecutionErrors()
        {
            string path = Path.Combine(ExecutionResult.OutputDirectoryPath, "TestFoundErrors.json");
            if (!File.Exists(path))
            {
                LogError($"Couldn't find the test '{ExecutionResult.TestName}' output file: {path}");
                return [];
            }

            try
            {
                string jsonContent = File.ReadAllText(path);
                return JsonSerializer.Deserialize(jsonContent, TestReportJsonContext.Default.ObservableCollectionTestError) ?? [];
            }
            catch (Exception ex)
            {
                LogError($"Error when trying to read the test '{ExecutionResult.TestName}' output file: {ex.Message}");
                return [];
            }
        }

        private ObservableCollection<GUIErrorSection> GetExecutionErrorSections()
        {
            var groupedErrors = ExecutionErrors.GroupBy(e => e.ErrorID)
                                  .Select(g => new GUIErrorSection(
                                      g.Key,
                                      g.GetItemByIndex(0).ErrorDescription,
                                      g.GetItemByIndex(0).ErrorCategory,
                                      [.. g]
                                  ))
                                  .ToList();
            return [.. groupedErrors];
        }
        
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
