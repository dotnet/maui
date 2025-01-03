#addin "nuget:?package=NuGet.Packaging&version=6.7.0"
#addin "nuget:?package=NuGet.Protocol&version=6.7.0"

using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

bool isCleanSet = HasArgument("clean") || IsTarget("clean");

Task("Clean")
    .WithCriteria(isCleanSet)
    .Description("Deletes all the obj/bin directories")
    .Does(() =>
{
    List<string> foldersToClean = new List<string>();

    foreach (var item in new [] {"obj", "bin"})
    {
        foreach(string f in System.IO.Directory.GetDirectories(".", item, SearchOption.AllDirectories))
        {
            var directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
            if(f.StartsWith($".{directorySeparatorChar}bin") || f.StartsWith($".{directorySeparatorChar}tools"))
                continue;

            // this is here as a safety check
            if(!f.StartsWith($".{directorySeparatorChar}src"))
                continue;

            CleanDirectories(f);
        }        
    } 
});

DirectoryPath _artifactStagingDirectory;
DirectoryPath GetArtifactStagingDirectory() =>
    _artifactStagingDirectory ??= MakeAbsolute(Directory(EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY", "artifacts")));

DirectoryPath _logDirectory;
DirectoryPath GetLogDirectory() => 
    _logDirectory ??= MakeAbsolute(Directory(EnvironmentVariable("LogDirectory", $"{GetArtifactStagingDirectory()}/logs")));

DirectoryPath _testResultsDirectory;
DirectoryPath GetTestResultsDirectory() => 
    _testResultsDirectory ??= MakeAbsolute(Directory(EnvironmentVariable("TestResultsDirectory", $"{GetArtifactStagingDirectory()}/test-results")));

DirectoryPath _diffDirectory;
DirectoryPath GetDiffDirectory() => 
    _diffDirectory ??= MakeAbsolute(Directory(EnvironmentVariable("ApiDiffDirectory", $"{GetArtifactStagingDirectory()}/api-diff")));

DirectoryPath _tempDirectory;
DirectoryPath GetTempDirectory() => 
    _tempDirectory ??= MakeAbsolute(Directory(EnvironmentVariable("AGENT_TEMPDIRECTORY", EnvironmentVariable("TEMP", EnvironmentVariable("TMPDIR", "../maui-temp")) + "/" + Guid.NewGuid())));

string GetAgentName() =>
    EnvironmentVariable("AGENT_NAME", "");

bool IsCIBuild() =>
    !String.IsNullOrWhiteSpace(GetAgentName());

bool IsHostedAgent() =>
    GetAgentName().StartsWith("Azure Pipelines") || GetAgentName().StartsWith("Hosted Agent");

T GetBuildVariable<T>(string key, T defaultValue)
{
    // on MAC all environment variables are upper case regardless of how you specify them in devops
    // And then Environment Variable check is case sensitive
    T upperCaseReturnValue = Argument(key.ToUpper(), EnvironmentVariable(key.ToUpper(), defaultValue));
    return Argument(key, EnvironmentVariable(key, upperCaseReturnValue));
}

string GetAndroidSDKPath()
{
    var ANDROID_SDK_ROOT = Argument("android", EnvironmentVariable("ANDROID_SDK_ROOT") ?? EnvironmentVariable("ANDROID_HOME"));

    if (string.IsNullOrEmpty(ANDROID_SDK_ROOT)) {
        throw new Exception("Environment variable 'ANDROID_SDK_ROOT' or 'ANDROID_HOME' must be set to the Android SDK root.");    
    }

    return ANDROID_SDK_ROOT;
}

public void PrintEnvironmentVariables()
{
    var envVars = EnvironmentVariables();

    string path;
    if (envVars.TryGetValue("PATH", out path))
    {
        Information("Path: {0}", path);
    }

    foreach(var envVar in envVars)
    {
        Information(
            "Key: {0}\tValue: \"{1}\"",
            envVar.Key,
            envVar.Value
            );
    };
}

void SetEnvironmentVariable(string name, string value, bool prepend = false)
{
    var target = EnvironmentVariableTarget.Process;

    if (prepend)
        value = value + System.IO.Path.PathSeparator + EnvironmentVariable(name);

    Environment.SetEnvironmentVariable(name, value, target);

    Information("Setting environment variable: {0} = '{1}'", name, value);
}

bool IsTarget(string target) =>
    Argument<string>("target", "Default").Equals(target, StringComparison.InvariantCultureIgnoreCase);

bool TargetStartsWith(string target) =>
    Argument<string>("target", "Default").StartsWith(target, StringComparison.InvariantCultureIgnoreCase);

public async Task DownloadNuGetPackageAsync(string packageId, string version, string outputDirectory, string feedUri)
{
    // Create a source repository
    var repository = Repository.Factory.GetCoreV3(feedUri);
    
    // Find the package
    var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
    var packageVersion = NuGetVersion.Parse(version);
    var cacheContext = new SourceCacheContext();
    
    // Set up logging (optional, use NullLogger if you don't need logging)
    ILogger logger = NullLogger.Instance;

    // Download the package to the output directory
    EnsureDirectoryExists(outputDirectory);
    var packagePath = System.IO.Path.Combine(outputDirectory, $"{packageId}.{version}.nupkg");
    
    using (var fileStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        // Download package
        var success = await resource.CopyNupkgToStreamAsync(
            packageId, packageVersion, fileStream, cacheContext, logger, default);

        if (!success)
        {
            throw new Exception("Failed to download the package.");
        }

        Information("Package '{0} v{1}' downloaded successfully to {2}", packageId, version, packagePath);
    }
}
