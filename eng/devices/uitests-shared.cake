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

void RunUITestsWithRetry(string project, string config, string toolPath, string resultsFileName, Action onFinalFailure = null)
{
	int numOfRetries = IsCIBuild() ? 1 : 0;
	var resultDirectory = MakeAbsolute(GetTestResultsDirectory()).FullPath;
	var nunitDirectory = System.IO.Path.Combine(resultDirectory, $"nunit-{Guid.NewGuid():N}");
	var retryDirectory = System.IO.Path.Combine(nunitDirectory, "retry");
	var resultsFile = System.IO.Path.Combine(resultDirectory, resultsFileName + ".trx");
	var retryFilter = testFilter;
	XDocument originalResults = null;

	for (int retryCount = 0; retryCount <= numOfRetries; retryCount++)
	{
		try
		{
			Information("UI test attempt: {0}", retryCount + 1);
			RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true,
				resultsFileNameWithoutExtension: resultsFileName, filter: retryFilter,
				resultsDirectory: originalResults == null ? null : retryDirectory,
				nunitResultsDirectory: numOfRetries > 0 ? (originalResults == null ? nunitDirectory : retryDirectory) : null);
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
				retryFilter = UITestRetry.GetFilter(testFilter,
					GetFiles($"{nunitDirectory}/*.xml").Select(file => XDocument.Load(file.FullPath)));
				originalResults = XDocument.Load(resultsFile);
				EnsureDirectoryExists(retryDirectory);
				CopyFile(resultsFile, System.IO.Path.Combine(nunitDirectory, "first-attempt.trx"));
				Information("Retrying only failed UI test categories: {0}", retryFilter);
			}
			catch (Exception metadataException) when (metadataException is System.IO.IOException || metadataException is System.Xml.XmlException || metadataException is InvalidOperationException)
			{
				Warning("Unable to narrow the UI test retry; rerunning the original filter: {0}", metadataException.Message);
				retryFilter = testFilter;
				originalResults = null;
			}
		}
		finally
		{
			if (retryCount > 0 && originalResults != null)
			{
				var retryResultsFile = System.IO.Path.Combine(retryDirectory, resultsFileName + ".trx");
				if (System.IO.File.Exists(retryResultsFile))
				{
					var retryResults = XDocument.Load(retryResultsFile);
					XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
					var originalDeployment = (string)originalResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
					var retryDeployment = (string)retryResults.Root.Element(ns + "TestSettings")?.Element(ns + "Deployment")?.Attribute("runDeploymentRoot");
					if (!string.IsNullOrEmpty(originalDeployment) && !string.IsNullOrEmpty(retryDeployment))
					{
						var retryAttachments = System.IO.Path.Combine(retryDirectory, retryDeployment);
						if (DirectoryExists(retryAttachments))
							CopyDirectory(retryAttachments, System.IO.Path.Combine(resultDirectory, originalDeployment));
					}
					UITestRetry.MergeResults(originalResults, retryResults);
					originalResults.Save(resultsFile);
				}
				else
				{
					Warning("No retry TRX was produced; retaining the first attempt's failures.");
				}
			}
		}
	}
}