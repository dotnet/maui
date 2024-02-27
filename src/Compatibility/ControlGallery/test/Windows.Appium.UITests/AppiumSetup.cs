using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

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

		var windowsOptions = new AppiumOptions
		{
			// Specify windows as the driver, typically don't need to change this
			AutomationName = "windows",
			// Always Windows for Windows
			PlatformName = "Windows",
			// The identifier of the deployed application to test
			App = "com.microsoft.maui.controls.devicetests_9zz4h110yvjzm!App",
		};

		// Note there are many more options that you can use to influence the app under test according to your needs

		Driver = new WindowsDriver(windowsOptions);
	}

	[OneTimeTearDown]
	public void RunAfterAnyTests()
	{
		Driver?.Quit();

		// If an Appium server was started locally above, make sure we clean it up here
		AppiumServerHelper.DisposeAppiumLocalServer();
	}
}