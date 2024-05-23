namespace UnityDownloader;

public readonly record struct UnityVersionData(string Version, bool Is64Bit, string VersionId)
{
	public const string SetupIdentifier64Bit = "UnitySetup64-";
	public const string SetupIdentifier32Bit = "UnitySetup-";

	public string GetWindowsUrl()
	{
		//Example: https://download.unity3d.com/download_unity/5b98b70ebeb9/Windows64EditorInstaller/UnitySetup64-5.0.0f4.exe

		string middle = Is64Bit ? $"Windows64EditorInstaller/{SetupIdentifier64Bit}" : $"WindowsEditorInstaller/{SetupIdentifier32Bit}";

		return $"https://download.unity3d.com/download_unity/{VersionId}/{middle}{Version}.exe";
	}
}
