// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// examples
/*

Windows CMD:
build.cmd -Target NugetPack
build.cmd -Target NugetPack -ScriptArgs '-packageVersion="9.9.9-custom"','-configuration="Release"'

PowerShell:
./build.ps1 -Target NugetPack
./build.ps1 -Target NugetPack -ScriptArgs '-packageVersion="9.9.9-custom"'

 */
//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////
#addin "nuget:?package=Cake.Xamarin&version=3.0.2"
#addin "nuget:?package=Cake.Android.Adb&version=3.2.0"
#addin "nuget:?package=Cake.Git&version=0.21.0"
#addin "nuget:?package=Cake.Android.SdkManager&version=3.0.2"
#addin "nuget:?package=Cake.Boots&version=1.0.2.421"

#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"
//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var ANDROID_RENDERERS = Argument("ANDROID_RENDERERS", "FAST");
var XamarinFormsVersion = Argument("XamarinFormsVersion", "");
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var packageVersion = Argument("packageVersion", "");
var releaseChannelArg = Argument("CHANNEL", "Stable");
releaseChannelArg = EnvironmentVariable("CHANNEL") ?? releaseChannelArg;
var teamProject = Argument("TeamProject", "");

string artifactStagingDirectory = Argument("Build_ArtifactStagingDirectory", (string)null) ?? EnvironmentVariable("Build.ArtifactStagingDirectory") ?? EnvironmentVariable("Build_ArtifactStagingDirectory") ?? ".";
var ANDROID_HOME = EnvironmentVariable("ANDROID_HOME") ??
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");

string[] androidSdkManagerInstalls = new string[0]; //new [] { "platforms;android-24", "platforms;android-28", "platforms;android-29", "build-tools;29.0.3"};


Information ("XamarinFormsVersion: {0}", XamarinFormsVersion);
Information ("ANDROID_RENDERERS: {0}", ANDROID_RENDERERS);
Information ("configuration: {0}", configuration);
Information ("ANDROID_HOME: {0}", ANDROID_HOME);
Information ("Team Project: {0}", teamProject);

var releaseChannel = ReleaseChannel.Stable;
if(releaseChannelArg == "Preview")
{
    releaseChannel = ReleaseChannel.Preview;
}

Information ("Release Channel: {0}", releaseChannel);

string androidSDK_macos = "";
string monoSDK_macos = "";
string iOSSDK_macos = "";
string macSDK_macos = "";
string monoPatchVersion = "";
string monoMajorVersion = "";
string monoVersion = "";

if(releaseChannel == ReleaseChannel.Stable)
{
    if(IsXcodeVersionOver("11.4"))
    {
        // Xcode 11.4 just uses boots enums
        Information ("XCODE 11.4");
    }
    else
    {
        // Xcode 11.3
        monoMajorVersion = "";
        monoPatchVersion = "";
        androidSDK_macos = "https://download.visualstudio.microsoft.com/download/pr/8f94ca38-039a-4c9f-a51a-a6cb33c76a8c/aa46188c5f7a2e0c6f2d4bd4dc261604/xamarin.android-10.2.0.100.pkg";
        iOSSDK_macos = $"https://download.visualstudio.microsoft.com/download/pr/8f94ca38-039a-4c9f-a51a-a6cb33c76a8c/21e09d8084eb7c15eaa07c970e0eccdc/xamarin.ios-13.14.1.39.pkg";
        macSDK_macos = $"https://download.visualstudio.microsoft.com/download/pr/8f94ca38-039a-4c9f-a51a-a6cb33c76a8c/979144aead55378df75482d35957cdc9/xamarin.mac-6.14.1.39.pkg";
        monoSDK_macos = "https://download.visualstudio.microsoft.com/download/pr/8f94ca38-039a-4c9f-a51a-a6cb33c76a8c/3a376d8c817ec4d720ecca2d95ceb4c1/monoframework-mdk-6.8.0.123.macos10.xamarin.universal.pkg";

    }
}

if(String.IsNullOrWhiteSpace(monoSDK_macos))
{
    if(String.IsNullOrWhiteSpace(monoPatchVersion))
        monoVersion = $"{monoMajorVersion}";
    else
        monoVersion = $"{monoMajorVersion}.{monoPatchVersion}";

    if(!String.IsNullOrWhiteSpace(monoVersion))
    {
        monoSDK_macos = $"https://download.mono-project.com/archive/{monoMajorVersion}/macos-10-universal/MonoFramework-MDK-{monoVersion}.macos10.xamarin.universal.pkg";
    }
}

string androidSDK_windows = EnvironmentVariable("ANDROID_SDK_WINDOWS", "");
string iOSSDK_windows = EnvironmentVariable("IOS_SDK_WINDOWS", "");
string monoSDK_windows = EnvironmentVariable("MONO_SDK_WINDOWS", "");
string macSDK_windows = EnvironmentVariable("MAC_SDK_WINDOWS", "");

androidSDK_macos = EnvironmentVariable("ANDROID_SDK_MAC", androidSDK_macos);
iOSSDK_macos = EnvironmentVariable("IOS_SDK_MAC", iOSSDK_macos);
monoSDK_macos = EnvironmentVariable("MONO_SDK_MAC", monoSDK_macos);
macSDK_macos = EnvironmentVariable("MAC_SDK_MAC", macSDK_macos);

string androidSDK = IsRunningOnWindows() ? androidSDK_windows : androidSDK_macos;
string monoSDK = IsRunningOnWindows() ? monoSDK_windows : monoSDK_macos;
string iosSDK = IsRunningOnWindows() ? iOSSDK_windows : iOSSDK_macos;
string macSDK  = IsRunningOnWindows() ? macSDK_windows : macSDK_macos;


Information ("androidSDK: {0}", androidSDK);
Information ("monoSDK: {0}", monoSDK);
Information ("macSDK: {0}", macSDK);
Information ("iosSDK: {0}", iosSDK);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Deletes all the obj/bin directories")
    .Does(() =>
{
    CleanDirectories("./**/obj", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));
    CleanDirectories("./**/bin", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));
});

Task("provision-macsdk")
    .Description("Install Xamarin.Mac SDK")
    .Does(async () =>
    {
        if(!IsRunningOnWindows())
        {
            if(!String.IsNullOrWhiteSpace(macSDK))
                await Boots(macSDK);
            else
                await Boots (Product.XamarinMac, releaseChannel);
        }
        else if(!String.IsNullOrWhiteSpace(macSDK))
            await Boots(macSDK);
    });

Task("provision-iossdk")
    .Description("Install Xamarin.iOS SDK")
    .Does(async () =>
    {
        if (!IsRunningOnWindows ()) {
            if(!String.IsNullOrWhiteSpace(iosSDK))
                await Boots(iosSDK);
            else
                await Boots (Product.XamariniOS, releaseChannel);
        }
        else if(!String.IsNullOrWhiteSpace(iosSDK))
            await Boots(iosSDK);
    });

Task("provision-androidsdk")
    .Description("Install Xamarin.Android SDK")
    .Does(async () =>
    {
        Information ("ANDROID_HOME: {0}", ANDROID_HOME);

        if(androidSdkManagerInstalls.Length > 0)
        {
            var androidSdkSettings = new AndroidSdkManagerToolSettings {
                SkipVersionCheck = true
            };

            if(!String.IsNullOrWhiteSpace(ANDROID_HOME))            
                androidSdkSettings.SdkRoot = ANDROID_HOME;

            AcceptLicenses (androidSdkSettings);
            AndroidSdkManagerUpdateAll (androidSdkSettings);
            AcceptLicenses (androidSdkSettings);
            AndroidSdkManagerInstall (androidSdkManagerInstalls, androidSdkSettings);
        }

        if (!IsRunningOnWindows ()) {
            if(!String.IsNullOrWhiteSpace(androidSDK))
                await Boots (androidSDK);
            else
                await Boots (Product.XamarinAndroid, releaseChannel);
        }
        else if(!String.IsNullOrWhiteSpace(androidSDK))
            await Boots(androidSDK);
    });

Task("provision-monosdk")
    .Description("Install Mono SDK")
    .Does(async () =>
    {
        if(!IsRunningOnWindows())
        {
            if(!String.IsNullOrWhiteSpace(monoSDK))
                await Boots(monoSDK);
            else
                await Boots (Product.Mono, releaseChannel);
        }
        else if(!String.IsNullOrWhiteSpace(monoSDK))
            await Boots(monoSDK);
    });

Task("provision")
    .Description("Install SDKs required to build project")
    .IsDependentOn("provision-macsdk")
    .IsDependentOn("provision-iossdk")
    .IsDependentOn("provision-monosdk")
    .IsDependentOn("provision-androidsdk");

Task("NuGetPack")
    .Description("Build and Create Nugets")
    .IsDependentOn("Restore")
    .IsDependentOn("BuildForNuget")
    .IsDependentOn("_NuGetPack");


Task("_NuGetPack")
    .Description("Create Nugets without building anything")
    .Does(() =>
    {
        var nugetversion = String.Empty;

        if(!String.IsNullOrWhiteSpace(packageVersion))
        {
            nugetversion = packageVersion;
        }
        else
        {
            var nugetVersionFile = GetFiles(".XamarinFormsVersionFile.txt");
            nugetversion = FileReadText(nugetVersionFile.First());
        }

        Information("Nuget Version: {0}", nugetversion);

        var nugetPackageDir = Directory("./artifacts");
        var nuGetPackSettings = new NuGetPackSettings
        {
            OutputDirectory = nugetPackageDir,
            Version = nugetversion
        };

        var nugetFilePaths =
            GetFiles("./.nuspec/*.nuspec");

        nuGetPackSettings.Properties.Add("configuration", configuration);
        nuGetPackSettings.Properties.Add("platform", "anycpu");
        // nuGetPackSettings.Verbosity = NuGetVerbosity.Detailed;
        NuGetPack(nugetFilePaths, nuGetPackSettings);
    });


Task("Restore")
    .Description("Restore target on System.Maui.sln")
    .Does(() =>
    {
        try{
            MSBuild("./System.Maui.sln", GetMSBuildSettings().WithTarget("restore"));
        }
        catch{
            // ignore restore errors that come from uwp
            if(IsRunningOnWindows())
                throw;
        }
    });

Task("BuildForNuget")
    .Description("Builds all necessary projects to create Nuget Packages")
    .Does(() =>
{
    try{

        var msbuildSettings = GetMSBuildSettings();
        var binaryLogger = new MSBuildBinaryLogSettings {
            Enabled  = true
        };

        msbuildSettings.BinaryLogger = binaryLogger;
        msbuildSettings.ArgumentCustomization = args => args.Append("/nowarn:VSX1000");
        binaryLogger.FileName = $"{artifactStagingDirectory}/win-{configuration}.binlog";

        MSBuild("./System.Maui.sln", msbuildSettings);

        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/dualscreen-{configuration}-csproj.binlog";
        MSBuild("./System.Maui.DualScreen/System.Maui.DualScreen.csproj",
                    msbuildSettings
                        .WithRestore()
                        .WithTarget("rebuild"));


        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/win-{configuration}-csproj.binlog";
        MSBuild("./System.Maui.Platform.UAP/System.Maui.Platform.UAP.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("DisableEmbeddedXbf", "false")
                        .WithProperty("EnableTypeInfoReflection", "false"));

    }
    catch(Exception)
    {
        if(IsRunningOnWindows())
            throw;
    }
});

Task("BuildTasks")
    .Description("Build System.Maui.Build.Tasks/System.Maui.Build.Tasks.csproj")
    .Does(() =>
{
    MSBuild("./System.Maui.Build.Tasks/System.Maui.Build.Tasks.csproj", GetMSBuildSettings().WithRestore());
});

Task("Build")
    .Description("Builds all necessary projects to run Control Gallery")
    .IsDependentOn("Restore")
    .Does(() =>
{
    try{
        MSBuild("./System.Maui.sln", GetMSBuildSettings().WithRestore());
    }
    catch(Exception)
    {
        if(IsRunningOnWindows())
            throw;
    }
});

Task("Android100")
    .Description("Builds Monodroid10.0 targets")
    .Does(() =>
    {
            MSBuild("System.Maui.sln",
                    GetMSBuildSettings()
                        .WithRestore()
                        .WithProperty("AndroidTargetFrameworks", "MonoAndroid90;MonoAndroid10.0"));
    });

Task("VSMAC")
    .Description("Builds projects necessary so solution compiles on VSMAC")
    .Does(() =>
    {
        StartProcess("open", new ProcessSettings{ Arguments = "System.Maui.sln" });
    });

/*
Task("Deploy")
    .IsDependentOn("DeployiOS")
    .IsDependentOn("DeployAndroid");


// TODO? Not sure how to make this work
Task("DeployiOS")
    .Does(() =>
    {
        // not sure how to get this to deploy to iOS
        BuildiOSIpa("./System.Maui.sln", platform:"iPhoneSimulator", configuration:"Debug");

    });
*/
Task("DeployAndroid")
    .Description("Builds and deploy Android Control Gallery")
    .Does(() =>
    {
        MSBuild("./System.Maui.Build.Tasks/System.Maui.Build.Tasks.csproj", GetMSBuildSettings().WithRestore());
        MSBuild("./System.Maui.ControlGallery.Android/System.Maui.ControlGallery.Android.csproj", GetMSBuildSettings().WithRestore());
        BuildAndroidApk("./System.Maui.ControlGallery.Android/System.Maui.ControlGallery.Android.csproj", sign:true, configuration:configuration);
        AdbUninstall("AndroidControlGallery.AndroidControlGallery");
        AdbInstall("./System.Maui.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk");
        AmStartActivity("AndroidControlGallery.AndroidControlGallery/md546303760447087909496d02dc7b17ae8.Activity1");
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);


MSBuildSettings GetMSBuildSettings()
{
    var buildSettings =  new MSBuildSettings {
        PlatformTarget = PlatformTarget.MSIL,
        MSBuildPlatform = Cake.Common.Tools.MSBuild.MSBuildPlatform.x86,
        Configuration = configuration,
    };

    if(!String.IsNullOrWhiteSpace(XamarinFormsVersion))
    {
        buildSettings = buildSettings.WithProperty("XamarinFormsVersion", XamarinFormsVersion);
    }
    
    buildSettings.ArgumentCustomization = args => args.Append("/nowarn:VSX1000");
    return buildSettings;
}

bool IsXcodeVersionOver(string version)
{
    if(IsRunningOnWindows())
        return true;

    IEnumerable<string> redirectedStandardOutput;
    StartProcess("xcodebuild", 
        new ProcessSettings {
            Arguments = new ProcessArgumentBuilder().Append(@"-version"),
            RedirectStandardOutput = true
        },
         out redirectedStandardOutput
    );

    foreach (var item in redirectedStandardOutput)
    {
        if(item.Contains("Xcode"))
        {
            var xcodeVersion = Version.Parse(item.Replace("Xcode", ""));
            Information($"Xcode: {xcodeVersion}");
            return Version.Parse(item.Replace("Xcode", "")) >= Version.Parse(version); 
        }
    }

    return true;
}