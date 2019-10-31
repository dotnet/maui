
string monoMajorVersion = "6.4.0";
string monoPatchVersion = "198";
string monoVersion = $"{monoMajorVersion}.{monoPatchVersion}";

string monoSDK_windows = $"https://download.mono-project.com/archive/{monoMajorVersion}/windows-installer/mono-{monoVersion}-x64-0.msi";
string androidSDK_windows = "https://download.visualstudio.microsoft.com/download/pr/1131a8f5-99f5-4326-93b1-f5827b54ecd5/e7bd0f680004131157a22982c389b05f2d3698cc04fab3901ce2d7ded47ad8e0/Xamarin.Android.Sdk-10.0.0.43.vsix";
string iOSSDK_windows = "";
string macSDK_windows = "";

string androidSDK_macos = "https://download.visualstudio.microsoft.com/download/pr/d5a432e4-09f3-4da6-9bdd-1d4fdd87f34c/c4ce0854064ffc16b957f22ccc08f9df/xamarin.android-10.0.0.43.pkg";
string monoSDK_macos = $"https://download.mono-project.com/archive/{monoMajorVersion}/macos-10-universal/MonoFramework-MDK-{monoVersion}.macos10.xamarin.universal.pkg";
string iOSSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-3/5e8a208b5f44c4885060d95e3c3ad68d6a5e95e8/40/package/xamarin.ios-13.2.0.42.pkg";
string macSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-3/5e8a208b5f44c4885060d95e3c3ad68d6a5e95e8/40/package/xamarin.mac-6.2.0.42.pkg";

if (IsMac)
{
	Item (XreItem.Xcode_11_1_0_rc).XcodeSelect ();

  if(!String.IsNullOrEmpty(monoSDK_macos))
    Item ("Mono", monoVersion)
      .Source (_ => monoSDK_macos);

	if(!String.IsNullOrEmpty(androidSDK_macos))
		Item ("Xamarin.Android", "10.0.0.43")
      .Source (_ => androidSDK_macos);

	if(!String.IsNullOrEmpty(iOSSDK_macos))
		Item ("Xamarin.iOS", "13.2.0.42")
      .Source (_ => iOSSDK_macos);

	if(!String.IsNullOrEmpty(macSDK_macos))
		Item ("Xamarin.Mac", "6.2.0.42")
      .Source (_ => macSDK_macos);
    
	ForceJavaCleanup();

    var dotnetVersion = System.Environment.GetEnvironmentVariable("DOTNET_VERSION");
    if (!string.IsNullOrEmpty(dotnetVersion))
	  {
		// VSTS installs into a non-default location. Let's hardcode it here because why not.
		var vstsBaseInstallPath = Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".dotnet", "sdk");
		var vstsInstallPath = Path.Combine (vstsBaseInstallPath, dotnetVersion);
		var defaultInstallLocation = Path.Combine ("/usr/local/share/dotnet/sdk/", dotnetVersion);
		if (Directory.Exists (vstsBaseInstallPath) && !Directory.Exists (vstsInstallPath))
			ln (defaultInstallLocation, vstsInstallPath);
	  }
}
else
{
	if(!String.IsNullOrEmpty(androidSDK_windows))
		Item ("Xamarin.Android", "10.0.0.43")
      .Source (_ => androidSDK_windows);

	if(!String.IsNullOrEmpty(iOSSDK_windows))
		Item ("Xamarin.iOS", "13.2.0.42")
      .Source (_ => iOSSDK_windows);

	if(!String.IsNullOrEmpty(macSDK_windows))
		Item ("Xamarin.Mac", "6.2.0.42")
      .Source (_ => macSDK_windows);

	if(!String.IsNullOrEmpty(monoSDK_windows))
    Item ("Mono", monoVersion)
      .Source (_ => monoSDK_windows);

}

Item(XreItem.Java_OpenJDK_1_8_0_25);
AndroidSdk ().ApiLevel((AndroidApiLevel)24);
AndroidSdk ().ApiLevel((AndroidApiLevel)28);
AndroidSdk ().ApiLevel((AndroidApiLevel)29);

void ln (string source, string destination)
{
	Console.WriteLine ($"ln -sf {source} {destination}");
	if (!Config.DryRun)
		Exec ("/bin/ln", "-sf", source, destination);
}
