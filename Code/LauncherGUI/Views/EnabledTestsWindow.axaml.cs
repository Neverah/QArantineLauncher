using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia;

using QArantineLauncher.Code.Projects.Testing;
using QArantineLauncher.Code.LauncherGUI.Services;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class EnabledTestsWindow : Window
    {
        private readonly ProjectTester? _projectTester;
        private readonly Dictionary<string, bool>? _testsStatus;

        public EnabledTestsWindow()
        {
            InitializeComponent();

            WindowSettingsService.LoadWindowSize(this);

            Resized += EnableTestsWindow_Resized;
            PositionChanged += EnableTestsWindow_PosChanged;

            TestsListPanel = this.FindControl<StackPanel>("TestsListPanel");
            SearchTextBox = this.FindControl<TextBox>("SearchTextBox");
        }

        public EnabledTestsWindow(ProjectTester projectTester) : this()
        {
            _projectTester = projectTester;
            _testsStatus = _projectTester.GetExistentTestsList().ToDictionary(test => test, test => _projectTester.IsTestEnabled(test));
            PopulateTestsList();
            SearchTextBox.KeyUp += SearchTextBox_KeyUp;
        }

        private void PopulateTestsList(string filter = "")
        {
            TestsListPanel.Children.Clear();
            int index = 0;
            foreach (var test in _testsStatus!)
            {
                if (string.IsNullOrEmpty(filter) || test.Key.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    Grid grid = new();
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                    CheckBox checkBox = new() { IsChecked = test.Value };
                    Grid.SetColumn(checkBox, 0);

                    TextBlock textBlock = new() { Text = test.Key, Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(textBlock, 1);

                    checkBox.IsCheckedChanged += (s, e) => _testsStatus[test.Key] = checkBox.IsChecked ?? false;

                    grid.Children.Add(checkBox);
                    grid.Children.Add(textBlock);

                    // Alternar el color de fondo
                    if (index % 2 == 0)
                    {
                        grid.Background = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        grid.Background = new SolidColorBrush(Color.Parse("#1A1A1A"));
                    }

                    TestsListPanel.Children.Add(grid);
                    index++;
                }
            }
        }

        private void SearchTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            PopulateTestsList(SearchTextBox.Text ?? string.Empty);
        }

        private void EnableAllButton_Click(object? sender, RoutedEventArgs e)
        {
            SetAllTestsStatus(true);
        }

        private void DisableAllButton_Click(object? sender, RoutedEventArgs e)
        {
            SetAllTestsStatus(false);
        }

        private void SetAllTestsStatus(bool setEnabled)
        {
            foreach (string test in _testsStatus!.Keys.ToList())
            {
                if (string.IsNullOrEmpty(SearchTextBox.Text) || test.Contains(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase))
                {
                    _testsStatus[test] = setEnabled;
                }
            }
            PopulateTestsList(SearchTextBox.Text ?? string.Empty);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            foreach (var test in _testsStatus!)
            {
                if (test.Value)
                {
                    _projectTester?.EnableTest(test.Key);
                }
                else
                {
                    _projectTester?.DisableTest(test.Key);
                }
            }
            _projectTester?.SaveDisabledTestsToJson();
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void EnableTestsWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void EnableTestsWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}