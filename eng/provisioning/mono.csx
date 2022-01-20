if (IsMac)
{
    if (!Directory.Exists ("/Library/Frameworks/Mono.framework/Versions/Current/Commands/"))
    {
        Item ("Mono", "6.12.0.162")
            .Source (_ => "https://download.visualstudio.microsoft.com/download/pr/a3d9b621-803d-4771-ae3e-2ee9aede2c77/a83590c0b053f9c354df960984cb1a1e/monoframework-mdk-6.12.0.162.macos10.xamarin.universal.pkg");
    }
}