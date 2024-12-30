using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class ConfirmationWindow : Window
    {
        private Action _confirmationCallback;

        public ConfirmationWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MessageTextBlock.Text = "";
            _confirmationCallback = () => {};
        }

        public ConfirmationWindow(string message, Action confirmationCallback)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MessageTextBlock.Text = message;
            _confirmationCallback = confirmationCallback;
        }

        private void ConfirmButton_Click(object? sender, RoutedEventArgs e)
        {
            _confirmationCallback();
            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        public static async Task Show(string message, Action confirmationCallback)
        {
            ConfirmationWindow confirmationWindow = new(message, confirmationCallback);
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await confirmationWindow.ShowDialog(desktop.MainWindow!);
            }
            else
            {
                LogError($"No MainWindow found to call 'ShowDialog' for the new ConfirmationWindow");
            }
        }
    }
}
