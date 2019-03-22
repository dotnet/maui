var channel = Env("CHANNEL") ?? "Stable";

if (IsMac)
{
  Item (XreItem.Xcode_10_1_0).XcodeSelect ();
}
Console.WriteLine(channel);
XamarinChannel(channel);