using System.Text.Json.Serialization;
using QArantine.Code.Test;

namespace QArantineLauncher.Code.JsonContexts
{
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(TestResult))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class ProjectTesterJsonContext : JsonSerializerContext {}
}
