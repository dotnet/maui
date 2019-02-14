// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// examples
/*

./build.ps1 -Target NugetPack
./build.ps1 -Target NugetPack -ScriptArgs '-packageVersion="9.9.9-custom"'



 */
//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////
#addin "nuget:?package=Cake.Xamarin&version=3.0.0"
#addin "nuget:?package=Cake.Android.Adb&version=3.0.0"
#addin "nuget:?package=Cake.Git&version=0.19.0"
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

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./**/obj", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));
    CleanDirectories("./**/bin", (fsi)=> !fsi.Path.FullPath.Contains("XFCorePostProcessor") && !fsi.Path.FullPath.StartsWith("tools"));

});

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
        MSBuild("./Xamarin.Forms.ControlGallery.Android/Xamarin.Forms.ControlGallery.Android.csproj", 
                GetMSBuildSettings()
                    .WithRestore()
                    // work around bug on vs mac where resources generate wrong first time
                    .WithTarget("rebuild")
        );

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