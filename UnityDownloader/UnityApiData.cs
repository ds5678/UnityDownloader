using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiData([property: JsonPropertyName("getUnityReleases")] UnityApiReleaseInfo ReleaseInfo);
