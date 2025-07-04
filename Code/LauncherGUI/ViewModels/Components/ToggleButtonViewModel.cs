using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using QArantineLauncher.Code.LauncherGUI.ViewModels.Commands;

namespace QArantineLauncher.Code.LauncherGUI.ViewModels.Components
{
    public class ToggleButtonViewModel : INotifyPropertyChanged
    {
        private bool _internalValue;
        private readonly Func<bool>? _getter;
        private readonly Action<bool>? _setter;

        public bool IsToggled
        {
            get => _getter != null ? _getter() : _internalValue;
            set
            {
                if (IsToggled != value)
                {
                    if (_setter != null)
                        _setter(value);
                    else
                        _internalValue = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentIconPath));
                    Toggled?.Invoke(value);
                }
            }
        }

        public string IconPathActive { get; }
        public string IconPathInactive { get; }
        public string CurrentIconPath => IsToggled ? IconPathActive : IconPathInactive;

        public ICommand ToggleCommand { get; }
        public event Action<bool>? Toggled;

        // Constructors for internal state use
        public ToggleButtonViewModel(string iconPathActive, string iconPathInactive, bool initialState)
        {
            IconPathActive = iconPathActive;
            IconPathInactive = iconPathInactive;
            _internalValue = initialState;
            ToggleCommand = new RelayCommand(() => IsToggled = !IsToggled);
        }

        public ToggleButtonViewModel(string iconPathActive, string iconPathInactive, bool initialState, Action<bool>? toggledAction)
        {
            IconPathActive = iconPathActive;
            IconPathInactive = iconPathInactive;
            _internalValue = initialState;
            ToggleCommand = new RelayCommand(() => IsToggled = !IsToggled);
            Toggled += toggledAction;
        }

        // Constructors for external state use
        public ToggleButtonViewModel(string iconPathActive, string iconPathInactive, Func<bool> getter, Action<bool> setter)
        {
            IconPathActive = iconPathActive;
            IconPathInactive = iconPathInactive;
            _getter = getter;
            _setter = setter;
            ToggleCommand = new RelayCommand(() => IsToggled = !IsToggled);
        }

        public ToggleButtonViewModel(string iconPathActive, string iconPathInactive, Func<bool> getter, Action<bool> setter, Action<bool>? toggledAction)
        {
            IconPathActive = iconPathActive;
            IconPathInactive = iconPathInactive;
            _getter = getter;
            _setter = setter;
            ToggleCommand = new RelayCommand(() => IsToggled = !IsToggled);
            Toggled += toggledAction;
        }

        public void RefreshIconPath()
        {
            OnPropertyChanged(nameof(CurrentIconPath));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}