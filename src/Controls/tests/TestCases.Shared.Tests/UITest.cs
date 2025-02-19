using System.Reflection;
using System.Runtime.CompilerServices;
using ImageMagick;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.Browser;
using UITest.Appium;
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
	public abstract class UITest 
	{
		readonly VisualRegressionTester _visualRegressionTester;
		readonly IImageEditorFactory _imageEditorFactory;
		readonly VisualTestContext _visualTestContext;
		static IUIClientContext? _uiTestContext;
		IServerContext? _context;
		protected TestDevice _testDevice;
		string DriverShutdownMessage =>  $">>>>> {DateTime.Now} Driver has been shutdown. Most likely because the app or appium stopped responding.";
		

		protected UITest(TestDevice testDevice)
		{
			_testDevice = testDevice;
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

		public static IUIClientContext? UITestContext { get { return _uiTestContext; } }

		public TestDevice Device
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(Device)} property.")
					: UITestContext.Config.GetProperty<TestDevice>("TestDevice");
			}
		}

		public IApp App
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(App)} property.")
					: UITestContext.App;
			}
		}

		public bool IsSessionStillConnected => !(_uiTestContext is null && !AssemblySetupFixture.ServerContext.IsServerRunning);

		public void InitialSetup(IServerContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			var testConfig = GetTestConfig();
			testConfig.SetProperty("TestDevice", _testDevice);

			// Check to see if we have a context already from a previous test and re-use it as creating the driver is expensive

			try
			{
				_uiTestContext ??= context.CreateUIClientContext(testConfig);
			}
			finally
			{
				if (_uiTestContext is null)
				{
					TestContext.Error.WriteLine("Failed to get the driver.");
				}
			}
		}

		protected virtual bool ResetAfterEachTest => false;

		public void RecordTestSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");
		}
		

		[TearDown]
		public virtual void TestTearDown()
		{
			if (!this.IsSessionStillConnected) return;

			// At this point if the UITestContext hasn't been created just shut it down
			if (UITestContext is null)
			{
				this.ShutDownTestSessionAndFailTestRun();
				return;
			}

			RecordTestTeardown();
			UITestBaseTearDown();
			if (ResetAfterEachTest)
			{
				Reset();
			}
		}

		public void RecordTestTeardown()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Stop");
		}

		void FixtureSetupCore()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetupCore)} for {name}");

			try
			{
				try
				{

#if ANDROID || MACCATALYST
					App.ToggleSystemAnimations(false);
#endif
					FixtureSetup();
				}
				catch (Exception e)
				{
					TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetupCore)} for {name} Failed on First Try.{Environment.NewLine}Exception details: {e}");
					// Let's just try retrying once
					Reset();
					TestContext.Progress.WriteLine($">>>>> {DateTime.Now} Finished Resetting");
					FixtureSetup();
				}
			}
			catch (Exception e2) when (e2.InnerException is TaskCanceledException || e2 is TimeoutException)
			{
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The {nameof(FixtureSetupCore)} threw an exception during {name}.{Environment.NewLine}Exception details: {e2}");
				ShutDownTestSessionAndFailTestRun();
				throw;
			}
			finally
			{
				if (IsSessionStillConnected && UITestContext is not null)
				{
#if ANDROID || MACCATALYST
					App.ToggleSystemAnimations(true);
#endif
					SaveDeviceDiagnosticInfo();
					SaveUIDiagnosticInfo();
				}
			}
		}

		protected virtual void FixtureSetup()
		{
		}

		public void UITestBaseTearDown()
		{
			if (!this.IsSessionStillConnected) return;

			try
			{
				if (App.AppState != ApplicationState.Running)
				{
					SaveDeviceDiagnosticInfo();

					if (!ResetAfterEachTest)
					{
						Reset();
						FixtureSetupCore();
					}

					// Assert.Fail will immediately exit the test which is desirable as the app is not
					// running anymore so we can't capture any UI structures or any screenshots
					Assert.Fail("The app was expected to be running still, investigate as possible crash");
				}
			}
			finally
			{
				var testOutcome = TestContext.CurrentContext.Result.Outcome;
				if (testOutcome == ResultState.Error ||
					testOutcome == ResultState.Failure)
				{
					SaveDeviceDiagnosticInfo();
					SaveUIDiagnosticInfo();
				}
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			if (!this.IsSessionStillConnected) return;

			try
			{
				InitialSetup(AssemblySetupFixture.ServerContext);
				if (!ResetAfterEachTest)
				{
					FixtureSetupCore();
				}
			}
			catch (WebDriverException e)
			{
				if (e.InnerException is TaskCanceledException)
				{
					ShutDownTestSessionAndFailTestRun();
				}

				throw;
			}
			catch
			{
				if (IsSessionStillConnected && UITestContext is not null)
				{
					SaveDeviceDiagnosticInfo();
					SaveUIDiagnosticInfo();
				}

				throw;
			}
			finally
			{
				// If the UITestContext is null that means the driver failed to initialize
				// So let's just cut our losses for this entire run and fail the test run
				if (UITestContext is null && IsSessionStillConnected) ShutDownTestSessionAndFailTestRun();
			}
		}

		void ShutDownTestSessionAndFailTestRun([CallerMemberName] string? note = null)
		{
			if (IsSessionStillConnected && UITestContext is not null)
			{
				SaveDeviceDiagnosticInfo();
				SaveUIDiagnosticInfo();
			}

			TestContext.Error.WriteLine($">>>>> {DateTime.Now}-{note} Failed to communicate with the test process. Most likely the app or appium stopped responding. Shutting down the test session.");
			
			_uiTestContext?.Dispose();
			_uiTestContext = null;
			AssemblySetupFixture.ServerContext?.Dispose();
			TestContext.Error.WriteLine(DriverShutdownMessage);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (!this.IsSessionStillConnected) return;

			var outcome = TestContext.CurrentContext.Result.Outcome;

			// We only care about setup failures as regular test failures will already do logging
			if (outcome.Status == ResultState.SetUpFailure.Status &&
				outcome.Site == ResultState.SetUpFailure.Site)
			{
				SaveDeviceDiagnosticInfo();

				if (App.AppState == ApplicationState.Running)
					SaveUIDiagnosticInfo();
			}
			
			if (!ResetAfterEachTest)
			{
				Reset();
			}
		}

		void SaveDeviceDiagnosticInfo([CallerMemberName] string? note = null)
		{
			try
			{
				var types = App.GetLogTypes().ToArray();
				TestContext.Progress.WriteLine($">>>>> {DateTime.Now} Log types: {string.Join(", ", types)}");

				foreach (var logType in new[] { "logcat" })
				{
					if (!types.Contains(logType, StringComparer.InvariantCultureIgnoreCase))
						continue;

					var logsPath = GetGeneratedFilePath($"AppLogs-{logType}.log", note);
					if (logsPath is not null)
					{
						var entries = App.GetLogEntries(logType);
						File.WriteAllLines(logsPath, entries);

						AddTestAttachment(logsPath, Path.GetFileName(logsPath));
					}
				}
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now}-{note} The {nameof(SaveDeviceDiagnosticInfo)} threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		protected bool SaveUIDiagnosticInfo([CallerMemberName] string? note = null)
		{
			try
			{
				if (App.AppState != ApplicationState.Running)
					return false;

				var screenshotPath = GetGeneratedFilePath("ScreenShot.png", note);
				if (screenshotPath is not null)
				{
					_ = App.Screenshot(screenshotPath);

					AddTestAttachment(screenshotPath, Path.GetFileName(screenshotPath));
				}

				var pageSourcePath = GetGeneratedFilePath("PageSource.txt", note);
				if (pageSourcePath is not null)
				{
					File.WriteAllText(pageSourcePath, App.ElementTree);

					AddTestAttachment(pageSourcePath, Path.GetFileName(pageSourcePath));
				}

				return true;
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now}-{note} The {nameof(SaveUIDiagnosticInfo)} threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}

			return false;
		}

		string? GetGeneratedFilePath(string filename, string? note = null)
		{
			// App could be null if UITestContext was not able to connect to the test process (e.g. port already in use etc...)
			if (UITestContext is null)
				return null;

			if (string.IsNullOrEmpty(note))
				note = "-";
			else
				note = $"-{note}-";

			filename = $"{Path.GetFileNameWithoutExtension(filename)}-{Guid.NewGuid().ToString("N")}{Path.GetExtension(filename)}";

			var logDir =
				Path.GetDirectoryName(Environment.GetEnvironmentVariable("APPIUM_LOG_FILE") ??
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;

			var name =
				TestContext.CurrentContext.Test.MethodName ??
				TestContext.CurrentContext.Test.Name;

			return Path.Combine(logDir, $"{name}-{_testDevice}{note}{filename}");
		}

		void AddTestAttachment(string filePath, string? description = null)
		{
			try
			{
				TestContext.AddTestAttachment(filePath, description);
			}
			catch (FileNotFoundException e) when (e.Message == "Test attachment file path could not be found.")
			{
				// Add the file path to better troubleshoot when these errors occur
				throw new FileNotFoundException($"Test attachment file path could not be found: '{filePath}' {description}", e);
			}
		}

		public IConfig GetTestConfig()
		{
			var frameworkVersion = "net9.0";
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
					config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "18.0");
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

		public virtual void Reset()
		{
			if (!this.IsSessionStillConnected)
				return;

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
			TimeSpan? retryDelay = null
#if MACUITEST || WINTEST
			, bool includeTitleBar = false
#endif
			)
		{
			try
			{
				VerifyScreenshot(name, retryDelay
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

		public void VerifyScreenshot(
			string? name = null,
			TimeSpan? retryDelay = null
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
						var deviceApiLevel = (long)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceApiLevel");
						var deviceScreenSize = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenSize");
						var deviceScreenDensity = (long)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceScreenDensity");

						if (!(deviceApiLevel == 30 && deviceScreenSize == "1080x1920" && deviceScreenDensity == 420))
						{
							Assert.Fail($"Android visual tests should be run on an API30 emulator image with 1080x1920 420dpi screen, but the current device is API {deviceApiLevel} with a {deviceScreenSize} {deviceScreenDensity}dpi screen. Follow the steps on the MAUI UI testing wiki to launch the Android emulator with the right image.");
						}
						break;

					case TestDevice.iOS:
						var platformVersion = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("platformVersion");
						var device = (string)((AppiumApp)App).Driver.Capabilities.GetCapability("deviceName");

						if (device.Contains(" Xs", StringComparison.OrdinalIgnoreCase) && platformVersion == "18.0")
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
							Assert.Fail($"iOS visual tests should be run on iPhone Xs (iOS 17.2) or iPhone X (iOS 16.4) simulator images, but the current device is '{deviceName}'. Follow the steps on the MAUI UI testing wiki.");
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
					TestDevice.Android => 60,
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
					TestDevice.Android => 125,
					TestDevice.iOS => 40,
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

		[SetUp]
		public void TestSetupCore() 
		{
			// Throw an exception because we want the test run to fail if the driver has been shutdown
			if (!IsSessionStillConnected) 
				throw new Exception(this.DriverShutdownMessage);

			TestSetup();
		}

		public virtual void TestSetup()
		{
			
			RecordTestSetup();
			if (ResetAfterEachTest)
			{
				FixtureSetupCore();
			}

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
