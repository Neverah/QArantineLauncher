using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class MessageBox : Window
    {

        public MessageBox()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MessageTextBlock.Text = "No message";
        }

        public MessageBox(string message)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MessageTextBlock.Text = message;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        public static async Task Show(string message, LogManager.LogLevel logLevel, Window? owner = null)
        {
            switch (logLevel)
            {
                case LogManager.LogLevel.FatalError:
                    LogFatalError(message);
                    break;

                case LogManager.LogLevel.Error:
                    LogError(message);
                    break;

                case LogManager.LogLevel.OK:
                    LogOK(message);
                    break;

                case LogManager.LogLevel.Warning:
                    LogWarning(message);
                    break;

                case LogManager.LogLevel.Debug:
                    LogDebug(message);
                    break;
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var messageBox = new MessageBox(message);
                if (owner == null)
                {
                    if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        await messageBox.ShowDialog(desktop.MainWindow!);
                }
                else
                {
                    await messageBox.ShowDialog(owner);
                }
            });
        }
    }
}
