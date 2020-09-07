#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System;
using System.Linq;

var desiredXcode = Environment.GetEnvironmentVariable ("REQUIRED_XCODE");
if (string.IsNullOrEmpty (desiredXcode)) {
	Console.WriteLine ("The environment variable 'REQUIRED_XCODE' must be exported and the value must be a valid value from the 'XreItem' enumeration.");
	return;
}

desiredXcode = desiredXcode.Replace("Xcode_", "").Replace("_", ".");

Item item;

if(desiredXcode == "Latest")
{
	item = XcodeBeta();

	if(item.Version.StartsWith("12.0.0-beta."))
	{
		Console.WriteLine ("CheckInstalledDevOpsBetaXcodeAndSymlink");
		int expectedBeta = Convert.ToInt32(item.Version.Replace("12.0.0-beta.", ""));
    	CheckInstalledDevOpsBetaXcodeAndSymlink("17200.1", expectedBeta: expectedBeta);
	}
}
else if (desiredXcode == "Stable")
	item = XcodeStable();
else
	item = Xcode(desiredXcode);

Console.WriteLine ("InstallPath: {0}", item.Version);
item.XcodeSelect ();

LogInstalledXcodes();

var appleSdkOverride = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Preferences", "Xamarin", "Settings.plist");
Item("Override Apple SDK Settings")
    .Condition(item => !File.Exists(appleSdkOverride) || GetSettingValue(appleSdkOverride, "AppleSdkRoot") != GetSelectedXcodePath())
    .Action (item => {
        DeleteSafe(appleSdkOverride);
        CreateSetting(appleSdkOverride, "AppleSdkRoot", GetSelectedXcodePath ());
        Console.WriteLine($"New VSMac iOS SDK Location: {GetSelectedXcodePath ()}");
    });



void DeleteSafe(string file)
{
    if (File.Exists(file))
        File.Delete(file);
}

void CreateSetting(string settingFile, string key, string value)
{
    Exec("defaults", "write", settingFile, key, value);
}

string GetSettingValue(string settingFile, string keyName)
{
    return Exec("defaults", "read", settingFile, keyName).FirstOrDefault();
}

bool CheckInstalledDevOpsBetaXcodeAndSymlink (string expectedBundleVersion, int expectedBeta)
{
    var devOpsXcodeBetaPath = "/Applications/Xcode_12_beta.app";
    if (!Directory.Exists (devOpsXcodeBetaPath))
        return false;

    var infoPlist = Plist (devOpsXcodeBetaPath);
    var bundleVersion = (string)infoPlist.CFBundleVersion;
    if (bundleVersion != expectedBundleVersion)
        return false;

    if (RunningInCI) {
        SafeSymlink (devOpsXcodeBetaPath, $"/Applications/Xcode_12.0.0-beta{expectedBeta}.app");
        SafeSymlink (devOpsXcodeBetaPath, $"/Applications/Xcode_12_beta_{expectedBeta}.app");
    }

    Console.WriteLine ($"CFBundleVersion found: {bundleVersion}");
    return true;
}

void SafeSymlink (string source, string destination)
{
    if (Directory.Exists (destination) || Config.DryRun)
        return;

    Console.WriteLine ($"ln -sf {source} {destination}");
    Exec ("/bin/ln", "-sf", source, destination);
    Console.WriteLine ($"Symlink created: '{source}' links to '{destination}'");
}