using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UITest.Core;

namespace UITest.Appium.NUnit
{
	public abstract class UITestBase : UITestContextBase
	{
		protected virtual bool ResetAfterEachTest => false;

		// Store paths of diagnostic files captured during OneTimeSetUp failure
		// so they can be re-attached to individual test results in TearDown
		// (NUnit doesn't attach files from OneTimeSetUp to individual test results in Azure DevOps)
		private readonly List<string> _fixtureSetupDiagnosticFiles = new();
		private bool _fixtureSetupFailed = false;

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
			UITestBaseTearDown();

			// If the fixture setup failed, re-attach diagnostic files to each individual test
			// so they appear in Azure DevOps test results (NUnit doesn't do this automatically
			// for files attached during OneTimeSetUp)
			if (_fixtureSetupFailed)
			{
				foreach (var filePath in _fixtureSetupDiagnosticFiles)
				{
					if (File.Exists(filePath))
					{
						AddTestAttachment(filePath, $"[FixtureSetup] {Path.GetFileName(filePath)}");
					}
				}
			}

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

		protected virtual void FixtureSetup()
		{
			var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
			TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {nameof(FixtureSetup)} for {name}");
		}

		protected virtual void FixtureOneTimeTearDown()
		{
			try
			{
				
				if (Device is TestDevice.Mac)
				{
					// For Mac, here needed to close the app itself, re-open happens on InitialSetup
					Close();
				}
				else if (!ResetAfterEachTest)
				{
					Reset();
				}
					 
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
				ApplicationState appState;
				try
				{
					appState = App.AppState;
				}
				catch (TimeoutException)
				{
					// Let unresponsive-app timeouts bubble to the outer TimeoutException handler.
					throw;
				}
				catch (Exception)
				{
					// AppState query itself can hang if the app is completely unresponsive.
					// Force-close the app and treat it as not running.
					App.CommandExecutor.Execute("forceCloseApp", new Dictionary<string, object>());
					appState = ApplicationState.NotRunning;
				}

				if (appState != ApplicationState.Running)
				{
					SaveDeviceDiagnosticInfo();

					if (!ResetAfterEachTest)
					{
						Reset();
						FixtureSetup();
					}

					// Assert.Fail will immediately exit the test which is desirable as the app is not
					// running anymore so we can't capture any UI structures or any screenshots
					Assert.Fail("The app was expected to be running still, investigate as possible crash");
				}
			}
			catch (TimeoutException ex)
			{
				// App is stuck in an infinite loop (e.g., layout cycle). Force-terminate and reset.
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} App became unresponsive, force-closing: {ex.Message}");
				try
				{
					App.CommandExecutor.Execute("forceCloseApp", new Dictionary<string, object>());
				}
				catch { /* best effort */ }

				if (!ResetAfterEachTest)
				{
					try
					{
						Reset();
						FixtureSetup();
					}
					catch (Exception resetEx)
					{
						TestContext.Error.WriteLine($">>>>> {DateTime.Now} Reset after force-close failed: {resetEx.Message}");
					}
				}

				Assert.Fail($"The app became unresponsive and was force-terminated: {ex.Message}");
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
				_fixtureSetupFailed = true;
				SaveDeviceDiagnosticInfo(storeForReattachment: true);
				SaveUIDiagnosticInfo(storeForReattachment: true);
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

		void SaveDeviceDiagnosticInfo([CallerMemberName] string? note = null, bool storeForReattachment = false)
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

						// Store path for re-attachment to individual tests if this is from fixture setup
						if (storeForReattachment)
						{
							_fixtureSetupDiagnosticFiles.Add(logsPath);
						}
					}
				}
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The SaveDeviceDiagnosticInfo threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		protected bool SaveUIDiagnosticInfo([CallerMemberName] string? note = null, bool storeForReattachment = false)
		{
			if (App.AppState != ApplicationState.Running)
				return false;

			var screenshotPath = GetGeneratedFilePath("ScreenShot.png", note);
			if (screenshotPath is not null)
			{
				_ = App.Screenshot(screenshotPath);

				AddTestAttachment(screenshotPath, Path.GetFileName(screenshotPath));

				// Store path for re-attachment to individual tests if this is from fixture setup
				if (storeForReattachment)
				{
					_fixtureSetupDiagnosticFiles.Add(screenshotPath);
				}
			}

			var pageSourcePath = GetGeneratedFilePath("PageSource.txt", note);
			if (pageSourcePath is not null)
			{
				File.WriteAllText(pageSourcePath, App.ElementTree);

				AddTestAttachment(pageSourcePath, Path.GetFileName(pageSourcePath));

				// Store path for re-attachment to individual tests if this is from fixture setup
				if (storeForReattachment)
				{
					_fixtureSetupDiagnosticFiles.Add(pageSourcePath);
				}
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