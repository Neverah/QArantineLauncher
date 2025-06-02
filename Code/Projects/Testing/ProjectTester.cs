using QArantine.Code.Test;
using System.Text.Json;

using QArantineLauncher.Code.JsonContexts;
using QArantineLauncher.Code.LauncherGUI.Views;

namespace QArantineLauncher.Code.Projects.Testing
{
    public class ProjectTester
    (
        Project ownerProject,
        string projectRootPath,
        string testsPath,
        string testsOutputPath
    )
    {
        public static readonly string TestNameKeyword = "[TestName]";
        public string ProjectRootPath { get; set; } = projectRootPath;
        public string TestsPath { get; set; } = testsPath;
        public string TestsOutputPath { get; set; } = testsOutputPath;
        public bool IsTesting = false;
        public bool WasLastProcessSuccesfull = false;
        public event EventHandler? TestingStarted;
        public event EventHandler? TestingEnded;
        public event EventHandler? TestResultDataAdded;
        public event EventHandler? TestResultsDataClear;
        public readonly Mutex ResultsDataListMutex = new();
        private readonly Project _ownerProject = ownerProject;
        private readonly Dictionary<string, bool> _enabledTests = [];
        private readonly string _disabledTestsFilePath = Path.Combine(testsPath, "DisabledTests.json");
        private bool _abortRequested = false;

        public async Task OnSelectedProjectChanged()
        {
            LoadDisabledTestsFromJson();
            await RecoverPreviousTestsResults();
        }

        public async Task StartTesting(bool keepCmdOpen = false)
        {
            _abortRequested = false;
            IsTesting = true;

            TestingStarted?.Invoke(this, EventArgs.Empty);

            CleanTestResultsData();

            (string, string)[] testsList = GetTestsList();

            // Name, DirectoryPath
            foreach((string, string) testData in testsList)
            {
                OnTestPreLaunch(testData.Item1, testData.Item2);
                await _ownerProject.pRunner.RunProgram(_ownerProject.LastExeType, true, keepCmdOpen, testData.Item1);
                if(_abortRequested) break;
                await OnTestPostLaunch(testData.Item1);
            }

            EndTesting();
        }

        private void EndTesting()
        {
            IsTesting = false;
            TestingEnded?.Invoke(this, EventArgs.Empty);
        }

        public void AbortTesting()
        {
            _abortRequested = true;
            _ownerProject.pRunner.AbortProgram();
        }

        public (string, string)[] GetTestsList()
        {
            if (!Directory.Exists(TestsPath)) return [];

            string[] files = Directory.GetFiles(TestsPath, "*.cs", SearchOption.AllDirectories);

            List<(string, string)> testNames = [];
            foreach (string file in files)
            {
                if (IsTestEnabled(GetTestRelativePathFromFullPath(file)))
                {
                    string testName = Path.GetFileNameWithoutExtension(file);
                    testNames.Add((testName, Path.GetDirectoryName(file)!));
                }
            }

            return [.. testNames];
        }

        public string[] GetExistentTestsList()
        {
            if (!Directory.Exists(TestsPath)) return [];

            string[] files = Directory.GetFiles(TestsPath, "*.cs", SearchOption.AllDirectories);
            return files.Select(GetTestRelativePathFromFullPath).ToArray();
        }

        private string GetTestRelativePathFromFullPath(string testFullPath)
        {
            string relativePath = testFullPath[TestsPath.Length..].TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(Path.GetDirectoryName(relativePath) ?? string.Empty, Path.GetFileNameWithoutExtension(relativePath));
        }

        private void OnTestPreLaunch(string testName, string testDirectoryPath)
        {
            int testsPathStartIndex = testDirectoryPath.IndexOf(_ownerProject.ProjectRootPath);
            if (testsPathStartIndex != -1) testDirectoryPath = testDirectoryPath.Remove(testsPathStartIndex, _ownerProject.ProjectRootPath.Length + 1);

            string sourcePath = Path.Combine(_ownerProject.LastWorkingDirectory, testDirectoryPath, $"{testName}.config");
            string goalPath = Path.Combine(_ownerProject.LastWorkingDirectory, "QArantine/Config/QArantineOverwrite.config");

            if(File.Exists(goalPath)) File.Delete(goalPath);
            if(File.Exists(sourcePath)) File.Copy(sourcePath, goalPath);
        }

        private async Task OnTestPostLaunch(string testName)
        {
            await AddTestResultData(testName);

            string expectedTestConfigFilePath = Path.Combine(_ownerProject.LastWorkingDirectory, "QArantine/Config/QArantineOverwrite.config");
            if (File.Exists(expectedTestConfigFilePath)) File.Delete(expectedTestConfigFilePath);
        }

        private async Task AddTestResultData(string testName)
        {
            TestResult? testResult = GetTestResultData(testName);
            if(testResult != null)
            {
                TestResultDataAdded?.Invoke(this, testResult);
            }
            else
            {
                await MessageBox.Show($"Error when trying to deserialize the test '{testName}' TestResult.json output file", LogManager.LogLevel.Error);
            }
        }

        public void CleanTestResultsData()
        {
            TestResultsDataClear?.Invoke(this, EventArgs.Empty);
        }

        public TestResult? GetTestResultData(string testName)
        {
            string path = Path.Combine(_ownerProject.LastWorkingDirectory, TestsOutputPath, testName, "TestResult.json");
            if (!File.Exists(path))
            {
                LogError($"Couldn't find the test '{testName}' output file: {path}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(path);
                return JsonSerializer.Deserialize<TestResult>(jsonContent, ProjectTesterJsonContext.Default.TestResult);
            }
            catch (Exception ex)
            {
                LogError($"Error when trying to read the test '{testName}' output file: {ex.Message}");
                return null;
            }
        }

        public void EnableTest(string testPath)
        {
            _enabledTests[testPath] = true;
        }

        public void DisableTest(string testPath)
        {
            _enabledTests[testPath] = false;
        }

        public bool IsTestEnabled(string testRelativePath)
        {
            return !_enabledTests.ContainsKey(testRelativePath) || _enabledTests[testRelativePath];
        }

        public void SaveDisabledTestsToJson()
        {
            List<string> disabledTests = _enabledTests.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();
            string json = JsonSerializer.Serialize(disabledTests, ProjectTesterJsonContext.Default.ListString);
            File.WriteAllText(_disabledTestsFilePath, json);
        }

        private void LoadDisabledTestsFromJson()
        {
            if (File.Exists(_disabledTestsFilePath))
            {
                string json = File.ReadAllText(_disabledTestsFilePath);
                var disabledTests = JsonSerializer.Deserialize(json, ProjectTesterJsonContext.Default.ListString);
                if (disabledTests != null)
                {
                    foreach (string testName in disabledTests) 
                        _enabledTests[testName] = false;
                }
            }
        }

        private async Task RecoverPreviousTestsResults()
        {
            CleanTestResultsData();

            (string, string)[] testsList = GetTestsList();

            foreach((string, string) testData in testsList)
            {
                if (File.Exists(Path.Combine(_ownerProject.LastWorkingDirectory, TestsOutputPath, testData.Item1, "TestResult.json")))
                {
                    await AddTestResultData(testData.Item1);
                }
            }
        }
    }
}