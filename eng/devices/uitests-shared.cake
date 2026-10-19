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
	UITestRetry.RunSetup(setup, IsCIBuild() && TARGET.StartsWith("uitest", StringComparison.OrdinalIgnoreCase),
		ex => Warning("UI test setup/build failed; retrying this phase once before running tests: {0}", ex.Message));
}

void RunUITestsWithRetry(string project, string config, string toolPath, string resultsFileName, Action onFinalFailure = null)
{
	int numOfRetries = IsCIBuild() ? 1 : 0;
	var resultDirectory = MakeAbsolute(GetTestResultsDirectory()).FullPath;
	var nunitDirectory = System.IO.Path.Combine(resultDirectory, $"nunit-{Guid.NewGuid():N}");
	var retryDirectory = System.IO.Path.Combine(nunitDirectory, "retry");
	var resultsFile = System.IO.Path.Combine(resultDirectory, resultsFileName + ".trx");
	var retryFilter = testFilter;
	XDocument originalResults = null;
	XDocument[] originalReports = Array.Empty<XDocument>();

	for (int retryCount = 0; retryCount <= numOfRetries; retryCount++)
	{
		XDocument[] reports = Array.Empty<XDocument>();
		System.Runtime.ExceptionServices.ExceptionDispatchInfo reportFailure = null;
		try
		{
			Information("UI test attempt: {0}", retryCount + 1);
			var attemptDirectory = originalResults == null ? resultDirectory : retryDirectory;
			var attemptNUnitDirectory = originalResults == null ? nunitDirectory : retryDirectory;
			System.Runtime.ExceptionServices.ExceptionDispatchInfo testFailure = null;
			try
			{
				RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true,
					resultsFileNameWithoutExtension: resultsFileName, filter: retryFilter,
					resultsDirectory: originalResults == null ? null : retryDirectory,
					nunitResultsDirectory: attemptNUnitDirectory);
			}
			catch (Exception ex)
			{
				testFailure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
			}

			bool nunitFailed = false;
			var attemptResultsFile = System.IO.Path.Combine(attemptDirectory, resultsFileName + ".trx");
			var availableReports = new List<XDocument>();
			foreach (var file in GetFiles($"{attemptNUnitDirectory}/*.xml"))
			{
				try
				{
					var report = XDocument.Load(file.FullPath);
					if (report.Root?.Name != "test-run")
						throw new InvalidOperationException("NUnit XML is missing a test-run.");
					availableReports.Add(report);
				}
				catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
				{
					Warning("Unable to read NUnit report {0}: {1}", file.FullPath, metadataException.Message);
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

			try
			{
				if (retryCount > 0 && originalResults != null)
				{
					if (System.IO.File.Exists(attemptResultsFile))
					{
						var retryResults = XDocument.Load(attemptResultsFile);
						XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
						var originalDeployment = (string)originalResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
						var retryDeployment = (string)retryResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
						if (!string.IsNullOrEmpty(originalDeployment) && !string.IsNullOrEmpty(retryDeployment))
						{
							var retryAttachments = System.IO.Path.Combine(retryDirectory, retryDeployment);
							if (DirectoryExists(retryAttachments))
								CopyDirectory(retryAttachments, System.IO.Path.Combine(resultDirectory, originalDeployment));
						}
						UITestRetry.MergeResults(originalResults, retryResults, originalReports, reports);
						originalResults.Save(resultsFile);
					}
					else
					{
						Warning("No retry TRX was produced; retaining the first attempt's failures.");
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
			if (nunitFailed || (string)originalResults?.Root.Element(trxNamespace + "ResultSummary")?.Attribute("outcome") == "Failed")
				throw new InvalidOperationException("UI test results contain unresolved test or fixture failures.");
			break;
		}
		catch (Exception ex)
		{
			if (retryCount == numOfRetries)
			{
				onFinalFailure?.Invoke();
				throw;
			}

			Warning("UI test attempt failed: {0}", ex.Message);
			try
			{
				if (reportFailure != null)
					throw new InvalidOperationException("NUnit reports could not all be reconciled.");
				retryFilter = UITestRetry.GetFilter(testFilter, reports);
				Information("Retrying only failed UI test categories: {0}", retryFilter);
			}
			catch (Exception metadataException) when (metadataException is InvalidOperationException || metadataException is FormatException)
			{
				Warning("Unable to narrow the UI test retry; rerunning the original filter: {0}", metadataException.Message);
				retryFilter = testFilter;
			}

			try
			{
				originalResults = XDocument.Load(resultsFile);
				originalReports = reports;
				EnsureDirectoryExists(retryDirectory);
				CopyFile(resultsFile, System.IO.Path.Combine(nunitDirectory, "first-attempt.trx"));
			}
			catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
			{
				Warning("Unable to preserve the first attempt's results; rerunning the original filter: {0}", metadataException.Message);
				retryFilter = testFilter;
				originalResults = null;
				originalReports = Array.Empty<XDocument>();
			}
		}
	}
}