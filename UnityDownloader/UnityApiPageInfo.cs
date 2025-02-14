using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiPageInfo([property: JsonPropertyName("hasNextPage")] bool HasNextPage);
