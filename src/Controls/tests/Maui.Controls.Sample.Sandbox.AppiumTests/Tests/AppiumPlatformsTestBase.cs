using System.Reflection;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TestUtils.Appium.UITests;

namespace Maui.Controls.Sample.Sandbox.AppiumTests
{
#if ANDROID
	[TestFixture(TestDevice.Android)]
#elif IOSUITEST
	[TestFixture(TestDevice.iOS)]
#elif MACUITEST
	[TestFixture(TestDevice.Mac)]
#elif WINTEST
	[TestFixture(TestDevice.Windows)]
#endif
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
			var testOutcome = TestContext.CurrentContext.Result.Outcome;
			if (testOutcome == ResultState.Error ||
				testOutcome == ResultState.Failure)
			{
				var logDir = (Path.GetDirectoryName(Environment.GetEnvironmentVariable("APPIUM_LOG_FILE")) ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;

				var pageSource = Driver?.PageSource!;
				File.WriteAllText(Path.Combine(logDir, $"{TestContext.CurrentContext.Test.MethodName}-PageSource.txt"), pageSource);

				var screenshot = Driver?.GetScreenshot();
				screenshot?.SaveAsFile(Path.Combine(logDir, $"{TestContext.CurrentContext.Test.MethodName}-ScreenShot.png"));
			}

			//this crashes on Android
			if (!IsAndroid && !IsWindows)
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
			var testConfig = new TestConfig(_testDevice, "com.microsoft.maui.uitests")
			{
				BundleId = "com.microsoft.maui.uitests",
			};
			switch (_testDevice)
			{
				case TestDevice.Android:
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.uitests");
					// activity { com.microsoft.maui.uitests / crc64fa090d87c1ce7f0b.MainActivity}
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
					testConfig.AppPath = Environment.GetEnvironmentVariable("WINDOWS_APP_PATH") ?? "";
					break;
			}

			return testConfig;
		}
	}
}
