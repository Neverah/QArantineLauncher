using System.Collections.ObjectModel;
using System.Text.Json;

using QArantineLauncher.Code.JsonContexts;

namespace QArantineLauncher.Code.Projects
{
    public class ProjectManager
    {
        public ObservableCollection<Project> ExistentProjects { get; set; }
        private readonly string SavedProjectsFilePath = "LauncherData/ProjectsData/SavedProjects.json";
        private static ProjectManager? _instance;
        private ProjectManager()
        {
            ExistentProjects = LoadSavedProjects();
        }

        public static ProjectManager Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public void InitEmptyExistentProjectsList()
        {
            ExistentProjects = [GetDefaultEmptyProject()];
        }

        public void SaveExistentProjects()
        {
            string jsonString = JsonSerializer.Serialize([.. ExistentProjects], ProjectJsonContext.Default.ListProject);

            string? directoryPath = Path.GetDirectoryName(SavedProjectsFilePath);
            if (directoryPath != null) Directory.CreateDirectory(directoryPath);

            File.WriteAllText(SavedProjectsFilePath, jsonString);
        }

        private ObservableCollection<Project> LoadSavedProjects()
        {
            if (File.Exists(SavedProjectsFilePath))
            {
                string jsonString = File.ReadAllText(SavedProjectsFilePath);

                List<Project>? projects = JsonSerializer.Deserialize(jsonString, ProjectJsonContext.Default.ListProject);
                if (projects != null) return [.. projects];
            }

            LogWarning($"The saved projects data file doesn't exist or is empty: {SavedProjectsFilePath}");
            return [GetDefaultEmptyProject()];
        }

        private static Project GetDefaultEmptyProject()
        {
            return new Project("DefaultEmptyProject", ".", "project", "QArantine/Tests", "", "", "");
        }
    }
}