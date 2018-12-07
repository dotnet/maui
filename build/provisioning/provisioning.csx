
var channel = Env("PROVISIONATOR_XAMARIN_CHANNEL") ?? "Stable";

if (IsMac)
{
  Item (XreItem.Xcode_10_1_0).XcodeSelect ();
}
XamarinChannel(channel);