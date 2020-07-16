using System;
using System.Linq;

var desiredXcode = Environment.GetEnvironmentVariable ("REQUIRED_XCODE");
if (string.IsNullOrEmpty (desiredXcode)) {
	Console.WriteLine ("The environment variable 'REQUIRED_XCODE' must be exported and the value must be a valid value from the 'XreItem' enumeration.");
	return;
}

XreItem xreItem;

if(desiredXcode == "Latest")
	xreItem = (XreItem)Enum.GetValues(typeof(XreItem)).Cast<int>().Max();
else
	xreItem = (XreItem) Enum.Parse (typeof (XreItem), desiredXcode);

var item = Item (xreItem);
Console.WriteLine ("InstallPath: {0}", item.Version);
item.XcodeSelect ();