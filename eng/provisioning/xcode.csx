#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System;
using System.Linq;

var desiredXcode = Environment.GetEnvironmentVariable("REQUIRED_XCODE");
if (string.IsNullOrEmpty(desiredXcode)) {
    Console.WriteLine("The environment variable 'REQUIRED_XCODE' must be exported and the value must be a valid value from the 'XreItem' enumeration.");
    return;
}

desiredXcode = desiredXcode.Replace("Xcode_", "").Replace("_", ".");
Console.WriteLine("Desired Xcode: {0}", desiredXcode);

// Find the best version
Item item;
if (desiredXcode == "Latest")
    item = XcodeBeta();
else if (desiredXcode == "Stable")
    item = XcodeStable();
else
    item = Xcode(desiredXcode);

// Fix up the case where the beta did not make it to the machine
var newVersion = TryMapBetaToStable(item.Version);
if (newVersion != item.Version)
    item = Xcode(newVersion);

Console.WriteLine("Selected version: {0}", item.Version);
item.XcodeSelect();

LogInstalledXcodes();

var appleSdkOverride = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Preferences", "Xamarin", "Settings.plist");
Item("Override Apple SDK Settings")
    .Condition(item => !File.Exists(appleSdkOverride) || GetSettingValue(appleSdkOverride, "AppleSdkRoot") != GetSelectedXcodePath())
    .Action(item =>
    {
        DeleteSafe(appleSdkOverride);
        CreateSetting(appleSdkOverride, "AppleSdkRoot", GetSelectedXcodePath());
        Console.WriteLine($"New VSMac iOS SDK Location: {GetSelectedXcodePath()}");
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

void SafeSymlink(string source, string destination)
{
    if (Directory.Exists(destination) || Config.DryRun)
        return;

    Console.WriteLine($"ln -sf {source} {destination}");
    Exec("/bin/ln", "-sf", source, destination);
    Console.WriteLine($"Symlink created: '{source}' links to '{destination}'");
}

string TryMapBetaToStable(string betaVersion)
{
    var index = item.Version.IndexOf("-beta");
    if (index == -1)
        return betaVersion;

    var stableVersion = item.Version.Substring(0, index);
    if (Directory.Exists($"/Applications/Xcode_{stableVersion}.app"))
        return stableVersion;

    if (stableVersion.EndsWith(".0"))
    {
        stableVersion = stableVersion.Substring(0, stableVersion.Length - 2);
        if (Directory.Exists($"/Applications/Xcode_{stableVersion}.app"))
            return stableVersion;
    }

    return betaVersion;
}