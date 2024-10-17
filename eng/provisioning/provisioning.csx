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
