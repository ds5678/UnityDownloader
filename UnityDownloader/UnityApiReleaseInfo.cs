using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiReleaseInfo([property: JsonPropertyName("pageInfo")] UnityApiPageInfo PageInfo, [property: JsonPropertyName("edges")] UnityApiEdge[] Edges);
