#load "../cake/helpers.cake"

if (!IsCIBuild() && GetBuildVariable("workloads", "notset") == "notset")
{
	SetEnvironmentVariable("workloads", "global");
}

#load "../cake/dotnet.cake"
#load "./devices-shared.cake"
#load "./uitest-retry.cake"

bool deviceCreate = Argument("create", TARGET.ToLower() != "uitest-build");
bool deviceBoot = Argument("boot", TARGET.ToLower() != "uitest-build");
bool targetBoot = TARGET.ToLower() == "boot";
bool targetCleanup = TARGET.ToLower() == "cleanup";
bool deviceBootWait = Argument("wait", true);

void RunUITestSetupWithRetry(Action setup)
{
	int attempt = 1;
	UITestRetry.RunSetup(setup, IsCIBuild() && TARGET.StartsWith("uitest", StringComparison.OrdinalIgnoreCase),
		ex => Warning("UI test setup/build attempt {0}/5 failed; retrying before running tests: {1}", attempt++, ex.Message));
}

void RunUITestsWithRetry(string project, string config, string toolPath, string resultsFileName, Action onFinalFailure = null)
{
	int numOfRetries = IsCIBuild() ? 1 : 0;
	var resultDirectory = MakeAbsolute(GetTestResultsDirectory()).FullPath;
	var nunitDirectory = System.IO.Path.Combine(resultDirectory, $"nunit-{Guid.NewGuid():N}");
	var retryDirectory = System.IO.Path.Combine(nunitDirectory, "retry");
	var resultsFile = System.IO.Path.Combine(resultDirectory, resultsFileName + ".trx");
	string retrySettingsFile = null;
	XDocument originalResults = null;
	XDocument[] originalReports = Array.Empty<XDocument>();

	for (int retryCount = 0; retryCount <= numOfRetries; retryCount++)
	{
		XDocument[] reports = Array.Empty<XDocument>();
		System.Runtime.ExceptionServices.ExceptionDispatchInfo reportFailure = null;
		bool archiveFailed = false;
		try
		{
			Information("UI test attempt: {0}", retryCount + 1);
			var attemptDirectory = retryCount == 0 ? resultDirectory : retryDirectory;
			var attemptNUnitDirectory = retryCount == 0 ? nunitDirectory : retryDirectory;
			if (retryCount > 0)
				EnsureDirectoryExists(retryDirectory);
			System.Runtime.ExceptionServices.ExceptionDispatchInfo testFailure = null;
			try
			{
				RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true,
					resultsFileNameWithoutExtension: resultsFileName, filter: testFilter,
					resultsDirectory: retryCount == 0 ? null : retryDirectory,
					nunitResultsDirectory: attemptNUnitDirectory, settingsFile: retrySettingsFile);
			}
			catch (Exception ex)
			{
				testFailure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
			}

			bool nunitFailed = false;
			var attemptResultsFile = System.IO.Path.Combine(attemptDirectory, resultsFileName + ".trx");
			string[] reportFiles = Array.Empty<string>();
			try
			{
				// Preserve raw evidence before reconciliation or retry selection can fail.
				reportFiles = GetFiles($"{attemptNUnitDirectory}/*.xml").Select(file => file.FullPath).ToArray();
				var rawDirectory = System.IO.Path.Combine(attemptNUnitDirectory, "raw");
				System.IO.Directory.CreateDirectory(rawDirectory);
				if (System.IO.File.Exists(attemptResultsFile))
					System.IO.File.Copy(attemptResultsFile, System.IO.Path.Combine(rawDirectory, resultsFileName + ".trx"));
				foreach (var file in reportFiles)
					System.IO.File.Copy(file, System.IO.Path.Combine(rawDirectory, System.IO.Path.GetFileName(file)));
			}
			catch (Exception archiveException) when (archiveException is System.IO.IOException || archiveException is UnauthorizedAccessException)
			{
				archiveFailed = true;
				Warning("Unable to archive raw UI test reports; stopping before they can be overwritten: {0}", archiveException.Message);
				testFailure?.Throw();
				throw;
			}

			var availableReports = new List<XDocument>();
			foreach (var file in reportFiles)
			{
				try
				{
					var report = XDocument.Load(file);
					if (report.Root?.Name != "test-run")
						throw new InvalidOperationException("NUnit XML is missing a test-run.");
					availableReports.Add(report);
				}
				catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
				{
					Warning("Unable to read NUnit report {0}: {1}", file, metadataException.Message);
					reportFailure = reportFailure ?? System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(metadataException);
				}
			}
			reports = availableReports.ToArray();
			try
			{
				if (reports.Length == 0)
				{
					Warning("No readable NUnit XML was produced; using the VSTest exit status for this attempt.");
				}
				else
				{
					var attemptResults = XDocument.Load(attemptResultsFile);
					nunitFailed = UITestRetry.ApplyNUnitFailures(attemptResults, reports);
					if (nunitFailed)
						attemptResults.Save(attemptResultsFile);
				}
			}
			catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
			{
				Warning("Unable to reconcile NUnit results: {0}", metadataException.Message);
				reportFailure = reportFailure ?? System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(metadataException);
			}

			bool retryResultsMissing = retryCount > 0 && !System.IO.File.Exists(attemptResultsFile);
			try
			{
				if (retryCount > 0)
				{
					if (!retryResultsMissing)
					{
						if (originalResults == null)
							CopyFile(attemptResultsFile, resultsFile);
						var retryResults = XDocument.Load(attemptResultsFile);
						XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
						var retryDeployment = (string)retryResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
						var publishedDeployment = originalResults == null ? retryDeployment :
							(string)originalResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
						if (!string.IsNullOrEmpty(publishedDeployment) && !string.IsNullOrEmpty(retryDeployment))
						{
							var retryAttachments = System.IO.Path.Combine(retryDirectory, retryDeployment);
							if (DirectoryExists(retryAttachments))
								CopyDirectory(retryAttachments, System.IO.Path.Combine(resultDirectory, publishedDeployment));
						}
						if (originalResults != null)
						{
							UITestRetry.MergeResults(originalResults, retryResults, originalReports, reports);
							originalResults.Save(resultsFile);
						}
					}
					else
					{
						Warning("No retry TRX was produced; retaining the first attempt's reports.");
					}
				}
			}
			catch (Exception metadataException) when ((testFailure != null || reportFailure != null) &&
				(metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException))
			{
				Warning("Unable to merge retry results; preserving the attempt's failure: {0}", metadataException.Message);
			}

			testFailure?.Throw();
			reportFailure?.Throw();
			XNamespace trxNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
			if (nunitFailed || retryResultsMissing || (string)originalResults?.Root.Element(trxNamespace + "ResultSummary")?.Attribute("outcome") == "Failed")
				throw new InvalidOperationException("UI test results contain unresolved test or fixture failures.");
			break;
		}
		catch (Exception ex)
		{
			if (retryCount == numOfRetries || archiveFailed)
			{
				onFinalFailure?.Invoke();
				throw;
			}

			Warning("UI test attempt failed: {0}", ex.Message);
			try
			{
				if (reportFailure != null)
					throw new InvalidOperationException("NUnit reports could not all be reconciled.");
				var filter = UITestRetry.GetFilter(testFilter, reports);
				EnsureDirectoryExists(retryDirectory);
				retrySettingsFile = System.IO.Path.Combine(retryDirectory, "retry.runsettings");
				UITestRetry.CreateRunSettings(filter).Save(retrySettingsFile);
				Information("Retrying failed UI tests (whole feature-matrix fixtures; category recovery for other fixture errors): {0}", filter);
			}
			catch (Exception metadataException) when (metadataException is InvalidOperationException || metadataException is FormatException ||
				metadataException is System.IO.IOException || metadataException is UnauthorizedAccessException)
			{
				Warning("Unable to narrow the UI test retry; rerunning the original filter: {0}", metadataException.Message);
				retrySettingsFile = null;
			}

			try
			{
				originalResults = XDocument.Load(resultsFile);
				originalReports = reports;
			}
			catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
			{
				Warning("Unable to read the first attempt's TRX for merging; rerunning the original filter: {0}", metadataException.Message);
				retrySettingsFile = null;
				originalResults = null;
				originalReports = Array.Empty<XDocument>();
			}
		}
	}
}