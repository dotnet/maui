if (IsMac)
{
	ForceJavaCleanup();
	MicrosoftOpenJdk ("11.0.13.8.1");
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
		.ApiLevel((AndroidApiLevel)32)
		.ApiLevel((AndroidApiLevel)33)
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
		.SdkManagerPackage ("build-tools;33.0.0");
}
else
{

	var androidSDK = AndroidSdk();
	foreach(var sdk in ANDROID_API_SDKS.Split(','))
	{
		Console.WriteLine("Installing SDK: {0}", sdk);
		androidSDK = androidSDK.SdkManagerPackage (sdk);
	}
}
