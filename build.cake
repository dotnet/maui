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
#addin "nuget:?package=Cake.Xamarin&version=3.0.0"
#addin "nuget:?package=Cake.Android.Adb&version=3.0.0"
#addin "nuget:?package=Cake.Git&version=0.19.0"
#addin "nuget:?package=Cake.Android.SdkManager&version=3.0.2"
#addin "nuget:?package=Cake.Boots&version=1.0.0.291"

#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ??
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");

string monoMajorVersion = "5.18.1";
string monoPatchVersion = "28";
string monoVersion = $"{monoMajorVersion}.{monoPatchVersion}";

string monoSDK_windows = "";//$"https://download.mono-project.com/archive/{monoMajorVersion}/windows-installer/mono-{monoVersion}-x64-0.msi";
string androidSDK_windows = "";//"https://aka.ms/xamarin-android-commercial-d15-9-windows";
string iOSSDK_windows = "";//"https://download.visualstudio.microsoft.com/download/pr/71f33151-5db4-49cc-ac70-ba835a9f81e2/d256c6c50cd80ec0207783c5c7a4bc2f/xamarin.visualstudio.apple.sdk.4.12.3.83.vsix";
string macSDK_windows = "";

monoMajorVersion = "6.6.0";
monoPatchVersion = "";
if(String.IsNullOrWhiteSpace(monoPatchVersion))
    monoVersion = $"{monoMajorVersion}";
else
    monoVersion = $"{monoMajorVersion}.{monoPatchVersion}";
    
string androidSDK_macos = "https://aka.ms/xamarin-android-commercial-d16-4-macos";
string monoSDK_macos = $"https://download.mono-project.com/archive/{monoMajorVersion}/macos-10-universal/MonoFramework-MDK-{monoVersion}.macos10.xamarin.universal.pkg";
string iOSSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode11.3/5f802ef535488d12886f264b598b9c59ca2f2404/36/package/notarized/xamarin.ios-13.10.0.17.pkg";
string macSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode11.3/5f802ef535488d12886f264b598b9c59ca2f2404/36/package/notarized/xamarin.mac-6.10.0.17.pkg";

string androidSDK = IsRunningOnWindows() ? androidSDK_windows : androidSDK_macos;
string monoSDK = IsRunningOnWindows() ? monoSDK_windows : monoSDK_macos;
string iosSDK = IsRunningOnWindows() ? iOSSDK_windows : iOSSDK_macos;
string macSDK  = IsRunningOnWindows() ? "" : macSDK_macos;

string[] androidSdkManagerInstalls = new string[0];//new [] { "platforms;android-24", "platforms;android-28"};

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
        if(!IsRunningOnWindows() && !String.IsNullOrWhiteSpace(macSDK))
        {
            await Boots(macSDK);
        }
    });

Task("provision-iossdk")
    .Description("Install Xamarin.iOS SDK")
    .Does(async () =>
    {
        if(!String.IsNullOrWhiteSpace(iosSDK))
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
                SdkRoot = ANDROID_HOME,
                SkipVersionCheck = true
            };


            AcceptLicenses (androidSdkSettings);

            AndroidSdkManagerUpdateAll (androidSdkSettings);

            AcceptLicenses (androidSdkSettings);

            AndroidSdkManagerInstall (androidSdkManagerInstalls, androidSdkSettings);
        }
        if(!String.IsNullOrWhiteSpace(androidSDK))
            await Boots (androidSDK);
    });

Task("provision-monosdk")
    .Description("Install Mono SDK")
    .Does(async () =>
    {
        if(IsRunningOnWindows())
        {
            if(!String.IsNullOrWhiteSpace(monoSDK))
            {
                string monoPath = $"{System.IO.Path.GetTempPath()}mono.msi";

                if(!String.IsNullOrWhiteSpace(EnvironmentVariable("Build.Repository.LocalPath")))
                    monoPath = EnvironmentVariable("Build.Repository.LocalPath") + "\\" + "mono.msi";

                Information("Mono Path: {0}", monoPath);
                Information("Mono Version: {0}", monoSDK);
                DownloadFile(monoSDK, monoPath);

                StartProcess("msiexec", new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append(@"/i")
                        .Append(monoPath)
                        .Append("/qn")
                    }
                );
            }
        }
        else
        {
            if(!String.IsNullOrWhiteSpace(monoSDK))
                await Boots(monoSDK);
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
    .IsDependentOn("Build")
    .IsDependentOn("_NuGetPack");


Task("_NuGetPack")
    .Description("Create Nugets without building anything")
    .Does(() =>
    {
        var nugetVersionFile = 
            GetFiles(".XamarinFormsVersionFile.txt");
        var nugetversion = FileReadText(nugetVersionFile.First());

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

Task("Build")
    .Description("Builds all necessary projects to create Nuget Packages")
    .IsDependentOn("Restore")
    .Does(() =>
{
    try{
        MSBuild("./Xamarin.Forms.sln", GetMSBuildSettings().WithRestore());

        MSBuild("./Xamarin.Forms.Platform.UAP/Xamarin.Forms.Platform.UAP.csproj",
                    GetMSBuildSettings()
                        .WithRestore()
                        .WithProperty("DisableEmbeddedXbf", "false"));
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
