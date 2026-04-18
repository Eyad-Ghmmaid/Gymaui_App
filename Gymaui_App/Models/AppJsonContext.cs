using System.Text.Json.Serialization;

namespace Gymaui_App.Models
{
    [JsonSerializable(typeof(List<Exercise>))]
    internal partial class AppJsonContext : JsonSerializerContext
    {
    }
}
