using System.Reflection;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TestUtils.Appium.UITests;
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
	public class UITestBase : UITestContextTestBase
	{
		protected const int SetupMaxRetries = 1;
		readonly TestDevice _testDevice;
		readonly VisualRegressionTester _visualRegressionTester;
		readonly IImageEditorFactory _imageEditorFactory;
		readonly VisualTestContext _visualTestContext;

		public UITestBase(TestDevice device)
		{
			_testDevice = device;

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

		[SetUp]
		public void RecordTestSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");
		}

		[TearDown]
		public void RecordTestTeardown()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Stop");
		}

		protected virtual void FixtureSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
		}

		protected virtual void FixtureTeardown()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureTeardown)} for {name}");
		}

		[TearDown]
		public void UITestBaseTearDown()
		{
			if (App is IApp2 app2)
			{
				if (app2.AppState == ApplicationState.Not_Running)
				{
					// Assert.Fail will immediately exit the test which is desirable as the app is not
					// running anymore so we don't want to log diagnostic data as there is nothing to collect from
					Reset();
					FixtureSetup();
					Assert.Fail("The app was expected to be running still, investigate as possible crash");
				}
			}

			var testOutcome = TestContext.CurrentContext.Result.Outcome;
			if (testOutcome == ResultState.Error ||
				testOutcome == ResultState.Failure)
			{
				SaveDiagnosticLogs("UITestBaseTearDown");
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitialSetup(TestContextSetupFixture.TestContext);
			try
			{
				SaveDiagnosticLogs("BeforeFixtureSetup");
				FixtureSetup();
			}
			catch
			{
				SaveDiagnosticLogs("FixtureSetup");
				throw;
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			var outcome = TestContext.CurrentContext.Result.Outcome;

			// We only care about setup failures as regular test failures will already do logging
			if (outcome.Status == ResultState.SetUpFailure.Status &&
				outcome.Site == ResultState.SetUpFailure.Site)
			{
				SaveDiagnosticLogs("OneTimeTearDown");
			}

			FixtureTeardown();
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
			var windowsExe = "Controls.Sample.UITests.exe";
			var windowsExePath = Path.Combine(appProjectFolder, $"bin\\{testConfig.Configuration}\\{testConfig.FrameworkVersion}-windows10.0.20348\\win10-x64\\{windowsExe}");

			switch (_testDevice)
			{
				case TestDevice.iOS:
					testConfig.DeviceName = "iPhone X";
					testConfig.PlatformVersion = Environment.GetEnvironmentVariable("IOS_PLATFORM_VERSION") ?? "14.4";
					testConfig.Udid = Environment.GetEnvironmentVariable("IOS_SIMULATOR_UDID") ?? "";
					break;
				case TestDevice.Windows:
					testConfig.DeviceName = "WindowsPC";
					testConfig.AppPath = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINDOWS_APP_PATH"))
						? windowsExePath
						: Environment.GetEnvironmentVariable("WINDOWS_APP_PATH");
					break;
			}

			return testConfig;
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

			if (name == null)
				name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;

			if (App is not IApp2 app)
				throw new InvalidOperationException("App is not an IApp2");

			byte[] screenshotPngBytes = app.Screenshot() ?? throw new InvalidOperationException("Failed to get screenshot");

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

		void SaveDiagnosticLogs(string? note = null)
		{
			if (string.IsNullOrEmpty(note))
				note = "-";
			else
				note = $"-{note}-";

			var logDir = (Path.GetDirectoryName(Environment.GetEnvironmentVariable("APPIUM_LOG_FILE")) ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;

			// App could be null if UITestContext was not able to connect to the test process (e.g. port already in use etc...)
			if (UITestContext is not null)
			{
				string name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;

				_ = App.Screenshot(Path.Combine(logDir, $"{name}-{_testDevice}{note}ScreenShot"));

				if (App is IApp2 app2)
				{
					var pageSource = app2.ElementTree;
					File.WriteAllText(Path.Combine(logDir, $"{name}-{_testDevice}{note}PageSource.txt"), pageSource);
				}
			}

			foreach (var log in Directory.GetFiles(logDir))
			{
				TestContext.AddTestAttachment(log, Path.GetFileName(log));
			}
		}
	}
}
