using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiEdge([property: JsonPropertyName("node")] UnityApiNode Node);
