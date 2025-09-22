using System.Reflection;
using System.Text.RegularExpressions;
using ImageMagick;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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
		string _defaultiOSVersion = "18.5";

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
			var frameworkVersion = "net10.0";
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
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? _defaultiOSVersion);
					config.SetProperty("Udid", Environment.GetEnvironmentVariable("DEVICE_UDID") ?? "");
					break;
				case TestDevice.Windows:
					var appProjectFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..\\..\\..\\Controls.TestCases.HostApp");
					var windowsExe = "Controls.TestCases.HostApp.exe";
					var windowsExePath = Path.Combine(appProjectFolder, $"{configuration}\\{frameworkVersion}-windows10.0.20348.0\\win-x64\\{windowsExe}");
					var windowsExePath19041 = Path.Combine(appProjectFolder, $"{configuration}\\{frameworkVersion}-windows10.0.19041.0\\win-x64\\{windowsExe}");

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

			// This currently doesn't work
			if (!String.IsNullOrEmpty(TestContext.CurrentContext.Test.ClassName))
			{
				config.SetTestConfigurationArg("StartingTestClass", TestContext.CurrentContext.Test.ClassName);
			}

			var commandLineArgs = Environment.GetEnvironmentVariable("TEST_CONFIGURATION_ARGS") ?? "";
			if (!String.IsNullOrEmpty(commandLineArgs))
			{
				config.SetTestConfigurationArg("TEST_CONFIGURATION_ARGS", commandLineArgs);
			}

			return config;
		}

		public override void Reset()
		{
			App.ResetApp();
		}

		/// <summary>
		/// Verifies the screenshots and returns an exception in case of failure.
		/// </summary>
		/// <remarks>
		/// This is especially useful when capturing multiple screenshots in a single UI test.
		/// </remarks>
		/// <example>
		/// <code>
		/// Exception? exception = null;
		/// VerifyScreenshotOrSetException(ref exception, "MyScreenshotName");
		/// VerifyScreenshotOrSetException(ref exception, "MyOtherScreenshotName");
		/// if (exception is not null) throw exception;
		/// </code>
		/// </example>
		public void VerifyScreenshotOrSetException(
			ref Exception? exception,
			string? name = null,
			TimeSpan? retryDelay = null,
			int cropTop = 0,
			int cropBottom = 0,
			double tolerance = 0.0
#if MACUITEST || WINTEST
			, bool includeTitleBar = false
#endif
			)
		{
			try
			{
				VerifyScreenshot(name, retryDelay, cropTop, cropBottom, tolerance
#if MACUITEST || WINTEST
				, includeTitleBar
#endif
				);
			}
			catch (Exception ex)
			{
				exception ??= ex;
			}
		}

		/// <summary>
		/// Verifies a screenshot by comparing it against a baseline image and throws an exception if verification fails.
		/// </summary>
		/// <param name="name">Optional name for the screenshot. If not provided, a default name will be used.</param>
		/// <param name="retryDelay">Optional delay between retry attempts when verification fails.</param>
		/// <param name="cropTop">Number of pixels to crop from the top of the screenshot.</param>
		/// <param name="cropBottom">Number of pixels to crop from the bottom of the screenshot.</param>
		/// <param name="tolerance">Tolerance level for image comparison as a percentage from 0 to 100.</param>
#if MACUITEST || WINTEST
/// <param name="includeTitleBar">Whether to include the title bar in the screenshot comparison.</param>
#endif
		/// <remarks>
		/// This method immediately throws an exception if the screenshot verification fails.
		/// For batch verification of multiple screenshots, consider using <see cref="VerifyScreenshotOrSetException"/> instead.
		/// </remarks>
		/// <example>
		/// <code>
		/// // Exact match (no tolerance)
		/// VerifyScreenshot("LoginScreen");
		/// 
		/// // Allow 2% difference for dynamic content
		/// VerifyScreenshot("DashboardWithTimestamp", tolerance: 2.0);
		/// 
		/// // Allow 5% difference for animations or slight rendering variations
		/// VerifyScreenshot("ButtonHoverState", tolerance: 5.0);
		/// 
		/// // Combined with cropping and tolerance
		/// VerifyScreenshot("HeaderSection", cropTop: 50, cropBottom: 100, tolerance: 3.0);
		/// </code>
		/// </example>
		public void VerifyScreenshot(
			string? name = null,
			TimeSpan? retryDelay = null,
			int cropTop = 0,
			int cropBottom = 0,
			double tolerance = 0.0 // Add tolerance parameter (0.05 = 5%)
#if MACUITEST || WINTEST
			, bool includeTitleBar = false
#endif
		)
		{
			retryDelay ??= TimeSpan.FromMilliseconds(500);
			// Retry the verification once in case the app is in a transient state
			try
			{
				Verify(name);
			}
			catch
			{
				Thread.Sleep(retryDelay.Value);
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
						var deviceApiLevel = (long?)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceApiLevel")
							?? throw new InvalidOperationException("deviceApiLevel capability is missing or null.");
						var deviceScreenSize = (string?)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenSize")
							?? throw new InvalidOperationException("deviceScreenSize capability is missing or null.");
						var deviceScreenDensity = (long?)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenDensity")
							?? throw new InvalidOperationException("deviceScreenDensity capability is missing or null.");

						if (deviceApiLevel == 36)
						{
							environmentName = "android-notch-36";
						}

						if (!((deviceApiLevel == 30 && deviceScreenSize == "1080x1920" && deviceScreenDensity == 420) || (deviceApiLevel == 36 && deviceScreenSize == "1080x2424" && deviceScreenDensity == 420)))
						{
							Assert.Fail($"Android visual tests should be run on an API30 emulator image with 1080x1920 420dpi screen or API36 emulator image with 1080x2424 420dpi screen, but the current device is API {deviceApiLevel} with a {deviceScreenSize} {deviceScreenDensity}dpi screen. Follow the steps on the MAUI UI testing wiki to launch the Android emulator with the right image.");
						}
						break;

					case TestDevice.iOS:
						var platformVersion = (string?)((AppiumApp)App).Driver.Capabilities.GetCapability("platformVersion")
							?? throw new InvalidOperationException("platformVersion capability is missing or null.");
						var device = (string?)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceName")
							?? throw new InvalidOperationException("deviceName capability is missing or null.");

						environmentName = "ios";

						if (device.Contains(" Xs", StringComparison.OrdinalIgnoreCase) && platformVersion == _defaultiOSVersion)
						{
							environmentName = "ios";
						}
						else if (deviceName == "iPhone Xs (iOS 17.2)" || (device.Contains(" Xs", StringComparison.OrdinalIgnoreCase) && platformVersion == "17.2"))
						{
							environmentName = "ios";
						}
						else if (deviceName == "iPhone X (iOS 16.4)" || (device.Contains(" X", StringComparison.OrdinalIgnoreCase) && platformVersion == "16.4"))
						{
							environmentName = "ios-iphonex";
						}
						else
						{
							//Assert.Fail($"iOS visual tests should be run on iPhone Xs (iOS {_defaultiOSVersion}) or iPhone X (iOS 16.4) simulator images, but the current device is '{deviceName}' '{platformVersion}'. Follow the steps on the MAUI UI testing wiki.");
						}
						break;

					case TestDevice.Windows:
						environmentName = "windows";
						break;

					case TestDevice.Mac:
						environmentName = "mac";
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

#if MACUITEST
				byte[] screenshotPngBytes = TakeScreenshot() ?? throw new InvalidOperationException("Failed to get screenshot");
#else
				byte[] screenshotPngBytes = App.Screenshot() ?? throw new InvalidOperationException("Failed to get screenshot");
#endif

				var actualImage = new ImageSnapshot(screenshotPngBytes, ImageSnapshotFormat.PNG);

				// For Android and iOS, crop off the OS status bar at the top since it's not part of the
				// app itself and contains the time, which always changes. For WinUI, crop off the title
				// bar at the top as it varies slightly based on OS theme and is also not part of the app.
				int cropFromTop = _testDevice switch
				{
					TestDevice.Android => environmentName == "android-notch-36" ? 95 : 60,
					TestDevice.iOS => environmentName == "ios-iphonex" ? 90 : 110,
					TestDevice.Windows => 32,
					TestDevice.Mac => 29,
					_ => 0,
				};

#if MACUITEST || WINTEST
				if (includeTitleBar)
				{
					cropFromTop = 0;
				}
#endif

				// For Android also crop the 3 button nav from the bottom, since it's not part of the
				// app itself and the button color can vary (the buttons change clear briefly when tapped).
				// For iOS, crop the home indicator at the bottom.
				int cropFromBottom = _testDevice switch
				{
					TestDevice.Android => environmentName == "android-notch-36" ? 40 : 125,
					TestDevice.iOS => 40,
					_ => 0,
				};

				cropFromTop = cropTop > 0 ? cropTop : cropFromTop;
				cropFromBottom = cropBottom > 0 ? cropBottom : cropFromBottom;

				if (cropFromTop > 0 || cropFromBottom > 0)
				{
					IImageEditor imageEditor = _imageEditorFactory.CreateImageEditor(actualImage);
					(int width, int height) = imageEditor.GetSize();

					imageEditor.Crop(0, cropFromTop, width, height - cropFromTop - cropFromBottom);

					actualImage = imageEditor.GetUpdatedImage();
				}

				// Apply tolerance if specified
				if (tolerance > 0)
				{
					VerifyWithTolerance(name!, actualImage, environmentName, tolerance);
				}
				else
				{
					_visualRegressionTester.VerifyMatchesSnapshot(name!, actualImage, environmentName: environmentName, testContext: _visualTestContext);
				}
			}
		}

		void VerifyWithTolerance(string name, ImageSnapshot actualImage, string environmentName, double tolerance)
		{
			if (tolerance > 15)
			{
				throw new ArgumentException($"Tolerance {tolerance}% exceeds the acceptable limit. Please review whether this requires a different test or if it is a bug.");
			}

			try
			{
				_visualRegressionTester.VerifyMatchesSnapshot(name, actualImage, environmentName: environmentName, testContext: _visualTestContext);
			}
			catch (Exception ex) when (IsVisualDifferenceException(ex))
			{
				var difference = ExtractDifferencePercentage(ex);
				if (difference <= tolerance)
				{
					// Log warning but pass test
					TestContext.WriteLine($"Visual difference {difference}% within tolerance {tolerance}% for '{name}' on {environmentName}");
					return;
				}
				throw; // Re-throw if exceeds tolerance
			}
		}

		bool IsVisualDifferenceException(Exception ex)
		{
			// Check if this is a visual regression failure
			return ex.GetType().Name.Contains("Assert", StringComparison.Ordinal) ||
				   ex.Message.Contains("Snapshot different", StringComparison.Ordinal) ||
				   ex.Message.Contains("baseline", StringComparison.Ordinal) ||
				   ex.Message.Contains("different", StringComparison.Ordinal);
		}

		double ExtractDifferencePercentage(Exception ex)
		{
			var message = ex.Message;

			// Extract percentage from pattern: "X,XX% difference"
			var match = Regex.Match(message, @"(\d+,\d+)%\s*difference", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				var percentageString = match.Groups[1].Value.Replace(',', '.');
				if (double.TryParse(percentageString, System.Globalization.NumberStyles.Float,
					System.Globalization.CultureInfo.InvariantCulture, out var percentage))
				{
					return percentage;
				}
			}

			// If can't extract specific percentage, throw an exception to indicate failure
			throw new InvalidOperationException("Unable to extract difference percentage from exception message.");
		}

		protected void VerifyInternetConnectivity()
		{
			try
			{
				App.WaitForElement("NoInternetAccessLabel", timeout: TimeSpan.FromSeconds(30));
				Assert.Inconclusive("This device doesn't have internet access");
			}
			catch (TimeoutException)
			{
				// Continue with the test
			}
		}

		public override void TestSetup()
		{
			base.TestSetup();
			var device = App.GetTestDevice();
			if (device == TestDevice.Android || device == TestDevice.iOS)
			{
				try
				{
					App.SetOrientationPortrait();
				}
				catch
				{
					// The app might not be ready
					// Probably reduce this value if this works
					Thread.Sleep(1000);
					App.SetOrientationPortrait();
				}
			}
		}

#if MACUITEST
		byte[] TakeScreenshot()
		{
			// Since the Appium screenshot on Mac (unlike Windows) is of the entire screen, not just the app,
			// we are going to crop the screenshot to the app window bounds, including rounded corners.
			var windowBounds = App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow")).GetRect();

			var x = windowBounds.X;
			var y = windowBounds.Y;
			var width = windowBounds.Width;
			var height = windowBounds.Height;
			const int cornerRadius = 12;

			// Take the screenshot
			var bytes = App.Screenshot();

			// Draw a rounded rectangle with the app window bounds as mask
			using var surface = new MagickImage(MagickColors.Transparent, width, height);
			new Drawables()
				.RoundRectangle(0, 0, width, height, cornerRadius, cornerRadius)
				.FillColor(MagickColors.Black)
				.Draw(surface);

			// Composite the screenshot with the mask
			using var image = new MagickImage(bytes);
			surface.Composite(image, -x, -y, CompositeOperator.SrcAtop);

			return surface.ToByteArray(MagickFormat.Png);
		}
#endif
	}
}