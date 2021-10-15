// Contains .NET 6-related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";
var dotnetProjectPath = "./build/DotNet/DotNet.csproj";
var graphicsSln = "Microsoft.Maui.Graphics-net6.sln";

// Tasks for CI

Task("dotnet")
    .Description("Provisions .NET 6 into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        Information("Provisions");
        // if (!localDotnet) 
        //     return;

        DotNetCoreBuild(dotnetProjectPath, new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{logDirectory}/dotnet-{configuration}.binlog")
                .SetConfiguration(configuration),
        });
    });

Task("dotnet-build")
    .IsDependentOn("dotnet")
    .Description("Build the solutions")
    .Does(() =>
    {
        RunMSBuildWithDotNet(graphicsSln);
    });


Task("dotnet-local-workloads")
    .Does(() =>
    {
        if (!localDotnet) 
            return;
        
        DotNetCoreBuild(dotnetProjectPath, new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{logDirectory}/dotnet-{configuration}.binlog")
                .SetConfiguration(configuration)
                .WithProperty("InstallWorkloadPacks", "false"),
        });

        DotNetCoreBuild(dotnetProjectPath, new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{logDirectory}/dotnet-install-{configuration}.binlog")
                .SetConfiguration(configuration)
                .WithTarget("Install"),
            ToolPath = dotnetPath,
        });
    });


string FindMSBuild()
{
    if (!string.IsNullOrWhiteSpace(MSBuildExe))
        return MSBuildExe;

    if (IsRunningOnWindows())
    {
        var vsInstallation = VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.Component.MSBuild", IncludePrerelease = true });
        if (vsInstallation != null)
        {
            var path = vsInstallation.CombineWithFilePath(@"MSBuild\Current\Bin\MSBuild.exe");
            if (FileExists(path))
                return path.FullPath;

            path = vsInstallation.CombineWithFilePath(@"MSBuild\15.0\Bin\MSBuild.exe");
            if (FileExists(path))
                return path.FullPath;
        }
    }
    return "msbuild";
}

void SetDotNetEnvironmentVariables()
{
    var dotnet = MakeAbsolute(Directory("./bin/dotnet/")).ToString();

    SetEnvironmentVariable("DOTNET_INSTALL_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_ROOT", dotnet);
    SetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0");
    SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "true");
    SetEnvironmentVariable("PATH", dotnet, prepend: true);
}


// NOTE: These methods work as long as the "dotnet" target has already run

void RunMSBuildWithDotNet(string sln, Dictionary<string, string> properties = null, bool deployAndRun = false)
{
    var name = System.IO.Path.GetFileNameWithoutExtension(sln);
    var binlog = $"\"{logDirectory}/{name}-{configuration}.binlog\"";
    
    if(localDotnet)
        SetDotNetEnvironmentVariables();

    // If we're not on Windows, use ./bin/dotnet/dotnet
    if (!IsRunningOnWindows() || deployAndRun)
    {
        var msbuildSettings = new DotNetCoreMSBuildSettings()
            .SetConfiguration(configuration)
            .EnableBinaryLogger(binlog);

        if (properties != null)
        {
            foreach (var property in properties)
            {
                msbuildSettings.WithProperty(property.Key, property.Value);
            }
        }

        if (deployAndRun)
            msbuildSettings.WithTarget("Run");

        var dotnetBuildSettings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = msbuildSettings,
        };

        if (localDotnet)
            dotnetBuildSettings.ToolPath = dotnetPath;

        DotNetCoreBuild(sln, dotnetBuildSettings);
    }
    else
    {
        // Otherwise we need to run MSBuild for WinUI support
        var msbuild = FindMSBuild();
        Information("Using MSBuild: {0}", msbuild);
        var msbuildSettings = new MSBuildSettings { ToolPath = msbuild }
            .WithRestore()
            .SetConfiguration(configuration)
            .EnableBinaryLogger(binlog);

        if (properties != null)
        {
            foreach (var property in properties)
            {
                msbuildSettings.WithProperty(property.Key, property.Value);
            }
        }

        MSBuild(sln, msbuildSettings);
    }
}

// void RunTestWithLocalDotNet(string csproj)
// {
//     var name = System.IO.Path.GetFileNameWithoutExtension(csproj);
//     var binlog = $"{logDirectory}/{name}-{configuration}.binlog";
//     var results = $"{name}-{configuration}.trx";

//     if(localDotnet)
//         SetDotNetEnvironmentVariables();

//     DotNetCoreTest(csproj,
//         new DotNetCoreTestSettings
//         {
//             Configuration = configuration,
//             ToolPath = dotnetPath,
//             NoBuild = true,
//             Logger = $"trx;LogFileName={results}",
//             ResultsDirectory = testResultsDirectory,
//             ArgumentCustomization = args => args.Append($"-bl:{binlog}")
//         });
// }
