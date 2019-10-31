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

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool nuget:?package=GitVersion.CommandLine&version=4.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var gitVersion = GitVersion();
var majorMinorPatch = gitVersion.MajorMinorPatch;
var informationalVersion = gitVersion.InformationalVersion;
var buildVersion = gitVersion.FullBuildMetaData;
var nugetversion = Argument<string>("packageVersion", gitVersion.NuGetVersion);

var ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? 
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");

string monoMajorVersion = "5.14.0";
string monoPatchVersion = "177";
string monoVersion = $"{monoMajorVersion}.{monoPatchVersion}";

string monoSDK_windows = $"https://download.mono-project.com/archive/{monoMajorVersion}/windows-installer/mono-{monoVersion}-x64-0.msi";
string androidSDK_windows = "";//"https://aka.ms/xamarin-android-commercial-d15-9-windows";
string iOSSDK_windows = "https://download.visualstudio.microsoft.com/download/pr/71f33151-5db4-49cc-ac70-ba835a9f81e2/d256c6c50cd80ec0207783c5c7a4bc2f/xamarin.visualstudio.apple.sdk.4.12.3.83.vsix";
string macSDK_windows = "";

string androidSDK_macos = "https://aka.ms/xamarin-android-commercial-d15-9-macos";
string monoSDK_macos = $"https://download.mono-project.com/archive/{monoMajorVersion}/macos-10-universal/MonoFramework-MDK-{monoVersion}.macos10.xamarin.universal.pkg";
string iOSSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10.2/9c8d8e0a50e68d9abc8cd48fcd47a669e981fcc9/53/package/xamarin.ios-12.4.0.64.pkg";
string macSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10.2/9c8d8e0a50e68d9abc8cd48fcd47a669e981fcc9/53/package/xamarin.mac-5.4.0.64.pkg";

string androidSDK = IsRunningOnWindows() ? androidSDK_windows : androidSDK_macos;
string monoSDK = IsRunningOnWindows() ? monoSDK_windows : monoSDK_macos;
string iosSDK = IsRunningOnWindows() ? "" : iOSSDK_macos;
string macSDK  = IsRunningOnWindows() ? "" : macSDK_macos;

string[] androidSdkManagerInstalls = new string[0];//new [] { "platforms;android-24", "platforms;android-28"};
            
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./**/obj", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));
    CleanDirectories("./**/bin", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));
});

Task("provision-macsdk")
    .Does(async () =>
    {
        if(!IsRunningOnWindows() && !String.IsNullOrWhiteSpace(macSDK))
        {
            await Boots(macSDK);
        }
    });

Task("provision-iossdk")
    .Does(async () =>
    {
        if(!IsRunningOnWindows())
        {   
            if(!String.IsNullOrWhiteSpace(iosSDK))
                await Boots(iosSDK);
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
        if(!String.IsNullOrWhiteSpace(androidSDK))
            await Boots (androidSDK);
    });

Task("provision-monosdk")
    .Does(async () =>
    {
        if(IsRunningOnWindows())
        {
            if(!String.IsNullOrWhiteSpace(monoSDK))
            {
                string monoPath = $"{System.IO.Path.GetTempPath()}/mono.msi";
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
    .IsDependentOn("provision-macsdk")
    .IsDependentOn("provision-iossdk")
    .IsDependentOn("provision-monosdk")
    .IsDependentOn("provision-androidsdk");

Task("NuGetPack")
    .IsDependentOn("Build")
    .IsDependentOn("_NuGetPack");


Task("_NuGetPack")
    .Does(() =>
    {
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


Task("BuildHack")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        if(!IsRunningOnWindows())
        {
            MSBuild("./Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.csproj", GetMSBuildSettings().WithRestore());
        }  
    });

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("BuildHack")
    .IsDependentOn("Android81")
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

Task("Android81")
    .IsDependentOn("BuildHack")
    .Does(() =>
    {
        string[] androidProjects = 
            new []
            {
                "./Xamarin.Forms.Platform.Android/Xamarin.Forms.Platform.Android.csproj",
                "./Xamarin.Forms.Platform.Android.AppLinks/Xamarin.Forms.Platform.Android.AppLinks.csproj",
                "./Xamarin.Forms.Maps.Android/Xamarin.Forms.Maps.Android.csproj",
                "./Stubs/Xamarin.Forms.Platform.Android/Xamarin.Forms.Platform.Android (Forwarders).csproj"
            };

        foreach(var project in androidProjects)
            MSBuild(project, 
                    GetMSBuildSettings()
                        .WithRestore()
                        .WithProperty("AndroidTargetFrameworkVersion", "v8.1"));
    });

Task("VSMAC")
    .IsDependentOn("BuildHack")
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
    .IsDependentOn("BuildHack")
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
    var msbuildSettings =  new MSBuildSettings();

    msbuildSettings.PlatformTarget = PlatformTarget.MSIL;
    msbuildSettings.MSBuildPlatform = (Cake.Common.Tools.MSBuild.MSBuildPlatform)1;
    msbuildSettings.Configuration = configuration;
    return msbuildSettings;

}
