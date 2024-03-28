/*
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace UITests;

[SetUpFixture]
public class AppiumSetup
{
	static AppiumDriver? Driver;

	public static AppiumDriver App => Driver ?? throw new NullReferenceException("AppiumDriver is null");

	[OneTimeSetUp]
	public void RunBeforeAnyTests()
	{
		// If you started an Appium server manually, make sure to comment out the next line
		// This line starts a local Appium server for you as part of the test run
		AppiumServerHelper.StartAppiumLocalServer();


		var androidOptions = new AppiumOptions
		{
			// Specify UIAutomator2 as the driver, typically don't need to change this
			AutomationName = "UIAutomator2",
			// Always Android for Android
			PlatformName = "Android",

			// RELEASE BUILD SETUP
			// The full path to the .apk file
			// This only works with release builds because debug builds have fast deployment enabled
			// and Appium isn't compatible with fast deployment
			// App = Path.Join(TestContext.CurrentContext.TestDirectory, "../../../../MauiApp/bin/Release/net8.0-android/com.companyname.basicappiumsample-Signed.apk"),
			// END RELEASE BUILD SETUP
		};

		// DEBUG BUILD SETUP
        // If you're running your tests against debug builds you'll need to set NoReset to true
        // otherwise appium will delete all the libraries used for Fast Deployment on Android
        // Release builds have Fast Deployment disabled
        // https://learn.microsoft.com/xamarin/android/deploy-test/building-apps/build-process#fast-deployment
        androidOptions.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");
        androidOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.mauicompatibilitygallery");

        //Make sure to set [Register("com.companyname.basicappiumsample.MainActivity")] on the MainActivity of your android application
		androidOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, $"com.microsoft.mauicompatibilitygallery.MainActivity");
        // END DEBUG BUILD SETUP


        // Specifying the avd option will boot the emulator for you
        // make sure there is an emulator with the name below
        // If not specified, make sure you have an emulator booted
        //androidOptions.AddAdditionalAppiumOption("avd", "pixel_5_-_api_33");

        // Note there are many more options that you can use to influence the app under test according to your needs

        Driver = new AndroidDriver(androidOptions);
	}

	[OneTimeTearDown]
	public void RunAfterAnyTests()
	{
		Driver?.Quit();

		// If an Appium server was started locally above, make sure we clean it up here
		AppiumServerHelper.DisposeAppiumLocalServer();
	}
}
*/