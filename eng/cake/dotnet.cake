#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"

// Contains .NET - related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./.dotnet/dotnet{ext}";
string configuration = GetBuildVariable("configuration", GetBuildVariable("BUILD_CONFIGURATION", "DEBUG"));
var localDotnet = GetBuildVariable("workloads", "local") == "local";
var vsVersion = GetBuildVariable("VS", "");
string MSBuildExe = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE", ""));
string nugetSource = Argument("nugetsource", "");
string officialBuildId = Argument("officialbuildid", "");

string DefaultDotnetVersion = Argument("targetFrameworkVersion", EnvironmentVariable("TARGET_FRAMEWORK_VERSION") ?? "net10.0");

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
    NativeAOT,
    CoreCLR
}

RuntimeVariant RUNTIME_VARIANT = Argument("runtimevariant", RuntimeVariant.Mono);
bool USE_NATIVE_AOT = RUNTIME_VARIANT == RuntimeVariant.NativeAOT ? true : false;
bool USE_CORECLR = RUNTIME_VARIANT == RuntimeVariant.CoreCLR ? true : false;

ProcessTFMSwitches();

// Tasks for CI

Task("dotnet")
    .Description("Provisions the .NET SDK into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        if (!localDotnet)
            return;

        //We are passing a nuget folder with nuget locations
        if (!string.IsNullOrEmpty(nugetSource))
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

        DotNetTool("tool", new DotNetToolSettings
        {
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

        DotNetTool("tool", new DotNetToolSettings
        {
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
        var properties = new Dictionary<string, string>
        {
            ["BuildTaskOnlyBuild"] = "true"
        };
        RunMSBuildWithDotNet($"{rootFolder}/Microsoft.Maui.BuildTasks.slnf", properties);
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
    .IsDependentOn("dotnet-buildtasks")
    .Description("Build the solutions")
    .Does(() =>
    {
        if (IsRunningOnWindows())
        {
            RunMSBuildWithDotNet("./Microsoft.Maui.slnx");
        }
        else
        {
            // On macOS, for this type of build we don't need to ensure that the provisioning profile is required
            var properties = new Dictionary<string, string>();
            properties["CodesignRequireProvisioningProfile"] = "false";
            RunMSBuildWithDotNet("./Microsoft.Maui-mac.slnf", properties);
        }
    });

Task("dotnet-samples")
    .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        var tempDir = PrepareSeparateBuildContext("samplesTest");

        var properties = new Dictionary<string, string>();

        if (useNuget)
        {
            properties = new Dictionary<string, string>
            {
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
            projectsToBuild = "./eng/Microsoft.Maui.Samples.slnf";
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

        if (USE_CORECLR)
        {
            Information("Building for CoreCLR");
            properties.Add("UseMonoRuntime", "false");
            properties.Add("TargetFramework", $"{DefaultDotnetVersion}-android");
        }

        if (USE_NATIVE_AOT)
        {
            Information("Building for NativeAOT");
            properties.Add("_UseNativeAot", "true");
            properties.Add("RuntimeIdentifier", "iossimulator-x64");
        }

        if (useNuget)
        {
            properties.Add("UseWorkload", "true");
            // properties.Add("GenerateAppxPackageOnBuild", "true");
            // We are passing a nuget folder with nuget locations
            properties.Add("RestoreConfigFile", tempDir.CombineWithFilePath("NuGet.config").FullPath);
        }
        RunMSBuildWithDotNet("./src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj", properties, binlogPrefix: "uitests-apphost-");
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
        var tests = new[]
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

        var sln = "./eng/Microsoft.Maui.Packages.slnf";
        if (!IsRunningOnWindows())
        {
            sln = "./eng/Microsoft.Maui.Packages-mac.slnf";
        }

        if (string.IsNullOrEmpty(officialBuildId))
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
    .Does(async () =>
    {
        // Download some additional symbols that need to be archived along with the maui symbols:
        //  - _NativeAssets.windows
        //     - libSkiaSharp.pdb
        //     - libHarfBuzzSharp.pdb
        var assetsDir = $"./artifacts/additional-assets";
        var nativeAssetsVersion = XmlPeek("./eng/Versions.props", "/Project/PropertyGroup/_SkiaSharpNativeAssetsVersion");
        await DownloadNuGetPackageAsync(
            "_NativeAssets.windows",
            nativeAssetsVersion,
            assetsDir,
            "https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp/nuget/v3/index.json");
        Zip(assetsDir, $"{assetsDir}.zip");
        foreach (var nupkg in GetFiles($"{assetsDir}/**/*.nupkg"))
            DeleteFile(nupkg);
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
                    CopyFiles($"{d}/lib/**/net??.?-windows?.?/**/*.{{dll,xml,pdb}}", $"{destDir}");

                    continue;
                }

                CopyFiles($"{d}/lib/**/net??.?/**/*.{{dll,xml,pdb}}", $"{destDir}");
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

        StartVisualStudioCodeForDotNet(false);
    });

Task("Insiders")
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

        StartVisualStudioCodeForDotNet(true);
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


Task("GenerateCgManifest")
    .Description("Generates the cgmanifest.json file with versions from Versions.props")
    .Does(() =>
{
    Information("Generating cgmanifest.json from Versions.props");

    // Use pwsh on all platforms
    var pwshExecutable = "pwsh";

    // Check if pwsh is available
    try
    {
        if (IsRunningOnWindows())
        {
            var exitCode = StartProcess("where", new ProcessSettings
            {
                Arguments = "pwsh",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (exitCode != 0)
            {
                Information("pwsh not found, falling back to powershell");
                pwshExecutable = "powershell";
            }
        }
        else
        {
            var exitCode = StartProcess("which", new ProcessSettings
            {
                Arguments = "pwsh",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (exitCode != 0)
            {
                throw new Exception("PowerShell Core (pwsh) is not installed. Please install it to continue.");
            }
        }
    }
    catch (Exception ex) when (!IsRunningOnWindows())
    {
        Error("Error checking for pwsh: " + ex.Message);
        throw new Exception("PowerShell Core (pwsh) is required on non-Windows platforms. Please install it and try again.");
    }

    // Execute the PowerShell script
    StartProcess(pwshExecutable, new ProcessSettings
    {
        Arguments = "-NonInteractive -ExecutionPolicy Bypass -File ./eng/scripts/update-cgmanifest.ps1"
    });
});

Task("publicapi")
    .Description("Clears PublicAPI.Unshipped.txt files and regenerates them with current public APIs. Processes Core, Controls, Essentials, and Graphics projects. Skips Windows files on non-Windows platforms and always skips Tizen files. Use after adding new public APIs to resolve build errors.")
    .Does(() =>
{
    var corePublicApiDir = MakeAbsolute(Directory("./src/Core/src/PublicAPI"));
    var controlsPublicApiDir = MakeAbsolute(Directory("./src/Controls/src/Core/PublicAPI"));
    var essentialsPublicApiDir = MakeAbsolute(Directory("./src/Essentials/src/PublicAPI"));
    var graphicsPublicApiDir = MakeAbsolute(Directory("./src/Graphics/src/Graphics/PublicAPI"));

    Information("Resetting PublicAPI.Unshipped.txt files...");

    // Find and clear all PublicAPI.Unshipped.txt files in Core, Controls, Essentials, and Graphics
    var coreUnshippedFiles = GetFiles($"{corePublicApiDir}/**/PublicAPI.Unshipped.txt");
    var controlsUnshippedFiles = GetFiles($"{controlsPublicApiDir}/**/PublicAPI.Unshipped.txt");
    var essentialsUnshippedFiles = GetFiles($"{essentialsPublicApiDir}/**/PublicAPI.Unshipped.txt");
    var graphicsUnshippedFiles = GetFiles($"{graphicsPublicApiDir}/**/PublicAPI.Unshipped.txt");
    var allUnshippedFiles = coreUnshippedFiles.Concat(controlsUnshippedFiles).Concat(essentialsUnshippedFiles).Concat(graphicsUnshippedFiles);

    foreach (var file in allUnshippedFiles)
    {
        // Skip Windows-specific files if not on Windows
        if (!IsRunningOnWindows() && file.FullPath.Contains("windows"))
        {
            Information($"Skipping Windows file (not on Windows): {file}");
            continue;
        }

        // Skip Tizen-specific files
        if (file.FullPath.Contains("tizen"))
        {
            Information($"Skipping Tizen file: {file}");
            continue;
        }

        // Skip macOS-specific files
        if (file.FullPath.Contains("macos"))
        {
            Information($"Skipping macOS file: {file}");
            continue;
        }

        Information($"Clearing: {file}");
        System.IO.File.WriteAllText(file.FullPath, string.Empty);
    }

    Information("Regenerating PublicAPI...");

    // Build Controls.Core.csproj with PublicApiType=Generate
    var settings = new DotNetBuildSettings
    {
        Configuration = "Debug",
        MSBuildSettings = new DotNetMSBuildSettings()
    };
    settings.MSBuildSettings.Properties["PublicApiType"] = new List<string> { "Generate" };

    DotNetBuild("./src/Controls/src/Core/Controls.Core.csproj", settings);

    Information("PublicAPI reset and regeneration completed!");
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
    var dotnet = MakeAbsolute(Directory("./.dotnet/")).ToString();

    envVariables.Add("DOTNET_INSTALL_DIR", dotnet);
    envVariables.Add("DOTNET_ROOT", dotnet);
    envVariables.Add("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet);
    envVariables.Add("DOTNET_MULTILEVEL_LOOKUP", "0");
    envVariables.Add("DOTNET_SYSTEM_NET_SECURITY_NOREVOCATIONCHECKBYDEFAULT", "true");
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
    var dotnet = dotnetDir ?? MakeAbsolute(Directory("./.dotnet/")).ToString();
    var dotnetHostPath = IsRunningOnWindows() ? $"{dotnet}/dotnet.exe" : $"{dotnet}/dotnet";
  
    SetEnvironmentVariable("VSDebugger_ValidateDotnetDebugLibSignatures", "0");
    SetEnvironmentVariable("DOTNET_INSTALL_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_ROOT", dotnet);
    if (IsRunningOnWindows())
    { 
        //workaround for dev18 
        SetEnvironmentVariable("DOTNET_HOST_PATH", dotnetHostPath);
    }
    SetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0");
    SetEnvironmentVariable("DOTNET_SYSTEM_NET_SECURITY_NOREVOCATIONCHECKBYDEFAULT", "true");
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

void StartVisualStudioCodeForDotNet(bool useInsiders)
{
    if (IsCIBuild())
    {
        Error("This target should not run on CI.");
        return;
    }

    if (localDotnet)
    {
        SetDotNetEnvironmentVariables();
    }

    string codeProcessName = useInsiders ? "code-insiders" : "code";

    StartProcess(codeProcessName, new ProcessSettings { EnvironmentVariables = GetDotNetEnvironmentVariables() });
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

    if (localDotnet)
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

        StartProcess("open", new ProcessSettings { Arguments = sln, EnvironmentVariables = GetDotNetEnvironmentVariables() });
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

    if (localDotnet)
        SetDotNetEnvironmentVariables();

    var msbuildSettings = new DotNetMSBuildSettings()
        .SetConfiguration(configuration)
        .SetMaxCpuCount(maxCpuCount)
        .WithTarget(target)
        .EnableBinaryLogger(binlog)

        // .SetVerbosity(Verbosity.Diagnostic)
        ;

    var loggerArg = GetMSBuildForwardingLoggerPath();
    if (loggerArg != null)
    {
        msbuildSettings.WithArgumentCustomization(args => args.Append(loggerArg));
    }

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

void RunTestWithLocalDotNet(string csproj, string config, string pathDotnet = null, Dictionary<string, string> argsExtra = null, bool noBuild = false, string resultsFileNameWithoutExtension = null, string filter = "", int maxCpuCount = 0)
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
            var loggerArg = GetMSBuildForwardingLoggerPath();
            if (loggerArg != null)
            {
               args.Append(loggerArg);
            }

            args.Append($"-bl:{binlog}");
            if(maxCpuCount > 0)
            {
               args.Append($"-maxcpucount:{maxCpuCount}");
            }

            if (argsExtra != null)
            {
                foreach (var prop in argsExtra)
                {
                    args.Append($"/p:{prop.Key}={prop.Value}");
                }
            }

            // https://github.com/microsoft/vstest/issues/5112
            args.Append($"/p:VStestUseMSBuildOutput=false");

            return args;
        }
    };

    if (!string.IsNullOrEmpty(pathDotnet))
    {
        settings.ToolPath = pathDotnet;
    }

    try
    {
        DotNetTest(csproj, settings);
    }
    finally
    {
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

    if (HasArgument("android"))
        replaceTarget.Add("_IncludeAndroid");

    if (HasArgument("windows"))
        replaceTarget.Add("_IncludeWindows");

    if (HasArgument("ios"))
        replaceTarget.Add("_IncludeIos");

    if (HasArgument("catalyst") || HasArgument("maccatalyst"))
        replaceTarget.Add("_IncludeMacCatalyst");

    if (HasArgument("tizen"))
        replaceTarget.Add("_IncludeTizen");

    if (replaceTarget.Count > 0)
    {
        CopyFile("Directory.Build.Override.props.in", "Directory.Build.Override.props");
        foreach (var replaceWith in replaceTarget)
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

string GetMSBuildForwardingLoggerPath()
{
    if (!IsCIBuild())
        return null;

    // Download and extract MSBuild logger
    var loggerUrl = "https://vstsagenttools.blob.core.windows.net/tools/msbuildlogger/3/msbuildlogger.zip";
    var loggerDir = MakeAbsolute(Directory("./artifacts/msbuildlogger"));
    EnsureDirectoryExists(loggerDir);
    var loggerZip = loggerDir.CombineWithFilePath("msbuildlogger.zip");

    if (!FileExists(loggerZip))
    {
        DownloadFile(loggerUrl, loggerZip.FullPath);
        Unzip(loggerZip.FullPath, loggerDir.FullPath);
    }

    var loggerArg = $"-dl:CentralLogger,\"{loggerDir}/Microsoft.TeamFoundation.DistributedTask.MSBuild.Logger.dll\"*ForwardingLogger,\"{loggerDir}/Microsoft.TeamFoundation.DistributedTask.MSBuild.Logger.dll\"";

    return loggerArg;
}
