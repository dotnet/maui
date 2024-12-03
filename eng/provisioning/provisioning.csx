if (IsMac)
{
	ForceJavaCleanup();
	MicrosoftOpenJdk ("17.0.12");
	//this is needed for tools on macos like for nuget pack additional target and for classic xamarin projects
	Item("https://download.mono-project.com/archive/6.12.0/macos-10-universal/MonoFramework-MDK-6.12.0.206.macos10.xamarin.universal.pkg");
	
	AppleCodesignIdentity("Apple Development: Jonathan Dick (FJL7285DY2)", "https://dl.internalx.com/qa/code-signing-entitlements/components-mac-ios-certificate.p12");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-ios-provisioning.mobileprovision");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-mac-provisioning.mobileprovision");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-tvos-provisioning.mobileprovision");
}

string ANDROID_API_SDKS = Environment.GetEnvironmentVariable ("ANDROID_API_SDKS");
string SKIP_ANDROID_API_SDKS = Environment.GetEnvironmentVariable ("SKIP_ANDROID_API_SDKS");
string SKIP_ANDROID_API_IMAGES = Environment.GetEnvironmentVariable ("SKIP_ANDROID_API_IMAGES");

string INSTALL_DEFAULT_ANDROID_API = Environment.GetEnvironmentVariable ("INSTALL_DEFAULT_ANDROID_API");


Console.WriteLine($"LOGGING:");
Console.WriteLine($"ANDROID_API_SDKS: {ANDROID_API_SDKS}");
Console.WriteLine($"SKIP_ANDROID_API_SDKS: {SKIP_ANDROID_API_SDKS}");
Console.WriteLine($"SKIP_ANDROID_API_IMAGES: {SKIP_ANDROID_API_IMAGES}");
Console.WriteLine($"INSTALL_DEFAULT_ANDROID_API: {INSTALL_DEFAULT_ANDROID_API}");

if(String.IsNullOrWhiteSpace(ANDROID_API_SDKS) && String.IsNullOrWhiteSpace(SKIP_ANDROID_API_SDKS))
{
	AndroidSdk()
		.ApiLevel((AndroidApiLevel)23)
		.ApiLevel((AndroidApiLevel)24)
		.ApiLevel((AndroidApiLevel)25)
		.ApiLevel((AndroidApiLevel)26)
		.ApiLevel((AndroidApiLevel)27)
		.ApiLevel((AndroidApiLevel)28)
		.ApiLevel((AndroidApiLevel)29)
		.ApiLevel((AndroidApiLevel)30)
		.ApiLevel((AndroidApiLevel)31)
		.ApiLevel((AndroidApiLevel)32)
		.ApiLevel((AndroidApiLevel)33)
		.ApiLevel((AndroidApiLevel)34)
		.ApiLevel((AndroidApiLevel)35);

	if(string.IsNullOrWhiteSpace(SKIP_ANDROID_API_IMAGES))
	{
		AndroidSdk()
		.VirtualDevice("Android_x64_API23",   (AndroidApiLevel)23, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API24",   (AndroidApiLevel)24, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API25",   (AndroidApiLevel)25, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API26",   (AndroidApiLevel)26, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x86_API27",   (AndroidApiLevel)27, AndroidSystemImageApi.GooglePlayStore,  AndroidSystemImageAbi.x86,       AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API28",   (AndroidApiLevel)28, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API29",   (AndroidApiLevel)29, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API30",   (AndroidApiLevel)30, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API31",   (AndroidApiLevel)31, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API32",   (AndroidApiLevel)32, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API33",   (AndroidApiLevel)33, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X)
		.VirtualDevice("Android_x64_API34",   (AndroidApiLevel)34, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.x86_64,    AndroidVirtualDevice.NEXUS_5X);
	
		if (IsArm64)
		{
			AndroidSdk()
				.VirtualDevice("Android_arm64_API23", (AndroidApiLevel)23, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API24", (AndroidApiLevel)24, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API25", (AndroidApiLevel)25, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API26", (AndroidApiLevel)26, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API27", (AndroidApiLevel)27, AndroidSystemImageApi.Google,          AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API28", (AndroidApiLevel)28, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API29", (AndroidApiLevel)29, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API30", (AndroidApiLevel)30, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API31", (AndroidApiLevel)31, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API32", (AndroidApiLevel)32, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API33", (AndroidApiLevel)33, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X)
				.VirtualDevice("Android_arm64_API34", (AndroidApiLevel)34, AndroidSystemImageApi.GooglePlayStore, AndroidSystemImageAbi.ARM64_v8a, AndroidVirtualDevice.NEXUS_5X);
		}
	}

	AndroidSdk().SdkManagerPackage ("build-tools;35.0.0");
}

if(!string.IsNullOrEmpty(INSTALL_DEFAULT_ANDROID_API))
{
	AndroidSdk()
		.ApiLevel((AndroidApiLevel)35)
		.SdkManagerPackage ("build-tools;35.0.0");;
}

else if(!String.IsNullOrWhiteSpace(ANDROID_API_SDKS))
{

	var androidSDK = AndroidSdk();
	foreach(var sdk in ANDROID_API_SDKS.Split(','))
	{
		Console.WriteLine("Installing SDK: {0}", sdk);
		androidSDK = androidSDK.SdkManagerPackage (sdk);
	}
}
