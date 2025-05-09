using System.Text.Json;

namespace UnityDownloader;

internal static class Program
{
	private static readonly HashSet<string> versionsToSkip =
	[
		"5.1.0b3",
		"5.1.0b4",
		"5.1.0b5",
		"5.1.0b6",
		"5.1.0f2",
	];
	static void Main(string[] args)
	{
		if (args.Length < 1)
		{
			PrintUsage();
			return;
		}

		switch (args[0])
		{
			case "identify":
				{
					string outputPath = args.Length > 1 ? args[1] : "versions.json";

					List<UnityApiNode> nodes = GetAllVersions();

					string result = JsonSerializer.Serialize(nodes, UnityApiSerializerContext.Default.ListUnityApiNode);

					File.WriteAllText(outputPath, result);
				}
				break;
			case "download" when args.Length >= 2:
				{
					string destinationDirectory = args[1];
					if (!Directory.Exists(destinationDirectory))
					{
						Console.WriteLine("Destination directory does not exist!");
						return;
					}

					string inputPath = args.Length > 2 ? args[2] : "versions.json";
					List<UnityApiNode>? nodes = JsonSerializer.Deserialize(File.ReadAllText(inputPath), UnityApiSerializerContext.Default.ListUnityApiNode);
					if (nodes == null)
					{
						Console.WriteLine("Failed to deserialize versions!");
						return;
					}

					Download(destinationDirectory, nodes);
					Console.WriteLine("Done!");
				}
				break;
			case "both" when args.Length >= 2:
				{
					string destinationDirectory = args[1];
					if (!Directory.Exists(destinationDirectory))
					{
						Console.WriteLine("Destination directory does not exist!");
						return;
					}

					Console.WriteLine("Getting the list of versions...");
					List<UnityApiNode> nodes = GetAllVersions();

					Console.WriteLine("Downloading each missing version...");
					Download(destinationDirectory, nodes);

					string outputPath = args.Length > 2 ? args[2] : "versions.json";
					string result = JsonSerializer.Serialize(nodes, UnityApiSerializerContext.Default.ListUnityApiNode);
					File.WriteAllText(outputPath, result);

					Console.WriteLine("Done!");
				}
				break;
			default:
				PrintUsage();
				return;
		}
	}

	private static void Download(string destinationDirectory, List<UnityApiNode> nodes)
	{
		string[] files = Directory.GetFiles(destinationDirectory, "*.exe").Select(f => Path.GetFileName(f)).ToArray();
		foreach (UnityApiNode unityVersion in nodes)
		{
			if (versionsToSkip.Contains(unityVersion.Version.ToString()))
				continue;
			string fileName = $"UnitySetup64-{unityVersion.Version}.exe";
			if (files.Contains(fileName))
				continue;
			Console.WriteLine(unityVersion.Version);
			Thread.Sleep(10000);
			HttpClient client = CreateHttpClient();
			Stream source;
			try
			{
				source = client.GetStreamAsync(unityVersion.Win64DownloadUrl).WaitForResult();
			}
			catch
			{
				Console.WriteLine($"Failed to download {unityVersion.Version}");
				continue;
			}
			using FileStream destination = File.Create(Path.Combine(destinationDirectory, fileName));
			source.CopyTo(destination);
		}
	}

	private static void PrintUsage()
	{
		Console.WriteLine("Usage:");
		Console.WriteLine("\tUnityDownloader identify <output json>?");
		Console.WriteLine("\tUnityDownloader download <destinationDirectory> <input json>?");
		Console.WriteLine("\tUnityDownloader both <destinationDirectory> <output json>?");
	}

	private static HttpClient CreateHttpClient()
	{
		HttpClient client = new();
		const string userAgent = "UnityDownloader/1.0";
		client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
		return client;
	}

	private static async Task<string?> GetResponseAsync(HttpClient client, int maxVersions = 100, int skip = 0)
	{
		const string Url = @"https://services.unity.com/graphql";

		string content = $$"""
			{
				"query":"query GetVersions($limit:Int!,$skip:Int!){getUnityReleases(limit:$limit,skip:$skip,entitlements:[XLTS]){pageInfo{hasNextPage}edges{node{version,shortRevision,releaseDate,unityHubDeepLink,stream} } } }",
				"operationName":"GetVersions",
				"variables":{
					"limit": {{maxVersions}},
					"skip": {{skip}}
				}
			}
			""";

		var response = await client.PostAsync(Url, new StringContent(content, null, "application/json"));
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Failed to get response: {response.StatusCode}");
			return null;
		}
		return await response.Content.ReadAsStringAsync();
	}

	private static List<UnityApiNode> GetAllVersions()
	{
		return GetAllVersions(CreateHttpClient()).WaitForResult();
	}

	private static async Task<List<UnityApiNode>> GetAllVersions(HttpClient client)
	{
		List<UnityApiNode> nodes = new();
		int skip = 0;
		const int maxVersions = 100;
		while (true)
		{
			string? response = await GetResponseAsync(client, maxVersions, skip);
			if (response == null)
				break;
			UnityApiResponse? unityResponse = JsonSerializer.Deserialize(response, UnityApiSerializerContext.Default.UnityApiResponse);
			if (unityResponse == null)
				break;
			nodes.AddRange(unityResponse.Nodes);
			if (!unityResponse.HasNextPage)
				break;
			skip += maxVersions;

			await Task.Delay(5000);
		}

		nodes.Sort((x, y) => x.Version.CompareTo(y.Version));
		return nodes;
	}
}
