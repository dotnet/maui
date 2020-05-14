if (IsMac)
{
	if (!Directory.Exists ("/Library/Frameworks/Mono.framework/Versions/Current/Commands/"))
	{
		Item ("Mono", "6.8.0.123")
			.Source (_ => "https://download.mono-project.com/archive/6.8.0/macos-10-universal/MonoFramework-MDK-6.8.0.123.macos10.xamarin.universal.pkg");
	}
    
	ForceJavaCleanup();
	Item (XreItem.Java_OpenJDK_1_8_0_25);

	string releaseChannel = Environment.GetEnvironmentVariable ("CHANNEL");
	Console.WriteLine ("ANDROID_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC"));
	Console.WriteLine ("IOS_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("IOS_SDK_MAC"));
	Console.WriteLine ("MONO_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("MONO_SDK_MAC"));
	Console.WriteLine ("MAC_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("MAC_SDK_MAC"));
	Console.WriteLine ("releaseChannel: {0}", releaseChannel);

	bool specificSdkSet = false;

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MONO_SDK_MAC")))
	{
		Item ("Mono")
      		.Source (_ => Environment.GetEnvironmentVariable ("MONO_SDK_MAC"));

		specificSdkSet = true;
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC")))
	{
		Item ("Xamarin.Android")
      		.Source (_ => Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC"));

		specificSdkSet = true;
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("IOS_SDK_MAC")))
	{
		Item ("Xamarin.iOS")
      		.Source (_ => Environment.GetEnvironmentVariable ("IOS_SDK_MAC"));


		specificSdkSet = true;
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MAC_SDK_MAC")))
	{
		Item ("Xamarin.Mac")
      		.Source (_ => Environment.GetEnvironmentVariable ("MAC_SDK_MAC"));

		specificSdkSet = true;
	}
	
	if(!specificSdkSet)
	{
		if(releaseChannel == "Beta")
		{
			Item ("Xamarin.Mac")
				.Source (_ => "https://xamci.azurewebsites.net/dl/xamarin/xamarin-macios/d16-5-xcode11.5/PKG-Xamarin.Mac-notarized");

			Item ("Xamarin.iOS")
				.Source (_ => "https://xamci.azurewebsites.net/dl/xamarin/xamarin-macios/d16-5-xcode11.5/PKG-Xamarin.iOS-notarized");

		}
		else if(releaseChannel == "Preview")
		{
			XamarinChannel("Preview");
		}
		else if(releaseChannel == "Stable")
		{
			XamarinChannel("Stable");
		}
	}
}
else
{

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("ANDROID_SDK_WINDOWS")))
		Item ("Xamarin.Android")
      		.Source (_ => Environment.GetEnvironmentVariable ("ANDROID_SDK_WINDOWS"));

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("IOS_SDK_WINDOWS")))
		Item ("Xamarin.iOS")
      		.Source (_ => Environment.GetEnvironmentVariable ("IOS_SDK_WINDOWS"));

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MONO_SDK_WINDOWS")))
		Item ("Mono")
      		.Source (_ => Environment.GetEnvironmentVariable ("MONO_SDK_WINDOWS"));

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MAC_SDK_WINDOWS")))
		Item ("Xamarin.Mac")
      		.Source (_ => Environment.GetEnvironmentVariable ("MAC_SDK_WINDOWS"));

}

AndroidSdk()
	.ApiLevel((AndroidApiLevel)24)
	.ApiLevel((AndroidApiLevel)28)
	.ApiLevel((AndroidApiLevel)29)
	.SdkManagerPackage ("build-tools;29.0.3");
