using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using QArantine.Code.Test;
using QArantineLauncher.Code.LauncherGUI.Models;

namespace QArantineLauncher.Code.JsonContexts
{
    [JsonSerializable(typeof(ObservableCollection<TestError>))]
    [JsonSerializable(typeof(ObservableCollection<GUIErrorSection>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class TestReportJsonContext : JsonSerializerContext {}
}
