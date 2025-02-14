using AssetRipper.Primitives;
using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiNode(
	[property: JsonPropertyName("version")] UnityVersion Version,
	[property: JsonPropertyName("shortRevision")] string ShortRevision,
	[property: JsonPropertyName("releaseDate")] string ReleaseDate,
	[property: JsonPropertyName("unityHubDeepLink")] string UnityHubDeepLink,
	[property: JsonPropertyName("stream")] string Stream)
{
	[JsonIgnore]
	public string Win64DownloadUrl => $@"https://download.unity3d.com/download_unity/{ShortRevision}/Windows64EditorInstaller/UnitySetup64-{Version}.exe";
}
