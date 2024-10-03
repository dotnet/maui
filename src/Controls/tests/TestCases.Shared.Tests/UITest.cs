using System.Reflection;
using NUnit.Framework;
using OpenQA.Selenium.Appium.iOS;
using UITest.Appium;
using UITest.Appium.NUnit;
using UITest.Core;
using VisualTestUtils;
using VisualTestUtils.MagickNet;

namespace Microsoft.Maui.TestCases.Tests
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
				ciArtifactsDirectory = Path.Combine(ciArtifactsDirectory, "Controls.TestCases.Shared.Tests");

			string projectRootDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!;

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
					config.SetProperty("DeviceName", Environment.GetEnvironmentVariable("DEVICE_NAME") ?? "iPhone Xs");
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "17.2");
					config.SetProperty("Udid", Environment.GetEnvironmentVariable("DEVICE_UDID") ?? "");
					break;
				case TestDevice.Windows:
					var appProjectFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..\\..\\..\\Controls.TestCases.HostApp");
					var windowsExe = "Controls.TestCases.HostApp.exe";
					var windowsExePath = Path.Combine(appProjectFolder, $"{configuration}\\{frameworkVersion}-windows10.0.20348.0\\win10-x64\\{windowsExe}");
					var windowsExePath19041 = Path.Combine(appProjectFolder, $"{configuration}\\{frameworkVersion}-windows10.0.19041.0\\win10-x64\\{windowsExe}");

					if (!File.Exists(windowsExePath) && File.Exists(windowsExePath19041))
					{
						windowsExePath = windowsExePath19041;
					}

					var appPath = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINDOWS_APP_PATH"))
					   ? windowsExePath
					   : Environment.GetEnvironmentVariable("WINDOWS_APP_PATH");
					config.SetProperty("AppPath", appPath);
					break;
			}

			return config;
		}

		public override void Reset()
		{
			App.ResetApp();
		}

		public void VerifyScreenshot(string? name = null)
		{
			// Retry the verification once in case the app is in a transient state
			try
			{
				Verify(name);
			}
			catch
			{
				Thread.Sleep(500);
				Verify(name);
			}

			void Verify(string? name)
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
						environmentName = "android";
						var deviceApiLevel = (long)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceApiLevel");
						var deviceScreenSize = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenSize");
						var deviceScreenDensity = (long)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenDensity");

						if (! (deviceApiLevel == 30 && deviceScreenSize == "1080x1920" && deviceScreenDensity == 420))
						{
							Assert.Fail($"Android visual tests should be run on an API30 emulator image with 1080x1920 420dpi screen, but the current device is API {deviceApiLevel} with a {deviceScreenSize} {deviceScreenDensity}dpi screen. Follow the steps on the MAUI UI testing wiki to launch the Android emulator with the right image.");
						}
						break;

					case TestDevice.iOS:
						var platformVersion = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("platformVersion");
						var device = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceName");

						if (deviceName == "iPhone Xs (iOS 17.2)" || (device.Contains(" Xs", StringComparison.OrdinalIgnoreCase) && platformVersion == "17.2"))
						{
							environmentName = "ios";
						}
						else if (deviceName == "iPhone X (iOS 16.4)" || (device.Contains(" X", StringComparison.OrdinalIgnoreCase) && platformVersion == "16.4"))
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

				// Currently Android is the OS with the ripple animations, but Windows may also have some animations
				// that need to finish before taking a screenshot.
				if (_testDevice == TestDevice.Android)
				{
					Thread.Sleep(350);
				}

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
					TestDevice.Windows => 2,
					_ => 0,
				};

				var cropFromLeft = _testDevice switch
				{
					TestDevice.Windows => 2,
					_ => 0,
				};

				var cropFromRight = _testDevice switch
				{
					TestDevice.Windows => 2,
					_ => 0,
				};

				if (cropFromTop > 0 || cropFromBottom > 0)
				{
					IImageEditor imageEditor = _imageEditorFactory.CreateImageEditor(actualImage);
					(int width, int height) = imageEditor.GetSize();

					imageEditor.Crop(cropFromLeft, cropFromTop, width - cropFromLeft - cropFromRight, height - cropFromTop - cropFromBottom);

					actualImage = imageEditor.GetUpdatedImage();
				}

				_visualRegressionTester.VerifyMatchesSnapshot(name!, actualImage, environmentName: environmentName, testContext: _visualTestContext);
			}
		}

		public override void TestSetup()
		{
			base.TestSetup();
			var device = App.GetTestDevice();
			if(device == TestDevice.Android || device == TestDevice.iOS)
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
