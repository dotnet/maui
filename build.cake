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
#addin "nuget:?package=Cake.Boots&version=1.0.2.437"
#addin "nuget:?package=Cake.AppleSimulator&version=0.2.0"
#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"
//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

string agentName = EnvironmentVariable("AGENT_NAME", "");
bool isCIBuild = !String.IsNullOrWhiteSpace(agentName);
string artifactStagingDirectory = EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY", ".");
string workingDirectory = EnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY", ".");
var configuration = Argument("BUILD_CONFIGURATION", "Debug");

var target = Argument("target", "Default");
var IOS_SIM_NAME = Argument("IOS_SIM_NAME", "iPhone 8");
var IOS_SIM_RUNTIME = Argument("IOS_SIM_RUNTIME", "com.apple.CoreSimulator.SimRuntime.iOS-13-5");
var IOS_TEST_PROJ = "./Xamarin.Forms.Core.iOS.UITests/Xamarin.Forms.Core.iOS.UITests.csproj";
var IOS_TEST_LIBRARY = Argument("IOS_TEST_LIBRARY", $"./Xamarin.Forms.Core.iOS.UITests/bin/{configuration}/Xamarin.Forms.Core.iOS.UITests.dll");
var IOS_IPA_PATH = Argument("IOS_IPA_PATH", $"./Xamarin.Forms.ControlGallery.iOS/bin/iPhoneSimulator/{configuration}/XamarinFormsControlGalleryiOS.app");
var IOS_BUNDLE_ID = "com.xamarin.quickui.controlgallery";
var IOS_BUILD_IPA = Argument("IOS_BUILD_IPA", (target == "cg-ios-deploy") ? true : (false || isCIBuild) );

var UWP_PACKAGE_ID = "0d4424f6-1e29-4476-ac00-ba22c3789cb6";
var UWP_TEST_LIBRARY = GetBuildVariable("UWP_TEST_LIBRARY", $"./Xamarin.Forms.Core.Windows.UITests/bin/{configuration}/Xamarin.Forms.Core.Windows.UITests.dll");
var UWP_PFX_PATH = Argument("UWP_PFX_PATH", "Xamarin.Forms.ControlGallery.WindowsUniversal\\Xamarin.Forms.ControlGallery.WindowsUniversal_TemporaryKey.pfx");
var UWP_APP_PACKAGES_PATH = Argument("UWP_APP_PACKAGES_PATH", "*/AppPackages/");
var UWP_APP_DRIVER_INSTALL_PATH = Argument("UWP_APP_DRIVER_INSTALL_PATH", "https://github.com/microsoft/WinAppDriver/releases/download/v1.2-RC/WindowsApplicationDriver.msi");
var ANDROID_RENDERERS = Argument("ANDROID_RENDERERS", "FAST");
var XamarinFormsVersion = Argument("XamarinFormsVersion", "");
var packageVersion = Argument("packageVersion", "");
var releaseChannelArg = Argument("CHANNEL", "Stable");
releaseChannelArg = EnvironmentVariable("CHANNEL") ?? releaseChannelArg;
var teamProject = Argument("TeamProject", "");
bool buildForVS2017 = Convert.ToBoolean(Argument("buildForVS2017", "false"));
bool isHostedAgent = agentName.StartsWith("Azure Pipelines") || agentName.StartsWith("Hosted Agent");


var NUNIT_TEST_WHERE = Argument("NUNIT_TEST_WHERE", "cat != Shell && cat != CollectionView && cat != UwpIgnore && cat != CarouselView");
var ExcludeCategory = GetBuildVariable("ExcludeCategory", "")?.Replace("\"", "");
var ExcludeCategory2 = GetBuildVariable("ExcludeCategory2", "")?.Replace("\"", "");
var IncludeCategory = GetBuildVariable("IncludeCategory", "")?.Replace("\"", "");

// Replace Azure devops syntax for unit tests to Nunit3 filters
if(!String.IsNullOrWhiteSpace(ExcludeCategory))
{
    ExcludeCategory = String.Join(" && cat != ", ExcludeCategory.Split(new string[] { "--exclude-category" }, StringSplitOptions.None));
    if(!ExcludeCategory.StartsWith("cat"))
        ExcludeCategory = $" cat !=  {ExcludeCategory}";

    NUNIT_TEST_WHERE = $"{NUNIT_TEST_WHERE} && {ExcludeCategory}";
}

if(!String.IsNullOrWhiteSpace(ExcludeCategory2))
{
    ExcludeCategory2 = String.Join(" && cat != ", ExcludeCategory2.Split(new string[] { "--exclude-category" }, StringSplitOptions.None));
    if(!ExcludeCategory2.StartsWith("cat"))
        ExcludeCategory2 = $" cat !=  {ExcludeCategory2}";

    NUNIT_TEST_WHERE = $"{NUNIT_TEST_WHERE} && {ExcludeCategory2}";
}

if(!String.IsNullOrWhiteSpace(IncludeCategory))
{
    IncludeCategory = String.Join(" || cat == ", IncludeCategory.Split(new string[] { "--include-category" }, StringSplitOptions.None));
    if(!IncludeCategory.StartsWith("cat"))
        IncludeCategory = $" cat ==  {IncludeCategory}";

    NUNIT_TEST_WHERE = $"({NUNIT_TEST_WHERE}) && ({IncludeCategory})";
}

var ANDROID_HOME = EnvironmentVariable("ANDROID_HOME") ??
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");
string[] androidSdkManagerInstalls = new [] { "platforms;android-28", "platforms;android-29", "build-tools;29.0.3"};

(string name, string location)[] windowsSdksInstalls = new (string name, string location)[]
{
    ("10.0.19041.0", "https://go.microsoft.com/fwlink/p/?linkid=2120843"), 
    ("10.0.18362.0", "https://go.microsoft.com/fwlink/?linkid=2083338"),
    ("10.0.16299.0", "https://go.microsoft.com/fwlink/p/?linkid=864422"),
    ("10.0.14393.0", "https://go.microsoft.com/fwlink/p/?LinkId=838916")
};

string[] netFrameworkSdksLocalInstall = new string[]
{
    "https://go.microsoft.com/fwlink/?linkid=2099470", //NET461 SDK
    "https://go.microsoft.com/fwlink/?linkid=874338", //NET472 SDK
    "https://go.microsoft.com/fwlink/?linkid=2099465", //NET47
    "https://download.microsoft.com/download/A/1/D/A1D07600-6915-4CB8-A931-9A980EF47BB7/NDP47-DevPack-KB3186612-ENU.exe" //net47 targeting pack
};

// these don't run on CI
(string msiUrl, string cabUrl)[] netframeworkMSI = new (string msiUrl, string cabUrl)[]
{
    (
        "https://download.visualstudio.microsoft.com/download/pr/34dae2b3-314f-465e-aba0-0a862c29638e/b2bc986f304acdd76fcd8f910012b656/sdk_tools462.msi",
        "https://download.visualstudio.microsoft.com/download/pr/6283f4a0-36b3-4336-a6f2-c5afd9f8fdbb/ffbe35e429f7d5c1d3777d03b2f38a24/sdk_tools462.cab"
    ),
    (
        "https://download.visualstudio.microsoft.com/download/pr/0d63c72c-9341-4de6-b493-dc7ad0d01246/f16b6402b8f8fb3b95dde5c1c2e5a2b4/sdk_tools461.msi",
        "https://download.visualstudio.microsoft.com/download/pr/3dc58ffd-d515-43a4-87bd-2aba395eab17/5bff8f781c9843d64bd2367898395c5e/sdk_tools461.cab"
    ),
    (
        "https://download.visualstudio.microsoft.com/download/pr/9d14aa59-3f7f-4fe6-85e9-3bc31031e1f2/88b90ec9d096ec382a001e1fbd4a6be8/sdk_tools472.msi",
        "https://download.visualstudio.microsoft.com/download/pr/77f1d250-f253-4c48-849c-0f08c9c11e77/ab2aa8f856e686cd4ad1c921742f2eeb/sdk_tools472.cab"
    )
};

Information ("XamarinFormsVersion: {0}", XamarinFormsVersion);
Information ("ANDROID_RENDERERS: {0}", ANDROID_RENDERERS);
Information ("configuration: {0}", configuration);
Information ("ANDROID_HOME: {0}", ANDROID_HOME);
Information ("Team Project: {0}", teamProject);
Information ("buildForVS2017: {0}", buildForVS2017);
Information ("Agent.Name: {0}", agentName);
Information ("isCIBuild: {0}", isCIBuild);
Information ("artifactStagingDirectory: {0}", artifactStagingDirectory);
Information("workingDirectory: {0}", workingDirectory);

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

if(buildForVS2017)
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

string androidSDK_windows = "";
string iOSSDK_windows = "";
string monoSDK_windows = "";
string macSDK_windows = "";

if(!buildForVS2017)
{
    androidSDK_macos = EnvironmentVariable("ANDROID_SDK_MAC", androidSDK_macos);
    iOSSDK_macos = EnvironmentVariable("IOS_SDK_MAC", iOSSDK_macos);
    monoSDK_macos = EnvironmentVariable("MONO_SDK_MAC", monoSDK_macos);
    macSDK_macos = EnvironmentVariable("MAC_SDK_MAC", macSDK_macos);

    androidSDK_windows = EnvironmentVariable("ANDROID_SDK_WINDOWS", "");
    iOSSDK_windows = EnvironmentVariable("IOS_SDK_WINDOWS", "");
    monoSDK_windows = EnvironmentVariable("MONO_SDK_WINDOWS", "");
    macSDK_windows = EnvironmentVariable("MAC_SDK_WINDOWS", "");
}

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
        if (!IsRunningOnWindows()) {
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

            try{
                AcceptLicenses (androidSdkSettings);
            }
            catch{}

            try{
                AndroidSdkManagerUpdateAll (androidSdkSettings);
            }
            catch{}
            
            try{
                AcceptLicenses (androidSdkSettings);
            }
            catch{}

            try{
                AndroidSdkManagerInstall (androidSdkManagerInstalls, androidSdkSettings);
            }
            catch{}
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

Task("provision-windowssdk")
    .Description("Install Windows SDK")
    .Does(() =>
    {
        if(IsRunningOnWindows() && !isHostedAgent)
        {
            int i = 0;
            foreach(var windowsSdk in windowsSdksInstalls)
            {
                string sdkPath = System.IO.Path.Combine(@"C:\Program Files (x86)\Windows Kits\10\Platforms\UAP", windowsSdk.name);
                if(DirectoryExists(sdkPath) && GetFiles(System.IO.Path.Combine(sdkPath, "*.*")).Count() > 0)
                {
                    Information("Already Installed: {0}", sdkPath);
                    continue;
                }


                Information("Installing: {0}", sdkPath);
                string installUrl = windowsSdk.location;
                string installerPath = $"{System.IO.Path.GetTempPath()}" + $"WindowsSDK{i}.exe";
                DownloadFile(installUrl, installerPath);

                var result = StartProcess(installerPath, new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append(@"/features + /q")
                    }
                );

                i++;
            }
        }
    });

Task("provision-netsdk-local")
    .Description("Install .NET SDK")
    .Does(() =>
    {
        if(IsRunningOnWindows() && (!isCIBuild || target == "provision-netsdk-local"))
        {
            foreach(var installUrl in netframeworkMSI)
            {
                string msiUrl = installUrl.msiUrl;
                string cabUrl = installUrl.cabUrl;


                string cabName = cabUrl.Split('/').Last();
                string msiName = msiUrl.Split('/').Last();                
                string cabPath = $"{System.IO.Path.GetTempPath()}{cabName}";

                Information("Downloading: {0} to {1}", cabUrl, cabPath);
                DownloadFile(cabUrl, cabPath);
                InstallMsi(msiUrl, null, msiName);
            }

            int i = 0;
            foreach(var installUrl in netFrameworkSdksLocalInstall)
            {
                Information("Installing: {0}", installUrl);
                string installerPath = $"{System.IO.Path.GetTempPath()}" + $"netSDKS{i}.exe";
                DownloadFile(installUrl, installerPath);

                var result = StartProcess(installerPath, new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append(@"/quiet")
                    }
                );

                i++;
            }
        }
    });

Task ("cg-uwp")
    .IsDependentOn("BuildTasks")
    .Does (() =>
{
    MSBuild ("Xamarin.Forms.ControlGallery.WindowsUniversal\\Xamarin.Forms.ControlGallery.WindowsUniversal.csproj", 
        GetMSBuildSettings().WithRestore());
});

Task ("cg-uwp-build-tests")
    .IsDependentOn("BuildTasks")
    .Does (() =>
{
    MSBuild ("Xamarin.Forms.ControlGallery.WindowsUniversal\\Xamarin.Forms.ControlGallery.WindowsUniversal.csproj", 
        GetMSBuildSettings(null)
            .WithProperty("AppxBundlePlatforms", "x86")
            .WithProperty("AppxBundle", "Always")
            .WithProperty("UapAppxPackageBuildMode", "StoreUpload")
            .WithProperty("AppxPackageSigningEnabled", "true")
            .WithProperty("PackageCertificateThumbprint", "a59087cc92a9a8117ffdb5255eaa155748f9f852")
            .WithProperty("PackageCertificateKeyFile", "Xamarin.Forms.ControlGallery.WindowsUniversal_TemporaryKey.pfx")
            .WithProperty("PackageCertificatePassword", "")
            .WithRestore()
    );

    MSBuild("Xamarin.Forms.Core.Windows.UITests\\Xamarin.Forms.Core.Windows.UITests.csproj", 
        GetMSBuildSettings().WithRestore());
});

Task ("cg-uwp-deploy")
    .WithCriteria(IsRunningOnWindows())
    .Does (() =>
{
    var uninstallPS = new Action (() => {
        try {
            StartProcess ("powershell",
                "$app = Get-AppxPackage -Name " + UWP_PACKAGE_ID + "; if ($app) { Remove-AppxPackage -Package $app.PackageFullName }");
        } catch { }
    });
    // Try to uninstall the app if it exists from before
    uninstallPS();

    StartProcess("certutil", "-f -p \"\" -importpfx \"" + UWP_PFX_PATH + "\"");
    
    // Install the appx
    var dependencies = GetFiles(UWP_APP_PACKAGES_PATH + "*/Dependencies/x86/*.appx");

    foreach (var dep in dependencies) {
        try
        {
            Information("Installing Dependency appx: {0}", dep);
            StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(dep).FullPath + "\"");
        }
        catch(Exception exc)
        {
            Information("Error: {0}", exc);
        }
    }

    var appxBundlePath = GetFiles(UWP_APP_PACKAGES_PATH + "*/*.appxbundle").First ();
    Information("Installing appx: {0}", appxBundlePath);
    StartProcess ("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(appxBundlePath).FullPath + "\"");
});

Task("cg-uwp-run-tests")
    .IsDependentOn("provision-uitests-uwp")
    .Does(() =>
    {
        System.Diagnostics.Process process = null;
        if(!isHostedAgent)
        {
            try
            {
                var info = new System.Diagnostics.ProcessStartInfo(@"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe")
                {
                };

                process =  System.Diagnostics.Process.Start(info);
            }
            catch(Exception exc)
            {
                Information("Failed: {0}", exc);
            }
        }

        try
        {
            NUnit3(new [] { UWP_TEST_LIBRARY },
                new NUnit3Settings {
                    Params = new Dictionary<string, string>()
                    {
                    },
                    Where = NUNIT_TEST_WHERE
                });
        }
        finally
        {
            try
            {
                process?.Kill();
            }
            catch{}
        }
    });

Task("cg-uwp-run-tests-ci")
    .IsDependentOn("provision-windowssdk")
    .IsDependentOn("cg-uwp-deploy")
    .IsDependentOn("cg-uwp-run-tests")
    .Does(() =>
    {
    });

Task("provision-uitests-uwp")
    .WithCriteria(IsRunningOnWindows() && !isHostedAgent)
    .Description("Installs and Starts WindowsApplicationDriver. Use WinAppDriverPath to specify WinAppDriver Location.")
    .Does(() =>
    {
        string installPath = Argument("WinAppDriverPath", @"C:\Program Files (x86)\");
        string driverPath = System.IO.Path.Combine(installPath, "Windows Application Driver");
        if(!DirectoryExists(driverPath))
        {
            try{
                InstallMsi(UWP_APP_DRIVER_INSTALL_PATH, installPath);
            }
            catch(Exception e)
            {
                Information("Failed to Install Win App Driver: {0}", e);
            }
        }
    });

void InstallMsi(string msiFile, string installTo, string fileName = "InstallFile.msi")
{
    string installerPath = $"{System.IO.Path.GetTempPath()}{fileName}";
        
    try
    {
        Information ("Installing: {0}", msiFile);
        DownloadFile(msiFile, installerPath);
        Information("File Downloaded To: {0}", installerPath);

        var argumentBuilder = 
            new ProcessArgumentBuilder()
                .Append("/a")
                .Append(installerPath)
                .Append("/qn");

        if(!String.IsNullOrWhiteSpace(installTo))
        {
            Information("Installing into: {0}", installTo);
            argumentBuilder = argumentBuilder.Append("TARGETDIR=\"" + installTo + "\"");
        }

        var result = StartProcess("msiexec", new ProcessSettings {
            Arguments = argumentBuilder
        });

        if(result != 0)
            throw new Exception("Failed to install: " + msiFile);

        Information("File Installed: {0}", result);
    }
    catch(Exception exc)
    {
        Information("Failed to install {0} make sure you are running script as admin {1}", msiFile, exc);
        throw;
    }
    finally{
        DeleteFile(installerPath);

    }
}

Task("provision")
    .Description("Install SDKs required to build project")
    .IsDependentOn("provision-macsdk")
    .IsDependentOn("provision-iossdk")
    .IsDependentOn("provision-androidsdk")
    .IsDependentOn("provision-netsdk-local")
    .IsDependentOn("provision-windowssdk")
    .IsDependentOn("provision-monosdk"); // always provision monosdk last otherwise CI might fail

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

Task("WriteGoogleMapsAPIKey")
    .Description("Write GoogleMapsAPIKey to Android Control Gallery")
    .Does(() =>
    {    
        string GoogleMapsAPIKey = Argument("GoogleMapsAPIKey", "");

        if(!String.IsNullOrWhiteSpace(GoogleMapsAPIKey))
        {
            Information("Writing GoogleMapsAPIKey");
            System.IO.File.WriteAllText("Xamarin.Forms.ControlGallery.Android/Properties/MapsKey.cs", "[assembly: Android.App.MetaData(\"com.google.android.maps.v2.API_KEY\", Value = \"" + GoogleMapsAPIKey + "\")]");
        }
    });

Task("BuildForNuget")
    .IsDependentOn("BuildTasks")
    .Description("Builds all necessary projects to create Nuget Packages")
    .Does(() =>
{
    try
    {

        var msbuildSettings = GetMSBuildSettings();
        var binaryLogger = new MSBuildBinaryLogSettings {
            Enabled  = isCIBuild
        };

        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/win-{configuration}.binlog";
        MSBuild("./Xamarin.Forms.sln", msbuildSettings);
        
        // // This currently fails on CI will revisit later
        // if(isCIBuild)
        // {        
        //     MSBuild("./Xamarin.Forms.Xaml.UnitTests/Xamarin.Forms.Xaml.UnitTests.csproj", GetMSBuildSettings().WithTarget("Restore"));
        //     MSBuild("./Xamarin.Forms.Xaml.UnitTests/Xamarin.Forms.Xaml.UnitTests.csproj", GetMSBuildSettings());
        // }

        // MSBuild("./Xamarin.Forms.sln", GetMSBuildSettings().WithTarget("Restore"));
        // MSBuild("./Xamarin.Forms.DualScreen.sln", GetMSBuildSettings().WithTarget("Restore"));

        // if(isCIBuild)
        // {       
        //     foreach(var platformProject in GetFiles("./Xamarin.*.UnitTests/*.csproj").Select(x=> x.FullPath))
        //     {
        //         if(platformProject.Contains("Xamarin.Forms.Xaml.UnitTests"))
        //             continue;

        //         Information("Building: {0}", platformProject);
        //         MSBuild(platformProject,
        //                 GetMSBuildSettings().WithRestore());
        //     }
        // }

        // MSBuild("./Xamarin.Forms.sln", GetMSBuildSettings().WithTarget("Restore"));
        // MSBuild("./Xamarin.Forms.DualScreen.sln", GetMSBuildSettings().WithTarget("Restore"));
        
        // msbuildSettings.BinaryLogger = binaryLogger;
        
        // var platformProjects = 
        //     GetFiles("./Xamarin.Forms.Platform.*/*.csproj")
        //         .Union(GetFiles("./Stubs/*/*.csproj"))
        //         .Union(GetFiles("./Xamarin.Forms.Maps.*/*.csproj"))
        //         .Union(GetFiles("./Xamarin.Forms.Pages.*/*.csproj"))
        //         .Union(GetFiles("./Xamarin.Forms.Material.*/*.csproj"))
        //         .Union(GetFiles("./Xamarin.Forms.Core.Design/*.csproj"))
        //         .Union(GetFiles("./Xamarin.Forms.Xaml.Design/*.csproj"))
        //         .Select(x=> x.FullPath).Distinct()
        //         .ToList();

        // foreach(var platformProject in platformProjects)
        // {
        //     if(platformProject.Contains("UnitTests"))
        //         continue;
                
        //     msbuildSettings = GetMSBuildSettings();
        //     string projectName = platformProject
        //         .Replace(' ', '_')
        //         .Split('/')
        //         .Last();

        //     binaryLogger.FileName = $"{artifactStagingDirectory}/{projectName}-{configuration}.binlog";
        //     msbuildSettings.BinaryLogger = binaryLogger;

        //     Information("Building: {0}", platformProject);
        //     MSBuild(platformProject,
        //             msbuildSettings);
        // }

        // dual screen
        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/dualscreen-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.DualScreen/Xamarin.Forms.DualScreen.csproj",
                    msbuildSettings
                        .WithRestore()
                        .WithTarget("rebuild"));


        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/win-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.Platform.UAP/Xamarin.Forms.Platform.UAP.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("DisableEmbeddedXbf", "false")
                        .WithProperty("EnableTypeInfoReflection", "false"));

        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/ios-{configuration}-csproj.binlog";
        MSBuild("./Xamarin.Forms.Platform.iOS/Xamarin.Forms.Platform.iOS.csproj",
                    msbuildSettings
                        .WithTarget("rebuild")
                        .WithProperty("USE2017", "true"));

        msbuildSettings = GetMSBuildSettings();
        msbuildSettings.BinaryLogger = binaryLogger;
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

Task("cg-android")
    .Description("Builds Android Control Gallery")
    .IsDependentOn("WriteGoogleMapsAPIKey")
    .IsDependentOn("BuildTasks")
    .Does(() => 
    {
        var buildSettings = GetMSBuildSettings();

        if(isCIBuild)
        {
            buildSettings = buildSettings.WithTarget("Rebuild").WithTarget("SignAndroidPackage");
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true
            };

            buildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/android-{ANDROID_RENDERERS}_{buildForVS2017}.binlog";
        }
        else
        {
            buildSettings = buildSettings.WithRestore();
        }

        MSBuild("./Xamarin.Forms.ControlGallery.Android/Xamarin.Forms.ControlGallery.Android.csproj", buildSettings);
    });

Task("cg-android-vs")
    .Description("Builds Android Control Gallery and open VS")
    .IsDependentOn("cg-android")
    .Does(() => 
    {
        StartVisualStudio();
    });

Task("cg-ios")
    .Description("Builds iOS Control Gallery and open VS")
    .IsDependentOn("BuildTasks")
    .Does(() =>
    {
        var buildSettings = 
            GetMSBuildSettings(null)
                .WithProperty("BuildIpa", $"{IOS_BUILD_IPA}");

        if(isCIBuild)
        {
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true
            };

            buildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/ios-cg-2017_{buildForVS2017}.binlog";
        }
        else
        {
            buildSettings = buildSettings.WithRestore();
        }

        MSBuild("./Xamarin.Forms.ControlGallery.iOS/Xamarin.Forms.ControlGallery.iOS.csproj", 
            buildSettings);
    });

Task("cg-ios-vs")
    .Description("Builds iOS Control Gallery and open VS")
    .IsDependentOn("cg-ios")
    .Does(() =>
    {   
        StartVisualStudio();
    });

Task("cg-ios-build-tests")
    .IsDependentOn("BuildTasks")
    .Does(() =>
    {
        // the UI Tests all reference the galleries so those get built as a side effect of building the
        // ui tests
        var buildSettings = 
            GetMSBuildSettings(null, configuration)
                .WithProperty("MtouchArch", "x86_64")
                .WithProperty("iOSPlatform", "iPhoneSimulator")
                .WithProperty("BuildIpa", $"true")
                .WithRestore();

        if(isCIBuild)
        {
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true,
                FileName = $"{artifactStagingDirectory}/ios-uitests-2017_{buildForVS2017}.binlog"
            };

            buildSettings.BinaryLogger = binaryLogger;
        }

        MSBuild(IOS_TEST_PROJ, buildSettings);
    });

Task("cg-ios-run-tests")
    .Does(() =>
    {
        var sim = GetIosSimulator();
        NUnit3(new [] { IOS_TEST_LIBRARY }, 
            new NUnit3Settings {
                Params = new Dictionary<string, string>()
                {
                    {"UDID", GetIosSimulator().UDID}
                },
                Where = NUNIT_TEST_WHERE
            });
    });

Task("cg-ios-run-tests-ci")
    .IsDependentOn("cg-ios-deploy")
    .IsDependentOn("cg-ios-run-tests")
    .Does(() =>
    {
    });

Task ("cg-ios-deploy")
    .Does (() =>
{
    // Look for a matching simulator on the system
    var sim = GetIosSimulator();

    // Boot the simulator
    Information("Booting: {0} ({1} - {2})", sim.Name, sim.Runtime, sim.UDID);
    if (!sim.State.ToLower().Contains ("booted"))
        BootAppleSimulator (sim.UDID);

    // Wait for it to be booted
    var booted = false;
    for (int i = 0; i < 100; i++) {
        if (ListAppleSimulators().Any (s => s.UDID == sim.UDID && s.State.ToLower().Contains("booted"))) {
            booted = true;
            break;
        }
        System.Threading.Thread.Sleep(1000);
    }

    // Install the IPA that was previously built
    var ipaPath = new FilePath(IOS_IPA_PATH);
    Information ("Installing: {0}", ipaPath);
    InstalliOSApplication(sim.UDID, MakeAbsolute(ipaPath).FullPath);


    // Launch the IPA
    Information("Launching: {0}", IOS_BUNDLE_ID);
    LaunchiOSApplication(sim.UDID, IOS_BUNDLE_ID);
});

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

Task("_PrintEnvironmentVariables")
    .Does(() => 
    {       
        var envVars = EnvironmentVariables();

        string path;
        if (envVars.TryGetValue("PATH", out path))
        {
            Information("Path: {0}", path);
        }

        foreach(var envVar in envVars)
        {
            Information(
                "Key: {0}\tValue: \"{1}\"",
                envVar.Key,
                envVar.Value
                );
        }
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

T GetBuildVariable<T>(string key, T defaultValue)
{
    return Argument(key, EnvironmentVariable(key, defaultValue));
}

void StartVisualStudio(string sln = "Xamarin.Forms.sln")
{
    if(isCIBuild)
        return;

    if(IsRunningOnWindows())
         StartProcess("start", new ProcessSettings{ Arguments = "Xamarin.Forms.sln" });
    else
         StartProcess("open", new ProcessSettings{ Arguments = "Xamarin.Forms.sln" });
}

MSBuildSettings GetMSBuildSettings(PlatformTarget? platformTarget = PlatformTarget.MSIL, string buildConfiguration = null)
{
    var buildSettings =  new MSBuildSettings {
        PlatformTarget = platformTarget,
        MSBuildPlatform = Cake.Common.Tools.MSBuild.MSBuildPlatform.x86,
        Configuration = buildConfiguration ?? configuration,
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

AppleSimulator GetIosSimulator()
{
    var sims = ListAppleSimulators ();
    // Look for a matching simulator on the system
    var sim = sims.First (s => s.Name == IOS_SIM_NAME && s.Runtime == IOS_SIM_RUNTIME);
    return sim;
}
