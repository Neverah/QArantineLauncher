using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using QArantineLauncher.Code.LauncherGUI.Services;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class TestReportWindow : Window
    {
        public TestReportWindow()
        {
            InitializeComponent();
            
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            WindowSettingsService.LoadWindowSize(this);

            Resized += TestReportWindow_Resized;
            PositionChanged += TestReportWindow_PosChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TestReportWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void TestReportWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}
