using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.DevTools.V104.Page;

namespace Microsoft.Maui.Appium
{
	public abstract class AppiumTestBase
	{
		protected AppiumDriver? Driver;
		protected AppiumOptions AppiumOptions;
		protected TestConfig? TestConfig;
	
		public bool IsAndroid => Driver != null && Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
		public bool IsWindows => Driver != null && Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Windows");

		public AppiumTestBase()
		{
			AppiumOptions = new AppiumOptions();
		}

		public abstract TestConfig GetTestConfig();

		protected virtual AppiumDriver? GetDriver(Uri? driverUri = null)
		{
			if (TestConfig == null)
				throw new InvalidOperationException("Make sure to provide a TestConfig");

			SetGeneralAppiumOptions(AppiumOptions, TestConfig);

			if (driverUri == null)
				driverUri = new Uri("http://localhost:4723/wd/hub");
			
			return TestConfig.TestDevice switch
			{
				TestDevice.Android => new AndroidDriver(driverUri, AppiumOptions),
				TestDevice.iOS => new IOSDriver(driverUri, AppiumOptions),
				TestDevice.Mac => new MacDriver(driverUri, AppiumOptions),
				TestDevice.Windows => new WindowsDriver(driverUri, AppiumOptions),
				_ => null,
			};
		}

		protected virtual void SetPlatformAppiumOptions(AppiumOptions appiumOptions)
		{
			if (TestConfig == null)
				return;
			var appId = TestConfig.BundleId ?? TestConfig.AppId;
			appiumOptions.PlatformName = TestConfig.PlatformName;
			appiumOptions.AutomationName = TestConfig.AutomationName;

			if (!string.IsNullOrEmpty(TestConfig.DeviceName))
				appiumOptions.DeviceName = TestConfig.DeviceName;
			
			if (!string.IsNullOrEmpty(TestConfig.PlatformVersion))
				appiumOptions.PlatformVersion = TestConfig.PlatformVersion;

			if (!string.IsNullOrEmpty(TestConfig.AppPath))
				appiumOptions.App = TestConfig.AppPath;
			switch (TestConfig.TestDevice)
			{
				case TestDevice.Android:
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.sandbox");
					// activity { com.microsoft.maui.sandbox / crc64fa090d87c1ce7f0b.MainActivity}
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, "MainActivity");
					break;
				case TestDevice.iOS:
					appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, TestConfig.Udid);
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					break;
				case TestDevice.Mac:
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					break;
				case TestDevice.Windows:
					break;
			}
		}

		public string GetElementId(string elementId)
		{
			if (IsAndroid)
				return $"{TestConfig?.AppId}:id/{elementId}";

			return elementId;
		}

		public By ByAutomationId(string elementId)
		{
			var id = GetElementId(elementId);
			if (IsWindows)
				return MobileBy.AccessibilityId(id);

			return MobileBy.Id(id);
		}

		static void SetGeneralAppiumOptions(AppiumOptions appiumOptions, TestConfig testConfig)
		{
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", testConfig.ReportDirectory);
			appiumOptions.AddAdditionalAppiumOption("reportFormat", testConfig.ReportFormat);
			if (string.IsNullOrEmpty(testConfig.TestName))
				appiumOptions.AddAdditionalAppiumOption("testName", testConfig.TestName);
			if (testConfig.FullReset)
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "true");

			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.NewCommandTimeout , 3000);
		}

	}
}