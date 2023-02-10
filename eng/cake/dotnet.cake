// Contains .NET 6-related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";
string configuration = GetBuildVariable("configuration", GetBuildVariable("BUILD_CONFIGURATION", "DEBUG"));
var localDotnet = GetBuildVariable("workloads", "local") == "local";
var vsVersion = GetBuildVariable("VS", "");
string MSBuildExe = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE", ""));
string nugetSource = Argument("nugetsource", "");

string TestTFM = Argument("testtfm", "");
if (TestTFM == "default")
    TestTFM = "";

Exception pendingException = null;

var NuGetOnlyPackages = new string[] {
    "Microsoft.Maui.Controls.*.nupkg",
    "Microsoft.Maui.Core.*.nupkg",
    "Microsoft.Maui.Essentials.*.nupkg",
    "Microsoft.Maui.Graphics.*.nupkg",
    "Microsoft.Maui.Maps.*.nupkg",
    "Microsoft.AspNetCore.Components.WebView.*.nupkg",
};

ProcessTFMSwitches();

// Tasks for CI

Task("dotnet")
    .Description("Provisions .NET 6 into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        if (!localDotnet) 
            return;

        //We are passing a nuget folder with nuget locations
        if(!string.IsNullOrEmpty(nugetSource))
        {
            EnsureDirectoryExists(nugetSource);
            var originalNuget = File("./NuGet.config");
            ReplaceTextInFiles(
                originalNuget,
                 $"<!-- <add key=\"local\" value=\"artifacts\" /> -->",
                $"<add key=\"nuget-only\" value=\"{nugetSource}\" />");
        }

        DotNetCoreBuild("./src/DotNet/DotNet.csproj", new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-{configuration}.binlog")
                .SetConfiguration(configuration),
        });
    });

Task("dotnet-local-workloads")
    .Does(() =>
    {
        if (!localDotnet) 
            return;
        
        DotNetCoreBuild("./src/DotNet/DotNet.csproj", new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-{configuration}.binlog")
                .SetConfiguration(configuration)
                .WithProperty("InstallWorkloadPacks", "false"),
        });

        DotNetCoreBuild("./src/DotNet/DotNet.csproj", new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-install-{configuration}.binlog")
                .SetConfiguration(configuration)
                .WithTarget("Install"),
            ToolPath = dotnetPath,
        });
    });

Task("dotnet-buildtasks")
    .WithCriteria(Argument<string>("sln", null) == null)
    .IsDependentOn("dotnet")
    .Does(() =>
    {
        RunMSBuildWithDotNet("./Microsoft.Maui.BuildTasks.slnf");
    })
   .OnError(exception =>
    {
        if (IsTarget("VS"))
        {
            pendingException = exception;
            return;
        }

        throw exception;
    });

Task("dotnet-build")
    .IsDependentOn("dotnet")
    .Description("Build the solutions")
    .Does(() =>
    {
        RunMSBuildWithDotNet("./Microsoft.Maui.BuildTasks.slnf");
        if (IsRunningOnWindows())
        {
            RunMSBuildWithDotNet("./Microsoft.Maui.sln", maxCpuCount: 1);
        }
        else
        {
            // NOTE: intentionally omit maxCpuCount, to avoid an issue with the 7.0.100 .NET SDK
            RunMSBuildWithDotNet("./Microsoft.Maui-mac.slnf");
        }
    });

Task("dotnet-samples")
    .Does(() =>
    {
        var tempDir = PrepareSeparateBuildContext("samplesTest", false);

        RunMSBuildWithDotNet("./Microsoft.Maui.Samples.slnf", new Dictionary<string, string> {
            ["UseWorkload"] = "true",
            // ["GenerateAppxPackageOnBuild"] = "true",
            ["RestoreConfigFile"] = tempDir.CombineWithFilePath("NuGet.config").FullPath,
        }, maxCpuCount: 1, binlogPrefix: "sample-");
    });

Task("dotnet-templates")
    .Does(() =>
    {
        if (localDotnet)
            SetDotNetEnvironmentVariables();

        var dn = localDotnet ? dotnetPath : "dotnet";

        var tempDir = PrepareSeparateBuildContext("templatesTest", true);

        // See: https://github.com/dotnet/project-system/blob/main/docs/design-time-builds.md
        var designTime = new Dictionary<string, string> {
            { "DesignTimeBuild", "true" },
            { "BuildingInsideVisualStudio", "true" },
            { "SkipCompilerExecution", "true" },
            // NOTE: this overrides a default setting that supports VS Mac
            // See: https://github.com/xamarin/xamarin-android/blob/94c2a3d86a2e0e74863b57e3c5c61dbd29daa9ea/src/Xamarin.Android.Build.Tasks/Xamarin.Android.Common.props.in#L19
            { "AndroidUseManagedDesignTimeResourceGenerator", "true" },
        };

        var properties = new Dictionary<string, string> {
            // Properties that ensure we don't use cached packages, and *only* the empty NuGet.config
            { "RestoreNoCache", "true" },
            // { "GenerateAppxPackageOnBuild", "true" },
            { "RestorePackagesPath", tempDir.Combine("packages").FullPath },
            { "RestoreConfigFile", tempDir.CombineWithFilePath("NuGet.config").FullPath },

            // Avoid iOS build warning as error on Windows: There is no available connection to the Mac. Task 'VerifyXcodeVersion' will not be executed
            { "CustomBeforeMicrosoftCSharpTargets", MakeAbsolute(File("./src/Templates/TemplateTestExtraTargets.targets")).FullPath },
            //Try not restore dependecies of 6.0.10
            { "DisableTransitiveFrameworkReferenceDownloads",  "true" },
        };

        var templates = new Dictionary<string, Action<DirectoryPath>> {
            { "maui:maui", null },
            { "mauiblazor:maui-blazor", null },
            { "mauilib:mauilib", null },
            { "mauicorelib:mauilib", dir => {
                CleanDirectories(dir.Combine("Platforms").FullPath);
                ReplaceTextInFiles($"{dir}/*.csproj", "UseMaui", "UseMauiCore");
                ReplaceTextInFiles($"{dir}/*.csproj", "SingleProject", "EnablePreviewMsixTooling");
            } },
            { "mauiunpackaged:maui", dir => {
                ReplaceTextInFiles($"{dir}/*.csproj", "<UseMaui>true</UseMaui>", "<UseMaui>true</UseMaui><WindowsPackageType>None</WindowsPackageType>");
            } },
            { "mauiblazorunpackaged:maui-blazor", dir => {
                ReplaceTextInFiles($"{dir}/*.csproj", "<UseMaui>true</UseMaui>", "<UseMaui>true</UseMaui><WindowsPackageType>None</WindowsPackageType>");
            } },
        };

        var alsoPack = new [] {
            "mauilib"
        };

        foreach (var template in templates)
        {
            foreach (var forceDotNetBuild in new [] { true, false })
            {
                // macOS does not support msbuild
                if (!IsRunningOnWindows() && !forceDotNetBuild)
                    continue;

                var type = forceDotNetBuild ? "DotNet" : "MSBuild";
                var projectName = template.Key.Split(":")[0];
                var templateName = template.Key.Split(":")[1];

                var framework = string.IsNullOrWhiteSpace(TestTFM) ? "" : $"--framework {TestTFM}";

                projectName = $"{tempDir}/{projectName}_{type}";
                projectName += string.IsNullOrWhiteSpace(TestTFM) ? "" : $"_{TestTFM.Replace('.', '_')}";

                // Create
                StartProcess(dn, $"new {templateName} -o \"{projectName}\" {framework}");

                // Modify
                if (template.Value != null)
                    template.Value(projectName);

                // Enable Tizen
                ReplaceTextInFiles($"{projectName}/*.csproj",
                    "<!-- <TargetFrameworks>",
                    "<TargetFrameworks>");
                ReplaceTextInFiles($"{projectName}/*.csproj",
                    "</TargetFrameworks> -->",
                    "</TargetFrameworks>");

                // Build
                RunMSBuildWithDotNet(projectName, properties, warningsAsError: true, forceDotNetBuild: forceDotNetBuild, binlogPrefix: "template-");

                // Pack
                if (alsoPack.Contains(templateName)) {
                    var packProperties = new Dictionary<string, string>(properties);
                    packProperties["PackageVersion"] = FileReadText("GitInfo.txt").Trim();
                    RunMSBuildWithDotNet(projectName, packProperties, warningsAsError: true, forceDotNetBuild: forceDotNetBuild, target: "Pack", binlogPrefix: "template-");
                }
            }
        }

        try
        {
            CleanDirectories(tempDir.FullPath);
        }
        catch
        {
            Information("Unable to clean up templates directory.");
        }
    });

Task("dotnet-test")
    .IsDependentOn("dotnet")
    .Description("Build the solutions")
    .Does(() =>
    {
        var tests = new []
        {
            "**/Controls.Core.UnitTests.csproj",
            "**/Controls.Core.Design.UnitTests.csproj",
            "**/Controls.Xaml.UnitTests.csproj",
            "**/Core.UnitTests.csproj",
            "**/Essentials.UnitTests.csproj",
            "**/Resizetizer.UnitTests.csproj",
            "**/Graphics.Tests.csproj",
        };

        var success = true;

        foreach (var test in tests)
        {
            foreach (var project in GetFiles(test))
            {
                try
                {
                    RunTestWithLocalDotNet(project.FullPath);
                }
                catch
                {
                    success = false;
                }
            }
        }

        if (!success)
            throw new Exception("Some tests failed. Check the logs or test results.");
    });

Task("dotnet-pack-maui")
    .WithCriteria(RunPackTarget())
    .Does(() =>
    {
        //We are passing a nuget folder with nuget locations
        if(!string.IsNullOrEmpty(nugetSource))
        {
            EnsureDirectoryExists(nugetSource);
            var originalNuget = File("./NuGet.config");
            ReplaceTextInFiles(
                originalNuget,
                 $"<!-- <add key=\"local\" value=\"artifacts\" /> -->",
                $"<add key=\"local\" value=\"{nugetSource}\" />");
        }
        DotNetCoreTool("pwsh", new DotNetCoreToolSettings
        {
            DiagnosticOutput = true,
            ArgumentCustomization = args => args.Append($"-NoProfile ./eng/package.ps1 -configuration \"{configuration}\"")
        });
    });

Task("dotnet-pack-additional")
    .WithCriteria(RunPackTarget())
    .Does(() =>
    {
        // Download some additional symbols that need to be archived along with the maui symbols:
        //  - _NativeAssets.windows
        //     - libSkiaSharp.pdb
        //     - libHarfBuzzSharp.pdb
        var assetsDir = $"./artifacts/additional-assets";
        var nativeAssetsVersion = XmlPeek("./eng/Versions.props", "/Project/PropertyGroup/_SkiaSharpNativeAssetsVersion");
        NuGetInstall("_NativeAssets.windows", new NuGetInstallSettings
        {
            Version = nativeAssetsVersion,
            ExcludeVersion = true,
            OutputDirectory = assetsDir,
            Source = new[] { "https://aka.ms/skiasharp-eap/index.json" },
        });
        foreach (var nupkg in GetFiles($"{assetsDir}/**/*.nupkg"))
            DeleteFile(nupkg);
        Zip(assetsDir, $"{assetsDir}.zip");
    });

Task("dotnet-pack-library-packs")
    .WithCriteria(RunPackTarget())
    .Does(() =>
    {
        var tempDir = $"./artifacts/library-packs-temp";

        var destDir = $"./artifacts/library-packs";
        EnsureDirectoryExists(destDir);
        CleanDirectories(destDir);
    });

Task("dotnet-pack-docs")
    .WithCriteria(RunPackTarget())
    .Does(() =>
    {
        var tempDir = $"./artifacts/docs-packs-temp";
        EnsureDirectoryExists(tempDir);
        CleanDirectories(tempDir);

        var destDir = $"./artifacts/docs-packs";
        EnsureDirectoryExists(destDir);
        CleanDirectories(destDir);

        // Get the docs for .NET MAUI
        foreach (var nupkg in GetFiles("./artifacts/Microsoft.Maui.*.Ref.any.*.nupkg"))
        {
            var d = $"{tempDir}/{nupkg.GetFilename()}";

            Unzip(nupkg, d);
            DeleteFiles($"{d}/**/*.pri");
            DeleteFiles($"{d}/**/*.aar");
            DeleteFiles($"{d}/**/*.DesignTools.*");
            CopyFiles($"{d}/ref/**/net?.?/**/*.dll", $"{destDir}");
            CopyFiles($"{d}/ref/**/net?.?/**/*.xml", $"{destDir}");
        }

        // Get the docs for libraries separately distributed as NuGets
        foreach (var pattern in NuGetOnlyPackages)
        {
            foreach (var nupkg in GetFiles($"./artifacts/{pattern}"))
            {
                var filename = nupkg.GetFilename().ToString();
                var d = $"{tempDir}/{filename}";
                Unzip(nupkg, d);
                DeleteFiles($"{d}/**/*.pri");
                DeleteFiles($"{d}/**/*.aar");
                DeleteFiles($"{d}/**/*.pdb");

                if (filename.StartsWith("Microsoft.AspNetCore.Components.WebView.Wpf")
                    || filename.StartsWith("Microsoft.AspNetCore.Components.WebView.WindowsForms"))
                {
                    CopyFiles($"{d}/lib/**/net?.?-windows?.?/**/*.dll", $"{destDir}");
                    CopyFiles($"{d}/lib/**/net?.?-windows?.?/**/*.xml", $"{destDir}");    

                    continue;
                }

                CopyFiles($"{d}/lib/**/{{net,netstandard}}?.?/**/*.dll", $"{destDir}");
                CopyFiles($"{d}/lib/**/{{net,netstandard}}?.?/**/*.xml", $"{destDir}");
            }
        }

        CleanDirectories(tempDir);
    });

Task("dotnet-pack")
    .WithCriteria(RunPackTarget())
    .IsDependentOn("dotnet-pack-maui")
    .IsDependentOn("dotnet-pack-additional")
    .IsDependentOn("dotnet-pack-library-packs")
    .IsDependentOn("dotnet-pack-docs");

Task("dotnet-build-test")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet-buildtasks")
    .IsDependentOn("dotnet-build")
    .IsDependentOn("dotnet-test");

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
            var diffCacheDir = GetTempDirectory().Combine("diffCache");
            EnsureDirectoryExists(diffCacheDir);
            CleanDirectories(diffCacheDir.FullPath);
            EnsureDirectoryExists(GetDiffDirectory());
            CleanDirectories(GetDiffDirectory().FullPath);

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
                        .AppendSwitchQuoted("--output", GetDiffDirectory().FullPath)
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

            var diffs = GetFiles($"{GetDiffDirectory()}/**/*.md");
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

// Tasks for Local Development
Task("VS")
    .Description("Provisions .NET 6, and launches an instance of Visual Studio using it.")
    .IsDependentOn("Clean")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet-buildtasks")
    .IsDependentOn("dotnet-pack") // Run conditionally 
    .Does(() =>
    {
        if (pendingException != null)
        {
            Error($"{pendingException}");
            Error("!!!!BUILD TASKS FAILED: !!!!!");
        }

        StartVisualStudioForDotNet6();
    }); 

// Keeping this for users that are already using this.
Task("VS-NET6")
    .Description("Provisions .NET 6 and launches an instance of Visual Studio using it.")
    .IsDependentOn("Clean")
    .IsDependentOn("VS")
    .Does(() =>
    {
       Warning("!!!!Please switch to using the `VS` target.!!!!");
    });

bool RunPackTarget()
{
    // Is the user running the pack target explicitly?
    if (TargetStartsWith("dotnet-pack"))
        return true;

    // If the default target is running then let the pack target run
    if (IsTarget("Default"))
        return true;

    // Does the user want to run a pack as part of a different target?
    if (HasArgument("pack"))
        return true;
        
    // If the request is to open a different sln then let's see if pack has ever run
    // if it hasn't then lets pack maui so the sln will open
    if (Argument<string>("sln", null) != null)
    {
        var mauiPacks = MakeAbsolute(Directory("./bin/dotnet/packs/Microsoft.Maui.Sdk")).ToString();
        if (!DirectoryExists(mauiPacks))
            return true;
    }

    return false;
}

Dictionary<string, string> GetDotNetEnvironmentVariables()
{
    Dictionary<string, string> envVariables = new Dictionary<string, string>();
    var dotnet = MakeAbsolute(Directory("./bin/dotnet/")).ToString();

    envVariables.Add("DOTNET_INSTALL_DIR", dotnet);
    envVariables.Add("DOTNET_ROOT", dotnet);
    envVariables.Add("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet);
    envVariables.Add("DOTNET_MULTILEVEL_LOOKUP", "0");
    envVariables.Add("MSBuildEnableWorkloadResolver", "true");

    var existingPath = EnvironmentVariable("PATH");
    Information(dotnet + ":" + existingPath);
    envVariables.Add("PATH", dotnet + ":" + existingPath);

    // Get "full" .binlog in Project System Tools
    if (HasArgument("debug"))
        envVariables.Add("MSBuildDebugEngine", "1");

    return envVariables;

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

    // Get "full" .binlog in Project System Tools
    if (HasArgument("dbg"))
        SetEnvironmentVariable("MSBuildDebugEngine", "1");
}

void StartVisualStudioForDotNet6()
{
    string sln = Argument<string>("sln", null);

    bool includePrerelease = true;

    if (!String.IsNullOrEmpty(vsVersion))
        includePrerelease = (vsVersion == "preview");

    if (String.IsNullOrWhiteSpace(sln))
    {
        if (IsRunningOnWindows())
        {
            sln = "./Microsoft.Maui.sln";
        }
        else
        {
            sln = "_omnisharp.sln";
        }
    }

    if (IsCIBuild())
    {
        Error("This target should not run on CI.");
        return;
    }

    if(localDotnet)
    {
        SetDotNetEnvironmentVariables();
    }

    if (IsRunningOnWindows())
    {
        var vsLatest = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = includePrerelease, });
        if (vsLatest == null)
            throw new Exception("Unable to find Visual Studio!");
    
        StartProcess(vsLatest.CombineWithFilePath("./Common7/IDE/devenv.exe"), sln);
    }
    else
    {
       
        StartProcess("open", new ProcessSettings{ Arguments = sln, EnvironmentVariables = GetDotNetEnvironmentVariables() });
    }
}

// NOTE: These methods work as long as the "dotnet" target has already run

void RunMSBuildWithDotNet(
    string sln,
    Dictionary<string, string> properties = null,
    string target = "Build",
    bool warningsAsError = false,
    bool restore = true,
    string targetFramework = null,
    bool forceDotNetBuild = false,
    int maxCpuCount = 0,
    string binlogPrefix = null)
{
    var useDotNetBuild = forceDotNetBuild || !IsRunningOnWindows() || target == "Run";

    var name = System.IO.Path.GetFileNameWithoutExtension(sln);
    var type = useDotNetBuild ? "dotnet" : "msbuild";
    var binlog = string.IsNullOrEmpty(targetFramework) ?
        $"\"{GetLogDirectory()}/{binlogPrefix}{name}-{configuration}-{target}-{type}.binlog\"" :
        $"\"{GetLogDirectory()}/{binlogPrefix}{name}-{configuration}-{target}-{targetFramework}-{type}.binlog\"";
    
    if(localDotnet)
        SetDotNetEnvironmentVariables();

    var msbuildSettings = new DotNetCoreMSBuildSettings()
        .SetConfiguration(configuration)
        .SetMaxCpuCount(maxCpuCount)
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

void RunTestWithLocalDotNet(string csproj)
{
    var name = System.IO.Path.GetFileNameWithoutExtension(csproj);
    var binlog = $"{GetLogDirectory()}/{name}-{configuration}.binlog";
    var results = $"{name}-{configuration}.trx";

    if(localDotnet)
        SetDotNetEnvironmentVariables();

    DotNetCoreTest(csproj,
        new DotNetCoreTestSettings
        {
            Configuration = configuration,
            ToolPath = dotnetPath,
            NoBuild = true,
            Loggers = {
                $"trx;LogFileName={results}"
            },
            ResultsDirectory = GetTestResultsDirectory(),
            ArgumentCustomization = args => args.Append($"-bl:{binlog}")
        });
}

DirectoryPath PrepareSeparateBuildContext(string dirName, bool generateDirectoryProps = false)
{
    var dir = GetTempDirectory().Combine(dirName);
    EnsureDirectoryExists(dir);
    CleanDirectories(dir.FullPath);

    var nugetOnly = dir.Combine("nuget-only");
    EnsureDirectoryExists(nugetOnly);
    CleanDirectories(nugetOnly.FullPath);

    CopyFileToDirectory(File("./NuGet.config"), dir);
    var config = dir.CombineWithFilePath("NuGet.config");

    foreach (var pattern in NuGetOnlyPackages)
    {
        CopyFiles($"./artifacts/{pattern}", nugetOnly, false);
    }

    // Add a specific folder for nuget-only packages
    ReplaceTextInFiles(
        config.FullPath,
        $"<!-- <add key=\"local\" value=\"artifacts\" /> -->",
        $"<add key=\"nuget-only\" value=\"{nugetOnly.FullPath}\" />");

    // Create empty Directory.Build.props/targets
    FileWriteText(dir.CombineWithFilePath("Directory.Build.props"), "<Project/>");
    FileWriteText(dir.CombineWithFilePath("Directory.Build.targets"), "<Project/>");

    return MakeAbsolute(dir);
}

void ProcessTFMSwitches()
{
    List<string> replaceTarget = new List<String>();

    if(HasArgument("android"))
        replaceTarget.Add("_IncludeAndroid");

    if(HasArgument("windows"))
        replaceTarget.Add("_IncludeWindows");

    if(HasArgument("ios"))
        replaceTarget.Add("_IncludeIos");

    if(HasArgument("catalyst") || HasArgument("maccatalyst"))
        replaceTarget.Add("_IncludeMacCatalyst");

    if(HasArgument("tizen"))
        replaceTarget.Add("_IncludeTizen");

    if (replaceTarget.Count > 0)
    {
        CopyFile("Directory.Build.Override.props.in", "Directory.Build.Override.props");
        foreach(var replaceWith in replaceTarget)
        {
            ReplaceTextInFiles("Directory.Build.Override.props", $"<{replaceWith}></{replaceWith}>", $"<{replaceWith}>true</{replaceWith}>");
        }
    }
    else
    {
        if (FileExists("Directory.Build.Override.props"))
            DeleteFile("Directory.Build.Override.props");
    }
}
