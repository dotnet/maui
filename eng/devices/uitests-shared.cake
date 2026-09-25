#load "../cake/helpers.cake"

if (!IsCIBuild() && GetBuildVariable("workloads", "notset") == "notset")
{
	SetEnvironmentVariable("workloads", "global");
}

#load "../cake/dotnet.cake"
#load "./devices-shared.cake"
#load "../scripts/UITestRetry.cs"

bool deviceCreate = Argument("create", TARGET.ToLower() != "uitest-build");
bool deviceBoot = Argument("boot", TARGET.ToLower() != "uitest-build");
bool targetBoot = TARGET.ToLower() == "boot";
bool targetCleanup = TARGET.ToLower() == "cleanup";
bool deviceBootWait = Argument("wait", true);

void RunUITestsWithRetry(string project, string config, string toolPath, string resultsFileName)
{
	var resultsPath = GetTestResultsDirectory().CombineWithFilePath(resultsFileName + ".trx").FullPath;
	UITestRetry.Run(resultsPath, testFilter, IsCIBuild(), (filter, outputPath) =>
	{
		try
		{
			RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true,
				resultsFileNameWithoutExtension: System.IO.Path.GetFileNameWithoutExtension(outputPath), filter: filter);
			return 0;
		}
		catch (CakeException exception)
		{
			Warning("UI test command failed: {0}", exception.Message);
			return 1;
		}
	});
}