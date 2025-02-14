using System.Text.Json.Serialization;

namespace UnityDownloader;

public sealed record class UnityApiResponse([property: JsonPropertyName("data")] UnityApiData Data)
{
	[JsonIgnore]
	public bool HasNextPage => Data.ReleaseInfo.PageInfo.HasNextPage;

	[JsonIgnore]
	public IEnumerable<UnityApiNode> Nodes => Data.ReleaseInfo.Edges.Select(e => e.Node);
}
