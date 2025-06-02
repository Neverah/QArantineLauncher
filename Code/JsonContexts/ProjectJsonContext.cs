using System.Text.Json.Serialization;
using QArantineLauncher.Code.Projects;

namespace QArantineLauncher.Code.JsonContexts
{
    [JsonSerializable(typeof(List<Project>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class ProjectJsonContext : JsonSerializerContext {}
}
