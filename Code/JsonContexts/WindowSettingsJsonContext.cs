using System.Text.Json.Serialization;
using QArantineLauncher.Code.LauncherGUI.Models;

namespace QArantineLauncher.Code.JsonContexts
{
    [JsonSerializable(typeof(List<WindowSettings>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class WindowSettingsJsonContext : JsonSerializerContext {}
}
