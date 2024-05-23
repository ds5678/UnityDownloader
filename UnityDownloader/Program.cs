namespace UnityDownloader;

internal static class Program
{
	private static readonly HashSet<string> versionsToSkip = ["5.1.0f2"];
	static void Main(string[] args)
	{
		if (args.Length != 1)
		{
			Console.WriteLine("Usage: UnityDownloader <destinationDirectory>");
			return;
		}

		string destinationDirectory = args[0];
		if (!Directory.Exists(destinationDirectory))
		{
			Console.WriteLine("Destination directory does not exist!");
			return;
		}

		string[] files = Directory.GetFiles(destinationDirectory, "*.exe").Select(f => Path.GetFileName(f)).ToArray();

		string pageData = FetchPageData().WaitForResult();
		foreach (UnityVersionData unityVersion in GetDownloadUrls(pageData))
		{
			if (versionsToSkip.Contains(unityVersion.Version))
				continue;

			string fileName = unityVersion.GetWindowsExeName();
			if (files.Contains(fileName))
				continue;

			Console.WriteLine(unityVersion.Version);
			Thread.Sleep(10000);
			HttpClient client = CreateHttpClient();
			Stream source = client.GetStreamAsync(unityVersion.GetWindowsUrl()).WaitForResult();
			using FileStream destination = File.Create(Path.Combine(destinationDirectory, fileName));
			source.CopyTo(destination);
		}
		Console.WriteLine("Done!");
	}

	private static Task<string> FetchPageData()
	{
		HttpClient client = CreateHttpClient();
		client.Timeout = TimeSpan.FromSeconds(30);
		return client.GetStringAsync("https://unity.com/releases/editor/archive");
	}

	private static HttpClient CreateHttpClient()
	{
		HttpClient client = new();
		const string userAgent = "UnityDownloader/1.0";
		client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
		return client;
	}

	private static IEnumerable<UnityVersionData> GetDownloadUrls(string pageContent)
	{
		HashSet<string> unityVersions = [];
		string[] pageLines = pageContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

		foreach (string line in pageLines)
		{
			if (string.IsNullOrWhiteSpace(line) || line.Contains("Samsung"))
				continue;

			const string hrefIdentifier = """<a href="https://download.unity3d.com/""";
			int hrefIdentifierIndex = line.IndexOf(hrefIdentifier);
			if (hrefIdentifierIndex < 0)
				continue;

			string setupIdentifier;
			bool is64Bit;
			if (line.Contains(UnityVersionData.SetupIdentifier64Bit))
			{
				setupIdentifier = UnityVersionData.SetupIdentifier64Bit;
				is64Bit = true;
			}
			else if (line.Contains(UnityVersionData.SetupIdentifier32Bit))
			{
				setupIdentifier = UnityVersionData.SetupIdentifier32Bit;
				is64Bit = false;
			}
			else
			{
				continue;
			}

			string subline = line.Substring(hrefIdentifierIndex + hrefIdentifier.Length);
			int extensionIndex = subline.IndexOf(".exe");
			if (extensionIndex < 0)
				continue;

			string foundVersion = subline.Substring(0, extensionIndex);
			foundVersion = foundVersion.Substring(foundVersion.LastIndexOf(setupIdentifier) + setupIdentifier.Length);

			string versionId = subline.Split('/')[1];

			if (unityVersions.Add(foundVersion))
			{
				yield return new UnityVersionData(foundVersion, is64Bit, versionId);
			}
		}
	}
}
