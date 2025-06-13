using System.Reflection;
using System.Runtime.CompilerServices;
using UITest.Core;
using Xunit;
using Xunit.Abstractions;

namespace UITest.Appium.XUnit
{
	public abstract class UITestBase : UITestContextBase, IDisposable
	{
		protected virtual bool ResetAfterEachTest => false;
		protected readonly ITestOutputHelper? _output;

		public UITestBase(TestDevice testDevice, ITestOutputHelper? output = null)
			: base(testDevice)
		{
			_output = output;
			// Equivalent to NUnit's [SetUp]
			TestSetup();
		}

		public void RecordTestSetup()
		{
			var name = GetTestName();
			_output?.WriteLine($">>>>> {DateTime.Now} {name} Start");
		}

		public virtual void TestSetup()
		{
			RecordTestSetup();
			if (ResetAfterEachTest)
			{
				FixtureSetup();
			}
		}

		public virtual void TestTearDown()
		{
			RecordTestTeardown();
			UITestBaseTearDown();
			if (ResetAfterEachTest)
			{
				Reset();
			}
		}

		public void RecordTestTeardown()
		{
			var name = GetTestName();
			_output?.WriteLine($">>>>> {DateTime.Now} {name} Stop");
		}

		protected virtual void FixtureSetup()
		{
			var name = GetTestName();
			_output?.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
		}

		protected virtual void FixtureOneTimeTearDown()
		{
			try
			{
				if (!ResetAfterEachTest)
					Reset();
			}
			catch (Exception e)
			{
				var name = GetTestName();
				_output?.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		public void UITestBaseTearDown()
		{
			try
			{
				if (App.AppState != ApplicationState.Running)
				{
					SaveDeviceDiagnosticInfo();

					if (!ResetAfterEachTest)
					{
						Reset();
						FixtureSetup();
					}

					// xUnit equivalent of Assert.Fail
					throw new Xunit.Sdk.XunitException("The app was expected to be running still, investigate as possible crash");
				}
			}
			finally
			{
				// Note: xUnit doesn't have direct equivalent of TestContext.CurrentContext.Result.Outcome
				// We'll need to handle this differently in xUnit
				SaveDeviceDiagnosticInfo();
				SaveUIDiagnosticInfo();
			}
		}

		// xUnit uses constructor for setup and IDisposable for teardown
		public virtual void Dispose()
		{
			TestTearDown();
			FixtureOneTimeTearDown();
		}

		private string GetTestName()
		{
			// In xUnit, we don't have direct access to test context like NUnit
			// We'll use reflection to get the test method name
			var stackTrace = new System.Diagnostics.StackTrace();
			var frame = stackTrace.GetFrames()?.FirstOrDefault(f => 
				f.GetMethod()?.GetCustomAttribute<FactAttribute>() != null ||
				f.GetMethod()?.GetCustomAttribute<TheoryAttribute>() != null);
			
			return frame?.GetMethod()?.Name ?? "UnknownTest";
		}

		void SaveDeviceDiagnosticInfo([CallerMemberName] string? note = null)
		{
			try
			{
				var types = App.GetLogTypes().ToArray();
				_output?.WriteLine($">>>>> {DateTime.Now} Log types: {string.Join(", ", types)}");

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
				var name = GetTestName();
				_output?.WriteLine($">>>>> {DateTime.Now} The SaveDeviceDiagnosticInfo threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		protected bool SaveUIDiagnosticInfo([CallerMemberName] string? note = null)
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

			var name = GetTestName();

			return Path.Combine(logDir, $"{name}-{_testDevice}{note}{filename}");
		}

		void AddTestAttachment(string filePath, string? description = null)
		{
			try
			{
				// xUnit doesn't have built-in test attachment support like NUnit
				// We'll log the file path instead
				_output?.WriteLine($"Test attachment: {filePath} - {description}");
			}
			catch (FileNotFoundException e) when (e.Message == "Test attachment file path could not be found.")
			{
				// Add the file path to better troubleshoot when these errors occur
				throw new FileNotFoundException($"Test attachment file path could not be found: '{filePath}' {description}", e);
			}
		}
	}
}