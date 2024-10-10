#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"

// Contains .NET - related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";
string configuration = GetBuildVariable("configuration", GetBuildVariable("BUILD_CONFIGURATION", "DEBUG"));
var localDotnet = GetBuildVariable("workloads", "local") == "local";
if (!localDotnet)
{
    dotnetPath = null;
}

var vsVersion = GetBuildVariable("VS", "");
string MSBuildExe = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE", ""));
string nugetSource = Argument("nugetsource", "");
string officialBuildId = Argument("officialbuildid", "");

string testFilter = Argument("test-filter", EnvironmentVariable("TEST_FILTER"));

var rootFolder = Context.Environment.WorkingDirectory;

if (rootFolder.FullPath.EndsWith("/devices", StringComparison.OrdinalIgnoreCase))
    rootFolder = rootFolder.Combine("../../").Collapse();

var arcadeBin = MakeAbsolute(new DirectoryPath("./artifacts/bin/"));

string TestTFM = Argument("testtfm", "");
var useNuget = Argument("usenuget", true);
if (TestTFM == "default")
    TestTFM = "";

Exception pendingException = null;

var NuGetOnlyPackages = new string[] {
    "Microsoft.Maui.Controls.*.{nupkg,snupkg}",
    "Microsoft.Maui.Core.*.{nupkg,snupkg}",
    "Microsoft.Maui.Essentials.*.{nupkg,snupkg}",
    "Microsoft.Maui.Graphics.*.{nupkg,snupkg}",
    "Microsoft.Maui.Maps.*.{nupkg,snupkg}",
    "Microsoft.Maui.Resizetizer.*.{nupkg,snupkg}",
    "Microsoft.AspNetCore.Components.WebView.*.{nupkg,snupkg}",
};
public enum RuntimeVariant
{
	Mono,
	NativeAOT
}

RuntimeVariant RUNTIME_VARIANT = Argument("runtimevariant", RuntimeVariant.Mono);
bool USE_NATIVE_AOT = RUNTIME_VARIANT == RuntimeVariant.NativeAOT ? true : false;

ProcessTFMSwitches();

// Tasks for CI

Task("dotnet")
    .Description("Provisions the .NET SDK into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        if (!localDotnet) 
            return;

        //We are passing a nuget folder with nuget locations
        if(!string.IsNullOrEmpty(nugetSource))
        {
            EnsureDirectoryExists(nugetSource);
            var originalNuget = File($"{rootFolder}/NuGet.config");
            ReplaceTextInFiles(originalNuget, "<add key=\"nuget-only\" value=\"true\" />", "");
            ReplaceTextInFiles(originalNuget, "NUGET_ONLY_PLACEHOLDER", nugetSource);
        }

        DotNetBuild($"{rootFolder}/src/DotNet/DotNet.csproj", new DotNetBuildSettings
        {
            MSBuildSettings = new DotNetMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-{configuration}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog")
                .SetConfiguration(configuration),
        });

        DotNetTool("tool",  new DotNetToolSettings {
		    ToolPath = dotnetPath,
		    DiagnosticOutput = true,	
		    ArgumentCustomization = args => args.Append("restore")
	    });
    });

Task("dotnet-local-workloads")
    .Does(() =>
    {
        if (!localDotnet) 
            return;
        
        DotNetBuild("./src/DotNet/DotNet.csproj", new DotNetBuildSettings
        {
            MSBuildSettings = new DotNetMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-{configuration}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog")
                .SetConfiguration(configuration)
                .WithProperty("InstallWorkloadPacks", "false"),
        });

        DotNetBuild("./src/DotNet/DotNet.csproj", new DotNetBuildSettings
        {
            MSBuildSettings = new DotNetMSBuildSettings()
                .EnableBinaryLogger($"{GetLogDirectory()}/dotnet-install-{configuration}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog")
                .SetConfiguration(configuration)
                .WithTarget("Install"),
            ToolPath = dotnetPath,
        });

        DotNetTool("tool",  new DotNetToolSettings {
		    ToolPath = dotnetPath,
		    DiagnosticOutput = true,	
		    ArgumentCustomization = args => args.Append("restore")
	    });
    });

Task("dotnet-buildtasks")
    .WithCriteria(Argument<string>("sln", null) == null)
    .IsDependentOn("dotnet")
    .Does(() =>
    {
        RunMSBuildWithDotNet($"{rootFolder}/Microsoft.Maui.BuildTasks.slnf");
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

Task("android-aar")
    .Does(() =>
    {
        var root = "./src/Core/AndroidNative/";

        var gradlew = root + "gradlew";
        if (IsRunningOnWindows())
            gradlew += ".bat";

        var exitCode = StartProcess(
            MakeAbsolute((FilePath)gradlew),
            new ProcessSettings
            {
                Arguments = $"createAar --rerun-tasks",
                WorkingDirectory = root
            });

        if (exitCode != 0)
        {
            if (IsCIBuild() || IsTarget("android-aar"))
                throw new Exception("Gradle failed to build maui.aar: " + exitCode);
            else
                Information("This task failing locally will not break local MAUI development. Gradle failed to build maui.aar: {0}", exitCode);
        }
    });

Task("dotnet-build")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet-buildtasks")
    .IsDependentOn("android-aar")
    .Description("Build the solutions")
    .Does(() =>
    {
        if (IsRunningOnWindows())
        {
            RunMSBuildWithDotNet("./Microsoft.Maui.sln");
        }
        else
        {
            RunMSBuildWithDotNet("./Microsoft.Maui-mac.slnf");
        }
    });

Task("dotnet-samples")
    .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        var tempDir = PrepareSeparateBuildContext("samplesTest");

        var properties = new Dictionary<string, string>();

        if(useNuget)
        {
            properties = new Dictionary<string, string> {
                ["UseWorkload"] = "true",
                // ["GenerateAppxPackageOnBuild"] = "true",
                ["RestoreConfigFile"] = tempDir.CombineWithFilePath("NuGet.config").FullPath,
            };
        }

        string projectsToBuild;
        if (USE_NATIVE_AOT)
        {
            if (configuration.Equals("Debug", StringComparison.OrdinalIgnoreCase))
            {
                var errMsg = $"Error: Building dotnet-samples with NativeAOT is only supported in Release configuration";
                Error(errMsg);
                throw new Exception(errMsg);
            }
            projectsToBuild = "./src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj";
            properties["_UseNativeAot"] = "true";
            properties["RuntimeIdentifier"] = "iossimulator-x64";
            properties["BuildIpa"] = "true";
        }
        else
        {
            projectsToBuild = "./Microsoft.Maui.Samples.slnf";
        }

        RunMSBuildWithDotNet(projectsToBuild, properties, binlogPrefix: "sample-");
    });

// Builds the host app for the UI Tests
Task("uitests-apphost")
    .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        var tempDir = PrepareSeparateBuildContext("samplesTest");

        var properties = new Dictionary<string, string>();

        if(useNuget)
        {
            properties = new Dictionary<string, string> {
                ["UseWorkload"] = "true",
                // ["GenerateAppxPackageOnBuild"] = "true",
                ["RestoreConfigFile"] = tempDir.CombineWithFilePath("NuGet.config").FullPath,
            };
        }
        RunMSBuildWithDotNet("./src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj", properties, binlogPrefix: "uitests-apphost-");
    });

Task("dotnet-legacy-controlgallery")
    .IsDependentOn("dotnet-legacy-controlgallery-android")
    .IsDependentOn("dotnet-legacy-controlgallery-ios");

Task("dotnet-legacy-controlgallery-ios")
    .Does(() =>
    {
        var properties = new Dictionary<string, string>();
        properties.Add("RuntimeIdentifier","iossimulator-x64");
        RunMSBuildWithDotNet("./src/Compatibility/ControlGallery/src/iOS/Compatibility.ControlGallery.iOS.csproj", properties, binlogPrefix: "controlgallery-ios-");
    });

Task("dotnet-legacy-controlgallery-android")
    .Does(() =>
    {
        var properties = new Dictionary<string, string>();
        RunMSBuildWithDotNet("./src/Compatibility/ControlGallery/src/Android/Compatibility.ControlGallery.Android.csproj", properties, binlogPrefix: "controlgallery-android-");
    });

Task("dotnet-integration-build")
    .Does(() =>
    {
        var properties = new Dictionary<string, string>();
        RunMSBuildWithDotNet("./src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj", properties, binlogPrefix: "integration-");
    });

Task("dotnet-integration-test")
    .IsDependentOn("dotnet-integration-build")
    .Does(() =>
    {
        RunTestWithLocalDotNet(
            "./src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj",
            config: configuration,
            pathDotnet: dotnetPath,
            noBuild: true,
            resultsFileNameWithoutExtension: Argument("resultsfilename", ""),
            filter: Argument("filter", ""));
    });

Task("dotnet-test")
    .IsDependentOn("dotnet")
    .Description("Build the solutions")
    .Does(() =>
    {
        var tests = new []
        {
            "**/Controls.Core.UnitTests.csproj",
        //    "**/Controls.Core.Design.UnitTests.csproj",
            "**/Controls.Xaml.UnitTests.csproj",
            "**/SourceGen.UnitTests.csproj",
            "**/Controls.BindingSourceGen.UnitTests.csproj",
            "**/Core.UnitTests.csproj",
            "**/Essentials.UnitTests.csproj",
            "**/Resizetizer.UnitTests.csproj",
            "**/Graphics.Tests.csproj",
            "**/Compatibility.Core.UnitTests.csproj",
        };

        var success = true;

        foreach (var test in tests)
        {
            if (!IsRunningOnWindows() && (test.Contains("Compatibility.Core.UnitTests") || test.Contains("Controls.Core.Design.UnitTests"))) 
            {
                continue;
            }
            foreach (var project in GetFiles(test))
            {
                try
                {
                    RunTestWithLocalDotNet(project.FullPath, configuration, dotnetPath);
                }
                catch
                {
                    success = false;
                    Error($"Test project failed: {project}");
                }
            }
        }

        if (!success)
            throw new Exception("Some tests failed. Check the logs or test results.");
    });

Task("dotnet-pack-maui")
    .IsDependentOn("android-aar")
    .WithCriteria(RunPackTarget())
    .Does(() =>
    {
        // We are passing a nuget folder with nuget locations
        if (!string.IsNullOrEmpty(nugetSource))
        {
            EnsureDirectoryExists(nugetSource);
            var originalNuget = File("./NuGet.config");
            ReplaceTextInFiles(originalNuget, "<add key=\"local\" value=\"true\" />", "");
            ReplaceTextInFiles(originalNuget, "LOCAL_PLACEHOLDER", nugetSource);
        }

        var sln = "./Microsoft.Maui.Packages.slnf";
        if (!IsRunningOnWindows())
            sln = "./Microsoft.Maui.Packages-mac.slnf";
 
        if(string.IsNullOrEmpty(officialBuildId))
        {
            officialBuildId = DateTime.UtcNow.ToString("yyyyMMdd.1");
        }

        RunMSBuildWithDotNet(sln, target: "Pack", properties: new Dictionary<string, string>
        {
            { "SymbolPackageFormat", "snupkg" },
            { "OfficialBuildId", officialBuildId },
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
            Source = new[] { "https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp/nuget/v3/index.json" },
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

        // Extract the binaries, xml & pdb files for docs purposes
        foreach (var pattern in NuGetOnlyPackages)
        {
            foreach (var nupkg in GetFiles($"./artifacts/**/{pattern}"))
            {
                var filename = nupkg.GetFilename().ToString();
                var d = $"{tempDir}/{filename}";
                Unzip(nupkg, d);
                DeleteFiles($"{d}/**/*.pri");
                DeleteFiles($"{d}/**/*.aar");
                DeleteFiles($"{d}/**/*.DesignTools.*");
                DeleteFiles($"{d}/**/*.resources.dll");

                if (filename.StartsWith("Microsoft.AspNetCore.Components.WebView.Wpf")
                    || filename.StartsWith("Microsoft.AspNetCore.Components.WebView.WindowsForms"))
                {
                    CopyFiles($"{d}/lib/**/net?.?-windows?.?/**/*.{{dll,xml,pdb}}", $"{destDir}");

                    continue;
                }

                CopyFiles($"{d}/lib/**/net?.?/**/*.{{dll,xml,pdb}}", $"{destDir}");
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
                DotNetTool("api-tools", new DotNetToolSettings
                {
                    DiagnosticOutput = true,
                    ArgumentCustomization = builder => builder
                        .Append("nuget-diff")
                        .AppendQuoted(nupkg.FullPath)
                        .Append("--latest")
                        // .Append("--verbose")
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

Task("VSCode")
    .Description("Provisions .NET, and launches an instance of Visual Studio Code using it.")
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

        UseLocalNuGetCacheFolder();

        StartVisualStudioCodeForDotNet();
    });

// Tasks for Local Development
Task("VS")
    .Description("Provisions .NET, and launches an instance of Visual Studio using it.")
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

        UseLocalNuGetCacheFolder();

        StartVisualStudioForDotNet();
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
    if (HasArgument("pack") && Argument<string>("pack", "true") != "false")
        return true;
        
    // If the request is to open a different sln then let's see if pack has ever run
    // if it hasn't then lets pack maui so the sln will open
    if (Argument<string>("sln", null) != null)
    {
        return Argument<string>("pack", "true") == "true";
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

void SetDotNetEnvironmentVariables(string dotnetDir = null)
{
    var dotnet = dotnetDir ?? MakeAbsolute(Directory("./bin/dotnet/")).ToString();
    
    SetEnvironmentVariable("VSDebugger_ValidateDotnetDebugLibSignatures", "0");
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

void UseLocalNuGetCacheFolder(bool reset = false)
{
    var temp = Context.Environment.GetSpecialPath(SpecialPath.LocalTemp);
    var packages = temp.Combine("Microsoft.Maui.Cache/NuGet/packages");

    EnsureDirectoryExists(packages);

    CleanDirectories(packages.FullPath + "/microsoft.maui.*");
    CleanDirectories(packages.FullPath + "/microsoft.aspnetcore.*");

    if (reset)
        CleanDirectories(packages.FullPath);

    SetEnvironmentVariable("RestorePackagesPath", packages.FullPath);
    SetEnvironmentVariable("NUGET_PACKAGES", packages.FullPath);
}

void StartVisualStudioCodeForDotNet()
{
    string workspace = "./maui.code-workspace";
    if (IsCIBuild())
    {
        Error("This target should not run on CI.");
        return;
    }

    if(localDotnet)
    {
        SetDotNetEnvironmentVariables();
    }

    StartProcess("code", new ProcessSettings{ Arguments = workspace, EnvironmentVariables = GetDotNetEnvironmentVariables() });
}

void StartVisualStudioForDotNet()
{
    string sln = Argument<string>("sln", null);

    bool includePrerelease = true;

    if (!String.IsNullOrEmpty(vsVersion))
        includePrerelease = (vsVersion == "preview");

    if (String.IsNullOrWhiteSpace(sln))
    {
        if (IsRunningOnWindows())
        {
            sln = "./Microsoft.Maui-windows.slnf";
        }
        else
        {
            sln = "./Microsoft.Maui-mac.slnf";
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
        $"\"{GetLogDirectory()}/{binlogPrefix}{name}-{configuration}-{target}-{type}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog\"" :
        $"\"{GetLogDirectory()}/{binlogPrefix}{name}-{configuration}-{target}-{targetFramework}-{type}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog\"";
    
    if(localDotnet)
        SetDotNetEnvironmentVariables();

    var msbuildSettings = new DotNetMSBuildSettings()
        .SetConfiguration(configuration)
        .SetMaxCpuCount(maxCpuCount)
        .WithTarget(target)
        .EnableBinaryLogger(binlog)
        
       // .SetVerbosity(Verbosity.Diagnostic)
        ;

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

    var dotnetBuildSettings = new DotNetBuildSettings
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

    DotNetBuild(sln, dotnetBuildSettings);
}

void RunTestWithLocalDotNet(string csproj, string config, string pathDotnet = null, Dictionary<string,string> argsExtra = null, bool noBuild = false, string resultsFileNameWithoutExtension = null, string filter = "", int maxCpuCount = 0)
{
    if (string.IsNullOrWhiteSpace(filter))
    {
        filter = testFilter;
    }

    if (!string.IsNullOrWhiteSpace(filter))
    {
        Information("Run Tests With Filter {0}", filter);	
    }

    string binlog;
    string results;
    var name = System.IO.Path.GetFileNameWithoutExtension(csproj);
    var logDirectory = GetLogDirectory();
    Information("Log Directory: {0}", logDirectory);

    if (!string.IsNullOrWhiteSpace(pathDotnet) && localDotnet)
    {
        // Make sure the path doesn't refer to the dotnet executable and make path absolute
        var localDotnetRoot = MakeAbsolute(Directory(System.IO.Path.GetDirectoryName(pathDotnet)));
    	Information("new dotnet root: {0}", localDotnetRoot);

        SetDotNetEnvironmentVariables(localDotnetRoot.FullPath);
    }

    if (string.IsNullOrWhiteSpace(resultsFileNameWithoutExtension))
    {
        binlog = $"{logDirectory}/{name}-{config}.binlog";
        results = $"{name}-{config}.trx";   
    }
    else
    {
        binlog = $"{logDirectory}/{resultsFileNameWithoutExtension}.binlog";
        results = $"{resultsFileNameWithoutExtension}.trx";
    }

    Information("Run Test binlog: {0}", binlog);

    var settings = new DotNetTestSettings
        {
            Configuration = config,
            NoBuild = noBuild,
            Filter = filter,
            Loggers = { 
                $"trx;LogFileName={results}",
                $"console;verbosity=normal"
            }, 
           	ResultsDirectory = GetTestResultsDirectory(),
        //    Verbosity = Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Diagnostic,
            ArgumentCustomization = args => 
            { 
                args.Append($"-bl:{binlog}");
                if(maxCpuCount > 0)
                {
                    args.Append($"-maxcpucount:{maxCpuCount}");
                }

                if(argsExtra != null)
                {
                    foreach(var prop in argsExtra)
                    {
                        args.Append($"/p:{prop.Key}={prop.Value}");
                    }        
                }        
                    
                // https://github.com/microsoft/vstest/issues/5112
                args.Append($"/p:VStestUseMSBuildOutput=false");
                
                return args;
            }
        };
    
    if(!string.IsNullOrEmpty(pathDotnet))
    {
        settings.ToolPath = pathDotnet;
    }

    try {
        DotNetTest(csproj, settings);
    } finally {
        Information("Test Run complete: {0}", results);
    }
}

DirectoryPath PrepareSeparateBuildContext(string dirName, string props = null, string targets = null)
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
    ReplaceTextInFiles(config.FullPath, "<add key=\"nuget-only\" value=\"true\" />", "");
    ReplaceTextInFiles(config.FullPath, "NUGET_ONLY_PLACEHOLDER", nugetOnly.FullPath);

    // Create empty or copy test Directory.Build.props/targets
    if (string.IsNullOrEmpty(props))
        FileWriteText(dir.CombineWithFilePath("Directory.Build.props"), "<Project/>");
    else
        CopyFile(props, dir.CombineWithFilePath("Directory.Build.props"));
    if (string.IsNullOrEmpty(targets))
        FileWriteText(dir.CombineWithFilePath("Directory.Build.targets"), "<Project/>");
    else
        CopyFile(targets, dir.CombineWithFilePath("Directory.Build.targets"));

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
