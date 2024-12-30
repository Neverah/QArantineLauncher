using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using QArantineLauncher.Code.Projects;

using QArantineLauncher.Code.LauncherGUI.Services;

namespace QArantineLauncher.Code.LauncherGUI.Views
{
    public partial class ProjectCreationWindow : Window
    {
        public ProjectCreationWindow()
        {
            InitializeComponent();

            WindowSettingsService.LoadWindowSize(this);

            Resized += ProjectCreationWindow_Resized;
            PositionChanged += ProjectCreationWindow_PosChanged;

            Button? CreateButton = this.FindControl<Button>("CreateButton");
            if (CreateButton != null) CreateButton.Click += CreateButton_Click;

            Button? CancelButton = this.FindControl<Button>("CancelButton");
            if (CancelButton != null) CancelButton.Click += CancelButton_Click;

            NameTextBox = this.FindControl<TextBox>("NameTextBox");
            ProjectRootPathTextBox = this.FindControl<TextBox>("ProjectRootPathTextBox");
            ExeFileNameTextBox = this.FindControl<TextBox>("ExeFileNameTextBox");
            TestsPathTextBox = this.FindControl<TextBox>("TestsPathTextBox");
            TestsOutputPathTextBox = this.FindControl<TextBox>("TestsOutputPathTextBox");
            RunParamsTextBox = this.FindControl<TextBox>("RunParamsTextBox");
            BuildOutputPathTextBox = this.FindControl<TextBox>("BuildOutputPathTextBox");
            PublishingOutputPathTextBox = this.FindControl<TextBox>("PublishingOutputPathTextBox");
            AdditionalCopyFilesTextBox = this.FindControl<TextBox>("AdditionalCopyFilesTextBox");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ProjectCreationWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void ProjectCreationWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private async void CreateButton_Click(object? sender, RoutedEventArgs e)
        {

            string? name = NameTextBox.Text;
            string? projectRootPath = ProjectRootPathTextBox.Text;
            string? exeFileName = ExeFileNameTextBox.Text;
            string? testsPath = TestsPathTextBox.Text;
            string? testsOutputPath = TestsOutputPathTextBox.Text;
            string? runParams = RunParamsTextBox.Text;
            string? buildOutputPath = BuildOutputPathTextBox.Text;
            string? publishingOutputPath = PublishingOutputPathTextBox.Text;
            string? additionalCopyFiles = AdditionalCopyFilesTextBox.Text;

            if (string.IsNullOrWhiteSpace(name) || 
                string.IsNullOrWhiteSpace(projectRootPath) || 
                string.IsNullOrWhiteSpace(exeFileName) || 
                string.IsNullOrWhiteSpace(testsPath) || 
                string.IsNullOrWhiteSpace(testsOutputPath))
            {
                await MessageBox.Show("Fields 'Project Name', 'Project Root Path', 'Exe File Name', 'Tests Path' and 'Tests Output Path' are required.", LogManager.LogLevel.Error, this);
                return;
            }

            Project project = new(
                name,
                projectRootPath,
                exeFileName,
                testsPath,
                testsOutputPath,
                buildOutputPath ?? "",
                publishingOutputPath ?? "",
                additionalCopyFiles ?? "",
                runParams ?? ""
            );

            if (ProjectManager.Instance.ExistentProjects.Contains(project))
            {
                await MessageBox.Show($"The project '{project.Name}' already exists.", LogManager.LogLevel.Error, this);
                return;
            }

            ProjectManager.Instance.ExistentProjects.Add(project);
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
