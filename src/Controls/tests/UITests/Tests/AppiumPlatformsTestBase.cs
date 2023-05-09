using System.Reflection;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests
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

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitializeEmulators();
			StartEmulators();

			if (_testDevice == TestDevice.Windows || _testDevice == TestDevice.iOS || _testDevice == TestDevice.Mac)
			{
				// Let's build and deploy the App Project
				BuildProject();
			}

			InitialSetup();
		}

		[SetUp]
		public void Setup()
		{
			StartEmulators();
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

			Teardown();
		}


		[OneTimeTearDown()]
		public void OneTimeTearDown()
		{
			TeardownOneTime();
		}

		public override TestConfig GetTestConfig()
		{
			var appProjectFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..\\..\\..\\..\\..\\samples\\Controls.Sample.UITests");
			var appProjectPath = Path.Combine(appProjectFolder, "Controls.Sample.UITests.csproj");
			var testConfig = new TestConfig(_testDevice, "com.microsoft.maui.uitests")
			{
				BundleId = "com.microsoft.maui.uitests",
				AppProjectPath = appProjectPath
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
					testConfig.PlatformVersion = Environment.GetEnvironmentVariable("IOS_PLATFORM_VERSION") ?? "16.2";
					testConfig.Udid = Environment.GetEnvironmentVariable("IOS_SIMULATOR_UDID") ?? "";
					break;
				case TestDevice.Mac:

					break;
				case TestDevice.Windows:
					testConfig.DeviceName = "WindowsPC";
					testConfig.AppPath = Environment.GetEnvironmentVariable("WINDOWS_APP_PATH") ??
						Path.Combine(appProjectFolder, $"bin\\{testConfig.Configuration}\\net7.0-windows10.0.20348\\win10-x64\\Controls.Sample.UITests.exe");
					break;
			}

			return testConfig;
		}
	}
}
