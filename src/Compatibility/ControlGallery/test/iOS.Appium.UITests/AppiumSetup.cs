/*
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;

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

		var iOSOptions = new AppiumOptions
		{
			// Specify XCUITest as the driver, typically don't need to change this
			AutomationName = "XCUITest",
			// Always iOS for iOS
			PlatformName = "iOS",
			// iOS Version
			PlatformVersion = "17.0",
			// Don't specify if you don't want a specific device
			DeviceName = "iPhone 15 Pro",
			// The full path to the .app file to test or the bundle id if the app is already installed on the device
			App = "com.microsoft.mauicompatibilitygallery",
		};

		// Note there are many more options that you can use to influence the app under test according to your needs

		Driver = new IOSDriver(iOSOptions);
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