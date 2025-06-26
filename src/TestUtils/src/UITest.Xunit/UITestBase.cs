using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using UITest.Core;

namespace UITest.Appium.Xunit
{
	public abstract class UITestBase : UITestContextBase, IDisposable
	{
		protected virtual bool ResetAfterEachTest => false;
		private readonly ITestOutputHelper? _testOutput;
		private bool _disposed = false;

		public UITestBase(TestDevice testDevice, ITestOutputHelper? testOutput = null)
			: base(testDevice)
		{
			_testOutput = testOutput;
			
			// Equivalent to OneTimeSetUp - run once when the test class is instantiated
			OneTimeSetup();
			
			// Equivalent to SetUp - run before each test (if ResetAfterEachTest is true)
			TestSetup();
		}

		public void RecordTestSetup()
		{
			var name = GetCurrentTestName();
			_testOutput?.WriteLine($">>>>> {DateTime.Now} {name} Start");
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
			var name = GetCurrentTestName();
			_testOutput?.WriteLine($">>>>> {DateTime.Now} {name} Stop");
		}

		protected virtual void FixtureSetup()
		{
			var name = GetCurrentTestName();
			_testOutput?.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
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
				var name = GetCurrentTestName();
				_testOutput?.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
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

					// xUnit equivalent: throw exception will fail the test
					throw new InvalidOperationException("The app was expected to be running still, investigate as possible crash");
				}
			}
			finally
			{
				// In xUnit, we don't have direct access to test outcome from NUnit
				// We'll need to handle this differently - let exceptions bubble up
				try
				{
					SaveDeviceDiagnosticInfo();
					SaveUIDiagnosticInfo();
				}
				catch
				{
					// Log but don't interfere with the actual test result
				}
			}
		}

		public void OneTimeSetup()
		{
			InitialSetup(UITestContextSetupFixture.ServerContext);
			try
			{
				if (!ResetAfterEachTest)
				{
					//SaveDiagnosticLogs("BeforeFixtureSetup");
					FixtureSetup();
				}
			}
			catch
			{
				SaveDeviceDiagnosticInfo();
				SaveUIDiagnosticInfo();
				throw;
			}
		}

		public void OneTimeTearDown()
		{
			// In xUnit, we can't easily detect setup failures like NUnit
			// We'll just perform the cleanup
			FixtureOneTimeTearDown();
		}

		// IDisposable implementation - equivalent to OneTimeTearDown
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				OneTimeTearDown();
				_disposed = true;
			}
		}

		private string GetCurrentTestName()
		{
			// In xUnit, getting current test name is more complex
			// We'll use the calling method name as a fallback
			var stackTrace = new System.Diagnostics.StackTrace();
			for (int i = 1; i < stackTrace.FrameCount; i++)
			{
				var method = stackTrace.GetFrame(i)?.GetMethod();
				if (method != null && method.GetCustomAttributes(typeof(FactAttribute), false).Any())
				{
					return method.Name;
				}
				if (method != null && method.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
				{
					return method.Name;
				}
			}
			return "UnknownTest";
		}

		void SaveDeviceDiagnosticInfo([CallerMemberName] string? note = null)
		{
			try
			{
				var types = App.GetLogTypes().ToArray();
				_testOutput?.WriteLine($">>>>> {DateTime.Now} Log types: {string.Join(", ", types)}");

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
				var name = GetCurrentTestName();
				_testOutput?.WriteLine($">>>>> {DateTime.Now} The SaveDeviceDiagnosticInfo threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
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

			var name =
				GetCurrentTestName();

			return Path.Combine(logDir, $"{name}-{_testDevice}{note}{filename}");
		}

		void AddTestAttachment(string filePath, string? description = null)
		{
			try
			{
				// xUnit doesn't have direct equivalent to TestContext.AddTestAttachment
				// We'll just log the file path for now
				_testOutput?.WriteLine($"Test attachment: {filePath} - {description}");
			}
			catch (FileNotFoundException e) when (e.Message == "Test attachment file path could not be found.")
			{
				// Add the file path to better troubleshoot when these errors occur
				throw new FileNotFoundException($"Test attachment file path could not be found: '{filePath}' {description}", e);
			}
		}
	}
}