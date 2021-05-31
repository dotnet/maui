// Contains .NET 6-related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";

Task("dotnet")
    .Description("Provisions .NET 6 into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        var binlog = $"artifacts/dotnet-{configuration}.binlog";
        var settings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger(binlog)
                .SetConfiguration(configuration),
        };
        DotNetCoreBuild("./src/DotNet/DotNet.csproj", settings);
    });

Task("dotnet-pack")
    .Description("Build and create .NET 6 NuGet packages")
    //.IsDependentOn("dotnet")
   // .IsDependentOn("dotnet-buildtasks")
    .Does(()=>
    {

        var settings = new DotNetCoreToolSettings
        {
            DiagnosticOutput = true,
            ArgumentCustomization = args => args.Append($"./eng/package.ps1 -configuration \"{configuration}\"")
        };

        DotNetCoreTool("pwsh", settings);

        // RunMSBuildWithLocalDotNet("Microsoft.Maui-net6.sln", (settings) =>
        // {
        //     if (settings is MSBuildSettings msbuildSettings)
        //     {
        //         msbuildSettings
        //             .WithProperty("Packing", "true")
        //             .WithProperty("CI", "true")
        //             .WithTarget("build");

        //     }
        //     else if( settings is DotNetCoreMSBuildSettings dotnetSettings )
        //     {
        //         dotnetSettings
        //             .WithProperty("Packing", "true")
        //             .WithProperty("CI", "true")
        //             .WithTarget("pack");

        //     }
        // });


        // if (IsRunningOnWindows())
        // {        
        //     RunMSBuildWithLocalDotNet("Microsoft.Maui-net6.sln", (settings) =>
        //     {
        //         if (settings is MSBuildSettings msbuildSettings)
        //         {
        //             msbuildSettings
        //                 .WithProperty("Packing", "true")
        //                 .WithProperty("CI", "true")
        //                 .WithTarget("pack");

        //         }
        //     });
        // }
    });

Task("dotnet-buildtasks")
    .IsDependentOn("dotnet")
    .Does(() =>
    {
        RunMSBuildWithLocalDotNet("./Microsoft.Maui.BuildTasks-net6.sln");
    });

Task("VS-DOGFOOD")
    .Description("Provisions .NET 6 and launches an instance of Visual Studio using it.")
    .IsDependentOn("dotnet")
    .Does(() =>
    {
        StartVisualStudioForDotNet6(null);
    });

Task("VS-NET6")
    .Description("Provisions .NET 6 and launches an instance of Visual Studio using it.")
    .IsDependentOn("Clean")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        StartVisualStudioForDotNet6();
    });

Task("VS-WINUI-CI")
    .Description("Validates that WinUI can build with the cake scripts.")
    .IsDependentOn("Clean")
    .IsDependentOn("dotnet")
    .Does(() =>
    {
        RunMSBuildWithLocalDotNet("./Microsoft.Maui.BuildTasks-net6.sln", settings => ((MSBuildSettings)settings).WithProperty("BuildForWinUI", "true"));
        RunMSBuildWithLocalDotNet("./Microsoft.Maui.WinUI.sln");
    });

Task("VS-WINUI")
    .Description("Provisions .NET 6 and launches an instance of Visual Studio with WinUI projects.")
        .IsDependentOn("Clean")
    //  .IsDependentOn("dotnet") WINUI currently can't launch application with local dotnet 
    //  .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        string sln = "./Microsoft.Maui.WinUI.sln";
        var msbuildSettings = new MSBuildSettings
        {
            Configuration = configuration,
            ToolPath = FindMSBuild(),
            BinaryLogger = new MSBuildBinaryLogSettings
            {
                Enabled  = true,
                FileName = "artifacts/winui-buildtasks.binlog",
            }
        }.WithRestore().WithProperty("BuildForWinUI", "true");

	    MSBuild("./Microsoft.Maui.BuildTasks-net6.sln", msbuildSettings);

	    msbuildSettings = new MSBuildSettings
        {
            Configuration = configuration,
            ToolPath = FindMSBuild(),
            BinaryLogger = new MSBuildBinaryLogSettings
            {
                Enabled  = true,
                FileName = "artifacts/winui.binlog",
            }
        }.WithRestore();

        MSBuild(sln, msbuildSettings);

        var vsLatest = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = true, });
        if (vsLatest == null)
            throw new Exception("Unable to find Visual Studio!");
        
        StartProcess(vsLatest.CombineWithFilePath("./Common7/IDE/devenv.exe"), sln);
    });

Task("VS-ANDROID")
    .Description("Provisions .NET 6 and launches an instance of Visual Studio with Android projects.")
    .IsDependentOn("Clean")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet-buildtasks")
    .Does(() =>
    {
        DotNetCoreRestore("./Microsoft.Maui-net6.sln", new DotNetCoreRestoreSettings
        {
            ToolPath = dotnetPath
        });

        StartVisualStudioForDotNet6("./Microsoft.Maui.Droid.sln");
    });

string FindMSBuild()
{
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
    var target = EnvironmentVariableTarget.Process;
    Environment.SetEnvironmentVariable("DOTNET_INSTALL_DIR", dotnet, target);
    Environment.SetEnvironmentVariable("DOTNET_ROOT", dotnet, target);
    Environment.SetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet, target);
    Environment.SetEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0", target);
    Environment.SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "true", target);
    Environment.SetEnvironmentVariable("PATH", dotnet + System.IO.Path.PathSeparator + EnvironmentVariable("PATH"), target);
}

void StartVisualStudioForDotNet6(string sln = "./Microsoft.Maui-net6.sln")
{
    if (isCIBuild)
    {
        Information("This target should not run on CI.");
        return;
    }
    if (!IsRunningOnWindows())
    {
        Information("This target is only supported on Windows.");
        return;
    }

    var vsLatest = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = true, });
    if (vsLatest == null)
        throw new Exception("Unable to find Visual Studio!");
    SetDotNetEnvironmentVariables();
    Environment.SetEnvironmentVariable("_ExcludeMauiProjectCapability", "true", EnvironmentVariableTarget.Process);
    StartProcess(vsLatest.CombineWithFilePath("./Common7/IDE/devenv.exe"), sln);
}

// NOTE: this method works as long as the DotNet target has already run
void RunMSBuildWithLocalDotNet(string sln, Action<object> settings = null)
{
    var name = System.IO.Path.GetFileNameWithoutExtension(sln);
    var binlog = $"artifacts/{name}-{configuration}.binlog";

    SetDotNetEnvironmentVariables();

    // If we're not on Windows, use ./bin/dotnet/dotnet
    if (!IsRunningOnWindows())
    {
        var dotnetBuildSettings = new DotNetCoreMSBuildSettings
        {
            BinaryLogger = new MSBuildBinaryLoggerSettings
            {
                Enabled = true,
                FileName = binlog,
            },
        };

        settings?.Invoke(dotnetBuildSettings);
        
        DotNetCoreBuild(sln,
            new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                ToolPath = dotnetPath,
                MSBuildSettings = dotnetBuildSettings,
            });
        return;
    }

    // Otherwise we need to run MSBuild for WinUI support
    var msbuildSettings = new MSBuildSettings
        {
            Configuration = configuration,
            BinaryLogger = new MSBuildBinaryLogSettings
            {
                Enabled  = true,
                FileName = binlog,
            },
            ToolPath = FindMSBuild(),
        };

    settings?.Invoke(msbuildSettings);

    MSBuild(sln,
       msbuildSettings
       .WithRestore());
}
