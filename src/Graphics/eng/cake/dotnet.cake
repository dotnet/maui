// Contains .NET 6-related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";

// Tasks for CI

Task("dotnet")
    .Description("Provisions .NET 6 into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        if (!localDotnet) 
            return;

        DotNetCoreBuild("./src/DotNet/DotNet.csproj", new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{logDirectory}/dotnet-{configuration}.binlog")
                .SetConfiguration(configuration),
        });
    });

Task("dotnet-local-workloads")
    .Does(() =>
    {
        if (!localDotnet) 
            return;
        
        DotNetCoreBuild("./build/DotNet/DotNet.csproj", new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{logDirectory}/dotnet-workloads-{configuration}.binlog")
                .SetConfiguration(configuration)
                .WithProperty("InstallDotNet", "false")
                .WithProperty("DotNetDirectory", dotnetInstallDirectory+"/")
        });
    });



Task("dotnet-diff")
    .Does(() =>
    {
        var nupkgs = GetFiles($"./artifacts/**/*.nupkg");
        if (!nupkgs.Any())
        {
            Warning($"##vso[task.logissue type=warning]No NuGet packages were found.");
        }
        else
        {
            // clean all working folders
            var diffCacheDir = tempDirectory.Combine("diffCache");
            EnsureDirectoryExists(diffCacheDir);
            CleanDirectories(diffCacheDir.FullPath);
            EnsureDirectoryExists(diffDirectory);
            CleanDirectories(diffDirectory.FullPath);

            // run the diff
            foreach (var nupkg in nupkgs)
            {
                DotNetCoreTool("api-tools", new DotNetCoreToolSettings
                {
                    DiagnosticOutput = true,
                    ArgumentCustomization = builder => builder
                        .Append("nuget-diff")
                        .AppendQuoted(nupkg.FullPath)
                        .Append("--latest")
                        // .Append("--verbose")
                        .Append("--prerelease")
                        .Append("--group-ids")
                        .Append("--ignore-unchanged")
                        .AppendSwitchQuoted("--output", diffDirectory.FullPath)
                        .AppendSwitchQuoted("--cache", diffCacheDir.FullPath)
                });
            }

            // clean working folders
            try
            {
                CleanDirectories(diffCacheDir.FullPath);
            }
            catch
            {
                Information("Unable to clean up diff cache directory.");
            }

            var diffs = GetFiles($"{diffDirectory}/**/*.md");
            if (!diffs.Any())
            {
                Warning($"##vso[task.logissue type=warning]No NuGet diffs were found.");
            }
            else
            {
                // clean working folders
                var temp = diffCacheDir.Combine("md-files");
                EnsureDirectoryExists(diffCacheDir);
                CleanDirectories(diffCacheDir.FullPath);

                // copy and rename files for UI
                foreach (var diff in diffs)
                {
                    var segments = diff.Segments.Reverse().ToArray();
                    var nugetId = segments[2];
                    var platform = segments[1];
                    var assembly = ((FilePath)segments[0]).GetFilenameWithoutExtension().GetFilenameWithoutExtension();
                    var breaking = segments[0].EndsWith(".breaking.md");

                    // using non-breaking spaces
                    var newName = breaking ? "[BREAKING]   " : "";
                    newName += $"{nugetId}    {assembly} ({platform}).md";
                    var newPath = diffCacheDir.CombineWithFilePath(newName);

                    CopyFile(diff, newPath);
                }

                // push changes to UI
                var temps = GetFiles($"{diffCacheDir}/**/*.md");
                foreach (var t in temps.OrderBy(x => x.FullPath))
                {
                    Information($"##vso[task.uploadsummary]{t}");
                }
            }
        }
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

void StartVisualStudioForDotNet6(string sln = null)
{
    if (sln == null)
    {
        if (IsRunningOnWindows())
        {
            sln = "./Microsoft.Maui.Graphics-net6.sln";
        }
        else
        {
            sln = "./Microsoft.Maui.Graphics.Mac-net6.slnf";
        }
    }
    if (isCIBuild)
    {
        Information("This target should not run on CI.");
        return;
    }
    if(localDotnet)
    {
        SetDotNetEnvironmentVariables();
        SetEnvironmentVariable("_ExcludeMauiProjectCapability", "true");
    }
    if (IsRunningOnWindows())
    {
        bool includePrerelease = true;

        if (!String.IsNullOrEmpty(vsVersion))
            includePrerelease = (vsVersion == "preview");

        var vsLatest = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = includePrerelease, });
        if (vsLatest == null)
            throw new Exception("Unable to find Visual Studio!");
       
        StartProcess(vsLatest.CombineWithFilePath("./Common7/IDE/devenv.exe"), sln);
    }
    else
    {
        StartProcess("open", new ProcessSettings{ Arguments = sln });
    }
}

// NOTE: These methods work as long as the "dotnet" target has already run

void RunMSBuildWithDotNet(
    string sln,
    Dictionary<string, string> properties = null,
    string target = "Build",
    bool warningsAsError = false,
    bool restore = true,
    string targetFramework = null)
{
    var name = System.IO.Path.GetFileNameWithoutExtension(sln);
    var binlog = string.IsNullOrEmpty(targetFramework) ?
        $"\"{logDirectory}/{name}-{configuration}-{target}.binlog\"" :
        $"\"{logDirectory}/{name}-{configuration}-{target}-{targetFramework}.binlog\"";
    
    if(localDotnet)
        SetDotNetEnvironmentVariables();

    // If we're not on Windows, use ./bin/dotnet/dotnet
    if (!IsRunningOnWindows() || target == "Run")
    {
        var msbuildSettings = new DotNetCoreMSBuildSettings()
            .SetConfiguration(configuration)
            .SetMaxCpuCount(0)
            .WithTarget(target)
            .EnableBinaryLogger(binlog);

        if (warningsAsError)
        {
            msbuildSettings.TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error);
        }

        if (properties != null)
        {
            foreach (var property in properties)
            {
                msbuildSettings.WithProperty(property.Key, property.Value);
            }
        }

        var dotnetBuildSettings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = msbuildSettings,
        };
        dotnetBuildSettings.ArgumentCustomization = args =>
        {
            if (!restore)
                args.Append("--no-restore");

            if (!string.IsNullOrEmpty(targetFramework))
                args.Append($"-f {targetFramework}");

            return args;
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
            .SetConfiguration(configuration)
            .SetMaxCpuCount(0)
            .WithTarget(target)
            .EnableBinaryLogger(binlog);

        if (warningsAsError)
        {
            msbuildSettings.WarningsAsError = true;
        }
        if (restore)
        {
            msbuildSettings.WithRestore();
        }
        if (!string.IsNullOrEmpty(targetFramework))
        {
            msbuildSettings.WithProperty("TargetFramework", targetFramework);
        }

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

void RunTestWithLocalDotNet(string csproj)
{
    var name = System.IO.Path.GetFileNameWithoutExtension(csproj);
    var binlog = $"{logDirectory}/{name}-{configuration}.binlog";
    var results = $"{name}-{configuration}.trx";

    if(localDotnet)
        SetDotNetEnvironmentVariables();

    DotNetCoreTest(csproj,
        new DotNetCoreTestSettings
        {
            Configuration = configuration,
            ToolPath = dotnetPath,
            NoBuild = true,
            Logger = $"trx;LogFileName={results}",
            ResultsDirectory = testResultsDirectory,
            ArgumentCustomization = args => args.Append($"-bl:{binlog}")
        });
}
