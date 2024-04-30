using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UITest.Core;

namespace UITest.Appium.NUnit
{
	//#if ANDROID
	//	[TestFixture(TestDevice.Android)]
	//#elif IOSUITEST
	//	[TestFixture(TestDevice.iOS)]
	//#elif MACUITEST
	//	[TestFixture(TestDevice.Mac)]
	//#elif WINTEST
	//	[TestFixture(TestDevice.Windows)]
	//#else
	//    [TestFixture(TestDevice.iOS)]
	//    [TestFixture(TestDevice.Mac)]
	//    [TestFixture(TestDevice.Windows)]
	//    [TestFixture(TestDevice.Android)]
	//#endif
	public abstract class UITestBase : UITestContextBase
	{
		public UITestBase(TestDevice testDevice, bool useBrowserStack)
			: base(testDevice, useBrowserStack)
		{
		}

		[SetUp]
		public void TestSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");

			// When running tests in isolation, we need to run the fixture setup steps for each test, navigating to the right UI
			if (RunTestsInIsolation)
			{
				SetUpForFixtureTests();
			}
		}

		[TearDown]
		public void TestTeardown()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Stop");

			// With BrowserStack, checking AppState isn't supported currently, producing the error below, so skip this on BrowserStack
			// BrowserStack error: System.NotImplementedException : Unknown mobile command "queryAppState". Only shell,scrollBackTo,viewportScreenshot,deepLink,startLogsBroadcast,stopLogsBroadcast,acceptAlert,dismissAlert,batteryInfo,deviceInfo,changePermissions,getPermissions,performEditorAction,startScreenStreaming,stopScreenStreaming,getNotifications,listSms,type commands are supported
			if (!_useBrowserStack)
			{
				if (App.AppState == ApplicationState.NotRunning)
				{
					Reset();
					FixtureSetup();

					// Assert.Fail will immediately exit the test which is desirable as the app is not
					// running anymore so we can't capture any UI structures or any screenshots
					Assert.Fail("The app was expected to be running still, investigate as possible crash");
				}
			}

			var testOutcome = TestContext.CurrentContext.Result.Outcome;
			if (testOutcome == ResultState.Error ||
				testOutcome == ResultState.Failure)
			{
				SaveDeviceDiagnosticInfo();
				SaveUIDiagnosticInfo();
			}

			if (RunTestsInIsolation)
			{
				TearDownDriver();
			}
		}

		protected virtual void FixtureSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
		}

		protected virtual void FixtureTeardown()
		{
			try
			{
				Reset();
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			if (!RunTestsInIsolation)
			{
				SetUpForFixtureTests();
			}
		}

		void SetUpForFixtureTests()
		{

			try
			{
				if (App.AppState == ApplicationState.NotRunning)
				{
					SaveDeviceDiagnosticInfo();

					Reset();
					FixtureSetup();

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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			// When running tests in isolation, we can skip this. OneTimeSetup failures never happen and there's no need to
			// call FixtureTeardown to navigate back in the UI since the app will be restarted for the next test.
			if (!RunTestsInIsolation)
			{
				var outcome = TestContext.CurrentContext.Result.Outcome;

				// We only care about setup failures as regular test failures will already do logging
				if (outcome.Status == ResultState.SetUpFailure.Status &&
					outcome.Site == ResultState.SetUpFailure.Site)
				{
					SaveDeviceDiagnosticInfo();

					if (App.AppState != ApplicationState.NotRunning)
						SaveUIDiagnosticInfo();
				}

				FixtureTeardown();
			}
		}

		void SaveDeviceDiagnosticInfo([CallerMemberName] string? note = null)
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

		protected bool SaveUIDiagnosticInfo([CallerMemberName] string? note = null)
		{
			if (App.AppState == ApplicationState.NotRunning)
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
	}
}