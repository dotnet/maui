if (IsMac)
{	
	AppleCodesignIdentity("Apple Development: Jonathan Dick (FJL7285DY2)", "https://dl.internalx.com/qa/code-signing-entitlements/components-mac-ios-certificate.p12");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-ios-provisioning.mobileprovision");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-mac-provisioning.mobileprovision");
	AppleCodesignProfile("https://dl.internalx.com/qa/code-signing-entitlements/components-tvos-provisioning.mobileprovision");
}
