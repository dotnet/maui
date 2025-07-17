using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UITest.Core;

namespace UITest.Appium.NUnit
{
	public abstract class UITestBase : UITestContextBase
	{
		// Timeout configurations
		protected virtual TimeSpan TeardownTimeout => TimeSpan.FromSeconds(30);
		protected virtual TimeSpan ResetTimeout => TimeSpan.FromSeconds(15);
		
		protected virtual bool ResetAfterEachTest => false;

		public UITestBase(TestDevice testDevice)
			: base(testDevice)
		{
		}

		public void RecordTestSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");
		}

		[SetUp]
		public virtual void TestSetup()
		{
			RecordTestSetup();
			
			if (ResetAfterEachTest)
			{
				FixtureSetup();
			}
		}
		
		[TearDown]
		public virtual void TestTearDown()
		{
			RecordTestTeardown();

			// Run teardown with overall timeout protection
			var teardownTask = Task.Run(ExecuteTeardown);

			if (!teardownTask.Wait(TeardownTimeout))
			{
				TestContext.Error.WriteLine(
					$">>>>> {DateTime.Now} TearDown timed out after {TeardownTimeout.TotalSeconds} seconds");
				SaveDeviceDiagnosticInfo();
				return;
			}

			// Check for any exceptions from teardown
			if (teardownTask.Exception is not null)
			{
				TestContext.Error.WriteLine(
					$">>>>> {DateTime.Now} TearDown failed: {teardownTask.Exception.InnerException?.Message}");
			}
		}

		void ExecuteTeardown()
		{
			// Always check for diagnostics first, before any reset operations
			UITestBaseTearDown();
            
			// Handle reset after each test efficiently
			if (ResetAfterEachTest)
			{
				ExecuteReset();
			}
		}
		
		void ExecuteReset()
		{
			try
			{
				// Only reset if app is still running, no point resetting a crashed app
				if (App?.AppState == ApplicationState.Running)
				{
					var resetTask = Task.Run(Reset);
					
					if (!resetTask.Wait(ResetTimeout))
					{
						TestContext.Error.WriteLine($">>>>> {DateTime.Now} Reset timed out after {ResetTimeout.TotalSeconds} seconds");
					}
					else if (resetTask.Exception != null)
					{
						TestContext.Error.WriteLine($">>>>> {DateTime.Now} Reset failed: {resetTask.Exception.InnerException?.Message}");
					}
				}
				else
				{
					TestContext.Progress.WriteLine($">>>>> {DateTime.Now} App not running ({App?.AppState}), skipping reset in teardown");
				}
			}
			catch (Exception resetException)
			{
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} Reset in teardown failed: {resetException.Message}");
				// Don't rethrow, teardown should be resilient
			}
		}

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

		protected virtual void FixtureOneTimeTearDown()
		{
			try
			{
				if (!ResetAfterEachTest)
					Reset();
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		public void UITestBaseTearDown()
		{
			try
			{
				// Check app state with fallback
				var appState = App?.AppState ?? ApplicationState.Unknown;

				if (appState == ApplicationState.NotRunning)
				{
					// App has crashed, save diagnostics
					SaveDeviceDiagnosticInfo();
					Assert.Fail("The app is not running, investigate as possible crash");
				}
				else
				{
					// App is running - handle reset if needed
					if (!ResetAfterEachTest)
					{
						try
						{
							Reset();
							FixtureSetup();
						}
						catch (Exception resetException)
						{
							TestContext.Error.WriteLine($">>>>> {DateTime.Now} Reset after test failed: {resetException.Message}");
						}
					}
					// If ResetAfterEachTest is true, reset will happen in TestTearDown()
				}
			}
			finally
			{
				var testOutcome = TestContext.CurrentContext.Result.Outcome;
				if (testOutcome == ResultState.Error || testOutcome == ResultState.Failure)
				{
					SaveDeviceDiagnosticInfo();
					SaveUIDiagnosticInfo();
				}
			}
		}
		
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			InitialSetup(UITestContextSetupFixture.ServerContext);
			try
			{
				if (!ResetAfterEachTest)
				{
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			var outcome = TestContext.CurrentContext.Result.Outcome;

			// We only care about setup failures as regular test failures will already do logging
			if (outcome.Status == ResultState.SetUpFailure.Status &&
				outcome.Site == ResultState.SetUpFailure.Site)
			{
				SaveDeviceDiagnosticInfo();

				if (App.AppState == ApplicationState.Running)
					SaveUIDiagnosticInfo();
			}

			FixtureOneTimeTearDown();
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
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The SaveDeviceDiagnosticInfo threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
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