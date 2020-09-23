if (IsMac)
{
	System.Net.Http.HttpClient client = new System.Net.Http.HttpClient (new System.Net.Http.HttpClientHandler { AllowAutoRedirect = true });
	if (!Directory.Exists ("/Library/Frameworks/Mono.framework/Versions/Current/Commands/"))
	{
		Item ("Mono", "6.8.0.123")
			.Source (_ => "https://download.mono-project.com/archive/6.8.0/macos-10-universal/MonoFramework-MDK-6.8.0.123.macos10.xamarin.universal.pkg");
	}
    
	ForceJavaCleanup();
	OpenJDK ("1.8.0-40");

	string releaseChannel = Environment.GetEnvironmentVariable ("CHANNEL");
	Console.WriteLine ("ANDROID_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC"));
	Console.WriteLine ("IOS_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("IOS_SDK_MAC"));
	Console.WriteLine ("MONO_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("MONO_SDK_MAC"));
	Console.WriteLine ("MAC_SDK_MAC: {0}", Environment.GetEnvironmentVariable ("MAC_SDK_MAC"));
	Console.WriteLine ("releaseChannel: {0}", releaseChannel);

	bool specificSdkSet = false;

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MONO_SDK_MAC")))
	{
		await ResolveUrl (Environment.GetEnvironmentVariable ("MONO_SDK_MAC"));
		specificSdkSet = true;
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC")))
	{
		await ResolveUrl (Environment.GetEnvironmentVariable ("ANDROID_SDK_MAC"));
		specificSdkSet = true;
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("IOS_SDK_MAC")))
	{
		await ResolveUrl (Environment.GetEnvironmentVariable ("IOS_SDK_MAC"));
		specificSdkSet = true;
	}
	else
	{
		await ResolveUrl ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-7-xcode11.7/3016ffe2b0ee27bf4a2d61e6161430d6bbd62f78/7/package/notarized/xamarin.mac-6.20.3.5.pkg");
	}

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MAC_SDK_MAC")))
	{
		await ResolveUrl (Environment.GetEnvironmentVariable ("MAC_SDK_MAC"));
		specificSdkSet = true;
	}
	else
	{
		await ResolveUrl ("https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-7-xcode11.7/3016ffe2b0ee27bf4a2d61e6161430d6bbd62f78/7/package/notarized/xamarin.ios-13.20.3.5.pkg");
	}
	
	if(!specificSdkSet)
	{
		if(releaseChannel == "Beta")
		{
			Console.WriteLine("Installing Beta Channel");			
			await ResolveUrl ("https://xamci.azurewebsites.net/dl/xamarin/xamarin-macios/d16-6-xcode11.6/PKG-Xamarin.Mac-notarized");
			await ResolveUrl ("https://xamci.azurewebsites.net/dl/xamarin/xamarin-macios/d16-6-xcode11.6/PKG-Xamarin.iOS-notarized");
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

	async System.Threading.Tasks.Task ResolveUrl (string url)
	{
		// When downloading a package using the xamci we have to use the following code to 
		// install updates otherwise provionator can't tell the difference between a new package or an old one
		try
		{
			using (var response = await client.GetAsync (url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead)) {
				response.EnsureSuccessStatusCode ();
				Item(response.RequestMessage.RequestUri.ToString());
			}
		}
		catch{
			Item(url);
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

string ANDROID_API_SDKS = Environment.GetEnvironmentVariable ("ANDROID_API_SDKS");

if(String.IsNullOrWhiteSpace(ANDROID_API_SDKS))
{
	AndroidSdk()
		.ApiLevel((AndroidApiLevel)24)
		.ApiLevel((AndroidApiLevel)28)
		.ApiLevel((AndroidApiLevel)29)
		.SdkManagerPackage ("build-tools;29.0.3");
}
else{

	var androidSDK = AndroidSdk();
	foreach(var sdk in ANDROID_API_SDKS.Split(','))
	{
		Console.WriteLine("Installing SDK: {0}", sdk);
		androidSDK = androidSDK.SdkManagerPackage (sdk);
	}
}