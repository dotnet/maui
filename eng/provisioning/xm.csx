if (IsMac)
{
    if (!Directory.Exists ("/Library/Frameworks/Xamarin.Mac.framework/"))
    {
        Item ("Xamarin.Mac", "8.4.0.0")
            .Source (_ => "https://download.visualstudio.microsoft.com/download/pr/f4f0db99-f0b3-41c5-a57d-b94bbe1226f3/e7c016f4fa32015b625112a27008663a/xamarin.mac-8.4.0.0.pkg");
    }
}