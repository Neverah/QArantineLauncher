using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using QArantineLauncher.Code.Projects;

using QArantineLauncher.Code.LauncherGUI.Services;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class ProjectUpdateWindow : Window
    {
        private readonly Project? _updatingProject;

        public ProjectUpdateWindow()
        {
            InitializeComponent();

            WindowSettingsService.LoadWindowSize(this);

            Resized += ProjectUpdateWindow_Resized;
            PositionChanged += ProjectUpdateWindow_PosChanged;

            Button? UpdateButton = this.FindControl<Button>("UpdateButton");
            if (UpdateButton != null) UpdateButton.Click += UpdateButton_Click;

            Button? CancelButton = this.FindControl<Button>("CancelButton");
            if (CancelButton != null) CancelButton.Click += CancelButton_Click;

            NameTextBlock = this.FindControl<TextBlock>("NameTextBlock");

            ProjectRootPathTextBox = this.FindControl<TextBox>("ProjectRootPathTextBox");
            ExeFileNameTextBox = this.FindControl<TextBox>("ExeFileNameTextBox");
            TestsPathTextBox = this.FindControl<TextBox>("TestsPathTextBox");
            TestsOutputPathTextBox = this.FindControl<TextBox>("TestsOutputPathTextBox");
            RunParamsTextBox = this.FindControl<TextBox>("RunParamsTextBox");
            BuildOutputPathTextBox = this.FindControl<TextBox>("BuildOutputPathTextBox");
            PublishingOutputPathTextBox = this.FindControl<TextBox>("PublishingOutputPathTextBox");
            AdditionalCopyFilesTextBox = this.FindControl<TextBox>("AdditionalCopyFilesTextBox");
        }

        public ProjectUpdateWindow(Project project) : this()
        {
            _updatingProject = project;

            NameTextBlock.Text = NameTextBlock.Text + project.Name ?? "?";
            ProjectRootPathTextBox.Text = project.ProjectRootPath ?? "";
            ExeFileNameTextBox.Text = project.ExeFileName ?? "";
            TestsPathTextBox.Text = project.TestsPath ?? "";
            TestsOutputPathTextBox.Text = project.TestsOutputPath ?? "";
            RunParamsTextBox.Text = project.RunParams ?? "";
            BuildOutputPathTextBox.Text = project.BuildOutputPath ?? "";
            PublishingOutputPathTextBox.Text = project.PublishingOutputPath ?? "";
            AdditionalCopyFilesTextBox.Text = project.AdditionalCopyFiles ?? "";
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ProjectUpdateWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void ProjectUpdateWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private async void UpdateButton_Click(object? sender, RoutedEventArgs e)
        {

            string? projectRootPath = ProjectRootPathTextBox.Text;
            string ? exeFileName = ExeFileNameTextBox.Text;
            string? testsPath = TestsPathTextBox.Text;
            string? testsOutputPath = TestsOutputPathTextBox.Text;
            string? runParams = RunParamsTextBox.Text;
            string? buildOutputPath = BuildOutputPathTextBox.Text;
            string? publishingOutputPath = PublishingOutputPathTextBox.Text;
            string? additionalCopyFiles = AdditionalCopyFilesTextBox.Text;

            if (string.IsNullOrWhiteSpace(projectRootPath) || 
                string.IsNullOrWhiteSpace(exeFileName) || 
                string.IsNullOrWhiteSpace(testsPath) ||
                string.IsNullOrWhiteSpace(testsOutputPath))
            {
                await MessageBox.Show("Fields 'Project Root Path', 'Exe File Name', 'Tests Path' and 'Tests Output Path' are required.", LogManager.LogLevel.Error, this);
                return;
            }

            if (_updatingProject != null)
            {
                _updatingProject.ProjectRootPath = projectRootPath;
                _updatingProject.ExeFileName = exeFileName;
                _updatingProject.TestsPath = testsPath;
                _updatingProject.TestsOutputPath = testsOutputPath;
                _updatingProject.RunParams = runParams ?? "";
                _updatingProject.BuildOutputPath = buildOutputPath ?? "";
                _updatingProject.PublishingOutputPath = publishingOutputPath ?? "";
                _updatingProject.AdditionalCopyFiles = additionalCopyFiles ?? "";
            }

            ProjectManager.Instance.SaveExistentProjects();

            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BrowseForProjectRootPath(object sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions{ Title = "Select a folder", AllowMultiple = false});
            if (folder != null && folder.Count > 0) ProjectRootPathTextBox.Text = folder[0]?.Path.LocalPath ?? string.Empty;
        }

        private async void BrowseForTestsPath(object sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions{ Title = "Select a folder", AllowMultiple = false});
            if (folder != null && folder.Count > 0) TestsPathTextBox.Text = folder[0]?.Path.LocalPath ?? string.Empty;
        }

        private async void BrowseForTestsOutputPath(object sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions{ Title = "Select a folder", AllowMultiple = false});
            if (folder != null && folder.Count > 0) TestsOutputPathTextBox.Text = folder[0]?.Path.LocalPath ?? string.Empty;
        }

        private async void BrowseForBuildOutputPath(object sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions{ Title = "Select a folder", AllowMultiple = false});
            if (folder != null && folder.Count > 0) BuildOutputPathTextBox.Text = folder[0]?.Path.LocalPath ?? string.Empty;
        }

        private async void BrowseForPublishingOutputPath(object sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions{ Title = "Select a folder", AllowMultiple = false});
            if (folder != null && folder.Count > 0) PublishingOutputPathTextBox.Text = folder[0]?.Path.LocalPath ?? string.Empty;
        }
    }
}
