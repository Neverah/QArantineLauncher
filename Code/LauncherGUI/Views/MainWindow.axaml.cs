using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

using QArantineLauncher.Code.LauncherGUI.Services;
using QArantineLauncher.Code.LauncherGUI.ViewModels;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            WindowSettingsService.LoadWindowSize(this);

            // Suscripción a eventos
            Resized += MainWindow_Resized;
            PositionChanged += MainWindow_PosChanged;
            Loaded += MainWindow_Loaded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void MainWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Obtener una referencia al ScrollViewer de los logs
            ScrollViewer? cleaningLogScrollViewer = this.FindControl<ScrollViewer>("CleaningLogScrollViewer");
            ScrollViewer? buildLogScrollViewer = this.FindControl<ScrollViewer>("BuildLogScrollViewer");
            ScrollViewer? publishingLogScrollViewer = this.FindControl<ScrollViewer>("PublishLogScrollViewer");
            ScrollViewer? testingResultsScrollViewer = this.FindControl<ScrollViewer>("TestingResultsScrollViewer");
            
            // Verificar si DataContext no es nulo antes de intentar pasar la referencia al ViewModel
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (cleaningLogScrollViewer != null)
                    viewModel.SetCleaningLogScrollViewerReference(cleaningLogScrollViewer);

                if (buildLogScrollViewer != null)
                    viewModel.SetBuildLogScrollViewerReference(buildLogScrollViewer);

                if (publishingLogScrollViewer != null)
                    viewModel.SetPublishingLogScrollViewerReference(publishingLogScrollViewer);

                if (testingResultsScrollViewer != null)
                    viewModel.SetTestingResultsScrollViewerReference(testingResultsScrollViewer);

                // Asignar función de cambio de valor al ComboBox de selección de proyecto
                ComboBox? comboBox = this.FindControl<ComboBox>("ProjectSelectionComboBox");
                if (comboBox != null) comboBox.SelectionChanged += viewModel.ProjectSelectionComboBox_SelectionChanged;

                // Asignar función de cambio de valor al ComboBox de kill de proceso
                comboBox = this.FindControl<ComboBox>("ProcessSelectionComboBox");
                if (comboBox != null)
                {
                    comboBox.DropDownOpened += viewModel.ProcessSelectionComboBox_DropDownOpened;
                    comboBox.SelectionChanged += viewModel.ProcessSelectionComboBox_SelectionChanged;
                }
            }
        }
    }
}
