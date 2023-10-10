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
			var frameworkVersion = "net7.0";
#if DEBUG
			var configuration = "Debug";
#else
			var configuration = "Release";
#endif

			IConfig config = new Config();
			config.SetProperty("AppId", "com.microsoft.maui.uitests");
			
			switch (_testDevice)
			{
				case TestDevice.iOS:
					config.SetProperty("DeviceName", "iPhone X");
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("IOS_PLATFORM_VERSION") ?? "14.4");
					config.SetProperty("Udid", Environment.GetEnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
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
			if (_testDevice == TestDevice.Mac)
			{
				// For now, ignore visual tests on Mac Catalyst since the Appium screenshot on Mac (unlike Windows)
				// is of the entire screen, not just the app. Later when xharness relay support is in place to
				// send a message to the MAUI app to get the screenshot, we can use that to just screenshot
				// the app.
				Assert.Ignore("MacCatalyst isn't supported yet for visual tests");
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
				TestDevice.iOS => 90,
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

			string platform = _testDevice switch
			{
				TestDevice.Android => "android",
				TestDevice.iOS => "ios",
				TestDevice.Mac => "mac",
				TestDevice.Windows => "windows",
				_ => throw new NotImplementedException($"Unknown device type {_testDevice}"),
			};

			_visualRegressionTester.VerifyMatchesSnapshot(name!, actualImage, environmentName: platform, testContext: _visualTestContext);
		}
	}
}