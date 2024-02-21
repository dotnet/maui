using System.Reflection;
using NUnit.Framework;
using UITest.Appium.NUnit;
using UITest.Core;
using VisualTestUtils;
using VisualTestUtils.MagickNet;

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
#else
	[TestFixture(TestDevice.iOS)]
	[TestFixture(TestDevice.Mac)]
	[TestFixture(TestDevice.Windows)]
	[TestFixture(TestDevice.Android)]
#endif
	public abstract class UITest : UITestBase
	{
		protected const int SetupMaxRetries = 1;
		readonly VisualRegressionTester _visualRegressionTester;
		readonly IImageEditorFactory _imageEditorFactory;
		readonly VisualTestContext _visualTestContext;

		protected UITest(TestDevice testDevice) : base(testDevice)
		{
			string? ciArtifactsDirectory = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
			if (ciArtifactsDirectory != null)
				ciArtifactsDirectory = Path.Combine(ciArtifactsDirectory, "Controls.AppiumTests");

			string assemblyDirectory = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)!;
			string projectRootDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", ".."));
			_visualRegressionTester = new VisualRegressionTester(testRootDirectory: projectRootDirectory,
				visualComparer: new MagickNetVisualComparer(),
				visualDiffGenerator: new MagickNetVisualDiffGenerator(),
				ciArtifactsDirectory: ciArtifactsDirectory);

			_imageEditorFactory = new MagickNetImageEditorFactory();
			_visualTestContext = new VisualTestContext();
		}

		public override IConfig GetTestConfig()
		{
			var frameworkVersion = "net8.0";
#if DEBUG
			var configuration = "Debug";
#else
			var configuration = "Release";
#endif

			IConfig config = new Config();
			config.SetProperty("AppId", "com.microsoft.maui.uitests");

			switch (_testDevice)
			{
				case TestDevice.Android:
					config.SetProperty("DeviceName", Environment.GetEnvironmentVariable("DEVICE_SKIN") ?? "");
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "");
					config.SetProperty("Udid", Environment.GetEnvironmentVariable("DEVICE_UDID") ?? "");
					break;
				case TestDevice.iOS:
					config.SetProperty("DeviceName", Environment.GetEnvironmentVariable("DEVICE_NAME") ?? "iPhone X");
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "17.0");
					config.SetProperty("Udid", Environment.GetEnvironmentVariable("DEVICE_UDID") ?? "");
					break;
				case TestDevice.Windows:
					var appProjectFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..\\..\\..\\..\\..\\samples\\Controls.Sample.UITests");
					var appProjectPath = Path.Combine(appProjectFolder, "Controls.Sample.UITests.csproj");
					var windowsExe = "Controls.Sample.UITests.exe";
					var windowsExePath = Path.Combine(appProjectFolder, $"bin\\{configuration}\\{frameworkVersion}-windows10.0.20348\\win10-x64\\{windowsExe}");

					var appPath = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINDOWS_APP_PATH"))
					   ? windowsExePath
					   : Environment.GetEnvironmentVariable("WINDOWS_APP_PATH");
					config.SetProperty("AppPath", appPath);
					break;
			}

			return config;
		}

		public void VerifyScreenshot(string? name = null)
		{
			string deviceName = GetTestConfig().GetProperty<string>("DeviceName") ?? string.Empty;
			// Remove the XHarness suffix if present
			deviceName = deviceName.Replace(" - created by XHarness", "", StringComparison.Ordinal);

			/*
			Determine the environmentName, used as the directory name for visual testing snaphots. Here are the rules/conventions:
			- Names are lower case, no spaces.
			- By default, the name matches the platform (android, ios, windows, or mac).
			- Each platform has a default device (or set of devices) - if the snapshot matches the default no suffix is needed (e.g. just ios).
			- If tests are run on secondary devices that produce different snapshots, the device name is used as suffix (e.g. ios-iphonex).
			- If tests are run on secondary devices with multiple OS versions that produce different snapshots, both device name and os version are
			  used as a suffix (e.g. ios-iphonex-16_4). We don't have any cases of this today but may eventually. The device name comes first here,
			  before os version, because most visual testing differences come from different sceen size (a device thing), not OS version differences,
			  but both can happen.
			*/
			string environmentName = "";
			switch (_testDevice)
			{
				case TestDevice.Android:
					if (deviceName == "Nexus 5X")
					{
						environmentName = "android";
					}
					else
					{
						Assert.Fail($"Android visual tests should be run on an Nexus 5X (API 30) emulator image, but the current device is '{deviceName}'. Follow the steps on the MAUI UI testing wiki.");
					}
					break;

				case TestDevice.iOS:
					if (deviceName == "iPhone Xs (iOS 17.2)")
					{
						environmentName = "ios";
					}
					else if (deviceName == "iPhone X (iOS 16.4)")
					{
						environmentName = "ios-iphonex";
					}
					else
					{
						Assert.Fail($"iOS visual tests should be run on iPhone Xs (iOS 17.2) or iPhone X (iOS 16.4) simulator images, but the current device is '{deviceName}'. Follow the steps on the MAUI UI testing wiki.");
					}
					break;

				case TestDevice.Windows:
					environmentName = "windows";
					break;

				case TestDevice.Mac:
					// For now, ignore visual tests on Mac Catalyst since the Appium screenshot on Mac (unlike Windows)
					// is of the entire screen, not just the app. Later when xharness relay support is in place to
					// send a message to the MAUI app to get the screenshot, we can use that to just screenshot
					// the app.
					Assert.Ignore("MacCatalyst isn't supported yet for visual tests");
					break;

				default:
					throw new NotImplementedException($"Unknown device type {_testDevice}");
			}

			name ??= TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;

			byte[] screenshotPngBytes = App.Screenshot() ?? throw new InvalidOperationException("Failed to get screenshot");

			var actualImage = new ImageSnapshot(screenshotPngBytes, ImageSnapshotFormat.PNG);

			// For Android and iOS, crop off the OS status bar at the top since it's not part of the
			// app itself and contains the time, which always changes. For WinUI, crop off the title
			// bar at the top as it varies slightly based on OS theme and is also not part of the app.
			int cropFromTop = _testDevice switch
			{
				TestDevice.Android => 60,
				TestDevice.iOS => environmentName == "ios-iphonex" ? 90 : 110,
				TestDevice.Windows => 32,
				_ => 0,
			};

			// For Android also crop the 3 button nav from the bottom, since it's not part of the
			// app itself and the button color can vary (the buttons change clear briefly when tapped)
			int cropFromBottom = _testDevice switch
			{
				TestDevice.Android => 125,
				_ => 0,
			};

			if (cropFromTop > 0 || cropFromBottom > 0)
			{
				IImageEditor imageEditor = _imageEditorFactory.CreateImageEditor(actualImage);
				(int width, int height) = imageEditor.GetSize();

				imageEditor.Crop(0, cropFromTop, width, height - cropFromTop - cropFromBottom);

				actualImage = imageEditor.GetUpdatedImage();
			}

			_visualRegressionTester.VerifyMatchesSnapshot(name!, actualImage, environmentName: environmentName, testContext: _visualTestContext);
		}
	}
}