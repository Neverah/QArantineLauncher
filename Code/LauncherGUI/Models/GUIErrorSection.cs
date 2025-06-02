using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using QArantine.Code.Test;

namespace QArantineLauncher.Code.LauncherGUI.Models
{
    public class GUIErrorSection
    (
        string errorID,
        string description,
        string category,
        ObservableCollection<TestError> executionErrors
    ) : INotifyPropertyChanged
    {    
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _errorID = errorID;
        private string _description = description;
        private string _category = category;
        private ObservableCollection<TestError> _executionErrors = [..executionErrors];

        public string ErrorID { get => _errorID; set { _errorID = value; RaisePropertyChanged(nameof(ErrorID)); } }
        public string Description { get => _description; set { _description = value; RaisePropertyChanged(nameof(Description)); } }
        public string Category { get => _category; set { _category = value; RaisePropertyChanged(nameof(Category)); } }
        public ObservableCollection<TestError> ExecutionErrors { get => _executionErrors; set { _executionErrors = value; RaisePropertyChanged(nameof(ExecutionErrors)); } }
        public ICommand? OpenLogCommand { get; private set; }
        public ICommand? OpenTestOutputDirectoryCommand { get; private set; }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}