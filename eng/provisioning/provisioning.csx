if (IsMac)
{
	System.Net.Http.HttpClient client = new System.Net.Http.HttpClient (new System.Net.Http.HttpClientHandler { AllowAutoRedirect = true });
	if (!Directory.Exists ("/Library/Frameworks/Mono.framework/Versions/Current/Commands/"))
 	{
 		Item ("Mono", "6.12.0.127")
 			.Source (_ => "https://download.mono-project.com/archive/6.12.0/macos-10-universal/MonoFramework-MDK-6.12.0.107.macos10.xamarin.universal.pkg");
 	}
	ForceJavaCleanup();
	MicrosoftOpenJdk ("11.0.13.8.1");

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

	if(!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable ("MAC_SDK_MAC")))
	{
		await ResolveUrl (Environment.GetEnvironmentVariable ("MAC_SDK_MAC"));
		specificSdkSet = true;
	}
	
	if(!specificSdkSet)
	{
		if(releaseChannel == "Beta")
		{
			Console.WriteLine ("Beta channel doesn't exist on provisionator");
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
		.ApiLevel((AndroidApiLevel)21)
		.ApiLevel((AndroidApiLevel)22)
		.ApiLevel((AndroidApiLevel)23)
		.ApiLevel((AndroidApiLevel)24)
		.ApiLevel((AndroidApiLevel)28)
		.ApiLevel((AndroidApiLevel)29)
		.ApiLevel((AndroidApiLevel)30)
		.ApiLevel((AndroidApiLevel)31)
		.VirtualDevice(
			"Android_API23",
			(AndroidApiLevel)23,
			AndroidSystemImageApi.Google,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API24",
			AndroidApiLevel.Nougat,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API25",
			AndroidApiLevel.Nougat_7_1,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API26",
			AndroidApiLevel.Oreo,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API27",
			AndroidApiLevel.Oreo_8_1 ,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API28",
			AndroidApiLevel.P,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API29",
			AndroidApiLevel.Q,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice(
			"Android_API30",
			AndroidApiLevel.R,
			AndroidSystemImageApi.GooglePlayStore,
			AndroidSystemImageAbi.x86,
			AndroidVirtualDevice.NEXUS_5X)
		
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
