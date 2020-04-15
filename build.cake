// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// examples
/*

Windows CMD:
build.cmd -Target NugetPack
build.cmd -Target NugetPack -ScriptArgs '-packageVersion="9.9.9-custom"'

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

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var packageVersion = Argument("packageVersion", "");
var releaseChannelArg = Argument("releaseChannel", "Stable");
releaseChannelArg = EnvironmentVariable("releaseChannel") ?? releaseChannelArg;
var teamProject = Argument("TeamProject", "");
bool buildForVS2017 = Convert.ToBoolean(Argument("buildForVS2017", "false"));

string artifactStagingDirectory = Argument("Build_ArtifactStagingDirectory", (string)null) ?? EnvironmentVariable("Build.ArtifactStagingDirectory") ?? EnvironmentVariable("Build_ArtifactStagingDirectory") ?? ".";
var ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ??
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");

string[] androidSdkManagerInstalls = new string[0];//new [] { "platforms;android-24", "platforms;android-28"};


Information ("Team Project: {0}", teamProject);
Information ("buildForVS2017: {0}", buildForVS2017);

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

if(buildForVS2017 || teamProject == "DevDiv")
{
    // VS2017
    monoMajorVersion = "5.18.1";
    monoPatchVersion = "";
    androidSDK_macos = "https://aka.ms/xamarin-android-commercial-d15-9-macos";
    iOSSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10.2/9c8d8e0a50e68d9abc8cd48fcd47a669e981fcc9/53/package/xamarin.ios-12.4.0.64.pkg";
    macSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10.2/9c8d8e0a50e68d9abc8cd48fcd47a669e981fcc9/53/package/xamarin.mac-5.4.0.64.pkg";

}
else if(releaseChannel == ReleaseChannel.Stable)
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
    

string androidSDK = IsRunningOnWindows() ? "" : androidSDK_macos;
string monoSDK = IsRunningOnWindows() ? "" : monoSDK_macos;
string iosSDK = IsRunningOnWindows() ? "" : iOSSDK_macos;
string macSDK  = IsRunningOnWindows() ? "" : macSDK_macos;

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
    });

Task("provision-androidsdk")
    .Description("Install Xamarin.Android SDK")
    .Does(async () =>
    {
        Information ("ANDROID_HOME: {0}", ANDROID_HOME);

        if(androidSdkManagerInstalls.Length > 0)
        {
            var androidSdkSettings = new AndroidSdkManagerToolSettings {
                SdkRoot = ANDROID_HOME,
                SkipVersionCheck = true
            };

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
    .Description("Restore target on Xamarin.Forms.sln")
    .Does(() =>
    {
        try{
            MSBuild("./Xamarin.Forms.sln", GetMSBuildSettings().WithTarget("restore"));
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

        MSBuild("./Xamarin.Forms.sln", msbuildSettings);

        binaryLogger.FileName = $"{artifactStagingDirectory}/win-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.Platform.UAP/Xamarin.Forms.Platform.UAP.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("DisableEmbeddedXbf", "false")
                        .WithProperty("EnableTypeInfoReflection", "false"));

        binaryLogger.FileName = $"{artifactStagingDirectory}/ios-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.Platform.iOS/Xamarin.Forms.Platform.iOS.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("USE2017", "true"));

        binaryLogger.FileName = $"{artifactStagingDirectory}/macos-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.Platform.MacOS/Xamarin.Forms.Platform.MacOS.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("USE2017", "true"));
    }
    catch(Exception)
    {
        if(IsRunningOnWindows())
            throw;
    }
});

Task("BuildTasks")
    .Description("Build Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.csproj")
    .Does(() =>
{
    MSBuild("./Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.csproj", GetMSBuildSettings().WithRestore());
});

Task("Build")
    .Description("Builds all necessary projects to run Control Gallery")
    .IsDependentOn("Restore")
    .Does(() =>
{
    try{
        MSBuild("./Xamarin.Forms.sln", GetMSBuildSettings().WithRestore());
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
            MSBuild("Xamarin.Forms.sln",
                    GetMSBuildSettings()
                        .WithRestore()
                        .WithProperty("AndroidTargetFrameworks", "MonoAndroid90;MonoAndroid10.0"));
    });

Task("VSMAC")
    .Description("Builds projects necessary so solution compiles on VSMAC")
    .Does(() =>
    {
        StartProcess("open", new ProcessSettings{ Arguments = "Xamarin.Forms.sln" });
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
        BuildiOSIpa("./Xamarin.Forms.sln", platform:"iPhoneSimulator", configuration:"Debug");

    });
*/
Task("DeployAndroid")
    .Description("Builds and deploy Android Control Gallery")
    .Does(() =>
    {
        MSBuild("./Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.csproj", GetMSBuildSettings().WithRestore());
        MSBuild("./Xamarin.Forms.ControlGallery.Android/Xamarin.Forms.ControlGallery.Android.csproj", GetMSBuildSettings().WithRestore());
        BuildAndroidApk("./Xamarin.Forms.ControlGallery.Android/Xamarin.Forms.ControlGallery.Android.csproj", sign:true, configuration:configuration);
        AdbUninstall("AndroidControlGallery.AndroidControlGallery");
        AdbInstall("./Xamarin.Forms.ControlGallery.Android/bin/Debug/AndroidControlGallery.AndroidControlGallery-Signed.apk");
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
    return new MSBuildSettings {
        PlatformTarget = PlatformTarget.MSIL,
        MSBuildPlatform = Cake.Common.Tools.MSBuild.MSBuildPlatform.x86,
        Configuration = configuration,
    };
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