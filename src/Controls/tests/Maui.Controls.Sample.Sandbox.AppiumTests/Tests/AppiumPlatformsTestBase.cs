using NUnit.Framework;
using Microsoft.Maui.Appium;
using TestUtils.Appium.UITests;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium;

namespace Maui.Controls.Sample.Sandbox.AppiumTests
{
#if ANDROIDUITEST
	[TestFixture(TestDevice.Android)]
#endif
#if IOSUITEST
	[TestFixture(TestDevice.iOS)]
#endif
#if MACUITEST
	[TestFixture(TestDevice.Mac)]
#endif
	// [TestFixture(TestDevice.Windows)]
	public class AppiumPlatformsTestBase : AppiumUITestBase
	{
		TestDevice _testDevice;
		public AppiumPlatformsTestBase(TestDevice device)
		{
			_testDevice = device;
		}

		[TearDown]
		public void TearDown()
		{
			//this crashes on Android
			if(!IsAndroid)
				Driver?.ResetApp();
		}


		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitialSetup();
		}

		[OneTimeTearDown()]
		public void OneTimeTearDown()
		{
			Teardown();
		}

		public override TestConfig GetTestConfig()
		{
			var testConfig = new TestConfig(_testDevice, "com.microsoft.maui.sandbox")
			{
				BundleId = "com.microsoft.maui.sandbox",
			};
			switch (_testDevice)
			{
				case TestDevice.Android:
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.sandbox");
					// activity { com.microsoft.maui.sandbox / crc64fa090d87c1ce7f0b.MainActivity}
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, "MainActivity");
					break;
				case TestDevice.iOS:
					testConfig.DeviceName = "iPhone X";
					testConfig.PlatformVersion = Environment.GetEnvironmentVariable("IOS_PLATFORM_VERSION") ?? "14.4";
					testConfig.Udid = Environment.GetEnvironmentVariable("IOS_SIMULATOR_UDID") ?? "";
					break;
				case TestDevice.Mac:

					break;
				case TestDevice.Windows:
					testConfig.DeviceName = "WindowsPC";
					testConfig.AppPath = "D:\\repos\\dotnet\\maui\\src\\Controls\\samples\\Controls.Sample.Sandbox\\bin\\Debug\\net7.0-windows10.0.20348\\win10-x64\\Maui.Controls.Sample.Sandbox.exe";
					break;
			}

			return testConfig;
		}
	}
}
