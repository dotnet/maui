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

Item item;
if (desiredXcode == "Latest")
    item = XcodeBeta();
else if (desiredXcode == "Stable")
    item = XcodeStable();
else
    item = Xcode(desiredXcode);

TryMapBetaToStable(item);

// remove the double "0" as this has issues on the lookup
if (item.Version.Contains(".0.0-") || item.Version.EndsWith(".0.0"))
    item = Xcode(item.Version.Replace(".0.0", ".0"));

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

bool TryMapBetaToStable(Item item)
{
    var index = item.Version.IndexOf("-beta");
    if (index == -1)
        return false;

    var stablePath = "/Applications/Xcode_" + item.Version.Substring(0, index) + ".app";
    var betaPath = "/Applications/Xcode_" + item.Version.Replace("-", "_") + ".app";
    var betaNoPatchPath = "/Applications/Xcode_" + item.Version.Replace(".0.0-", ".0_") + ".app";
    var betaNoMinorPath = "/Applications/Xcode_" + item.Version.Replace(".0.0-", "_") + ".app";

    SafeSymlink(stablePath, betaPath);
    SafeSymlink(stablePath, betaNoPatchPath);
    SafeSymlink(stablePath, betaNoMinorPath);

    return true;
}