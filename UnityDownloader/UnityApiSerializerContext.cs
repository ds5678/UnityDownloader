using System.Text.Json.Serialization;

namespace UnityDownloader;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UnityApiResponse))]
[JsonSerializable(typeof(List<UnityApiNode>))]
public sealed partial class UnityApiSerializerContext : JsonSerializerContext
{
}
