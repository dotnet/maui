#r "_provisionator/provisionator.dll"

using static Xamarin.Provisioning.ProvisioningScript;

using System;
using System.Linq;

var desiredXcode = Environment.GetEnvironmentVariable("REQUIRED_XCODE");
if (string.IsNullOrEmpty(desiredXcode)) {
    Console.WriteLine("The environment variable 'REQUIRED_XCODE' must be exported and the value must be a valid value from the 'XreItem' enumeration.");
    return;
}

//desiredXcode = desiredXcode.Replace("Xcode_", "").Replace("_", ".");
Console.WriteLine("Desired Xcode: {0}", desiredXcode);

Item item = Xcode(desiredXcode);

Console.WriteLine("Selected version: {0}", item.Version);
item.XcodeSelect() 
        .SimulatorRuntime(SimRuntime.iOS);

LogInstalledXcodes();
