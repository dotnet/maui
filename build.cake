// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// examples
/*

Windows CMD:
build.cmd -Target dotnet-pack
build.cmd -Target dotnet-pack -ScriptArgs '--packageVersion="9.9.9-custom"','--configuration="Release"'

PowerShell:
./build.ps1 -Target dotnet-pack
./build.ps1 -Target dotnet-pack -ScriptArgs '--packageVersion="9.9.9-custom"'

 */
//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////
#addin "nuget:?package=Cake.Android.SdkManager&version=3.0.2"
#addin "nuget:?package=Cake.Boots&version=1.0.4.600-preview1"
#addin "nuget:?package=Cake.AppleSimulator&version=0.2.0"
#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"
#load "eng/cake/dotnet.cake"
#load "eng/cake/helpers.cake"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool "nuget:?package=nuget.commandline&version=5.8.1"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

string agentName = EnvironmentVariable("AGENT_NAME", "");
bool isCIBuild = !String.IsNullOrWhiteSpace(agentName);
string artifactStagingDirectory = EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY", ".");
string workingDirectory = EnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY", ".");
string envProgramFiles = EnvironmentVariable("ProgramFiles(x86)");
var configuration = GetBuildVariable("BUILD_CONFIGURATION", GetBuildVariable("configuration", "DEBUG"));
var msbuildPath = GetBuildVariable("msbuild", $"{envProgramFiles}\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe");

var target = Argument("target", "Default");
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

var IOS_SIM_NAME = GetBuildVariable("IOS_SIM_NAME", "iPhone 8");
var IOS_SIM_RUNTIME = GetBuildVariable("IOS_SIM_RUNTIME", "com.apple.CoreSimulator.SimRuntime.iOS-14-4");
var IOS_CONTROLGALLERY = "src/Compatibility/ControlGallery/src/iOS/";
var IOS_CONTROLGALLERY_PROJ = $"{IOS_CONTROLGALLERY}Compatibility.ControlGallery.iOS.csproj";
var IOS_TEST_PROJ = "./src/Compatibility/ControlGallery/test/iOS.UITests/Compatibility.ControlGallery.iOS.UITests.csproj";
var IOS_TEST_LIBRARY = Argument("IOS_TEST_LIBRARY", $"./src/Compatibility/ControlGallery/test/iOS.UITests/bin/{configuration}/Microsoft.Maui.Controls.iOS.UITests.dll");
var IOS_IPA_PATH = Argument("IOS_IPA_PATH", $"./src/Compatibility/ControlGallery/src/iOS/bin/iPhoneSimulator/{configuration}/CompatibilityControlGalleryiOS.app");
var IOS_BUNDLE_ID = "com.microsoft.mauicompatibilitygallery";
var IOS_BUILD_IPA = Argument("IOS_BUILD_IPA", (target == "cg-ios-deploy") ? true : (false || isCIBuild) );
Guid IOS_SIM_UDID = Argument("IOS_SIM_UDID", Guid.Empty);

var UWP_PACKAGE_ID = "0d4424f6-1e29-4476-ac00-ba22c3789cb6";
var UWP_TEST_LIBRARY = GetBuildVariable("UWP_TEST_LIBRARY", $"./src/Compatibility/ControlGallery/test/Xamarin.Forms.Core.Windows.UITests/bin/{configuration}/Xamarin.Forms.Core.Windows.UITests.dll");
var UWP_PFX_PATH = Argument("UWP_PFX_PATH", "Xamarin.Forms.ControlGallery.WindowsUniversal\\Xamarin.Forms.ControlGallery.WindowsUniversal_TemporaryKey.pfx");
var UWP_APP_PACKAGES_PATH = Argument("UWP_APP_PACKAGES_PATH", "*/AppPackages/");
var UWP_APP_DRIVER_INSTALL_PATH = Argument("UWP_APP_DRIVER_INSTALL_PATH", "https://github.com/microsoft/WinAppDriver/releases/download/v1.2-RC/WindowsApplicationDriver.msi");

var ANDROID_BUNDLE_ID = "com.microsoft.mauicompatibilitygallery";
var ANDROID_CONTROLGALLERY = "src/Compatibility/ControlGallery/src/Android/";
var ANDROID_CONTROLGALLERY_PROJ = $"{ANDROID_CONTROLGALLERY}Compatibility.ControlGallery.Android.csproj";
var ANDROID_RENDERERS = Argument("ANDROID_RENDERERS", "FAST");
var ANDROID_TEST_PROJ = "./src/Compatibility/ControlGallery/test/Android.UITests/Compatibility.ControlGallery.Android.UITests.csproj";

var BUILD_TASKS_PROJ ="Microsoft.Maui.BuildTasks.sln";

var XamarinFormsVersion = Argument("XamarinFormsVersion", "");
var packageVersion = GetBuildVariable("packageVersion", "0.1.0-p2");
var releaseChannelArg = Argument("CHANNEL", "Stable");
releaseChannelArg = EnvironmentVariable("CHANNEL") ?? releaseChannelArg;
var teamProject = Argument("TeamProject", "");
bool isHostedAgent = agentName.StartsWith("Azure Pipelines") || agentName.StartsWith("Hosted Agent");

var MAUI_SLN = "./Microsoft.Maui.sln";

var CONTROLGALLERY_SLN = "./ControlGallery.sln";

string defaultUnitTestWhere = "";

if(target.ToLower().Contains("uwp"))
    defaultUnitTestWhere = "cat != UwpIgnore";

var NUNIT_TEST_WHERE = Argument("NUNIT_TEST_WHERE", defaultUnitTestWhere);
NUNIT_TEST_WHERE = ParseDevOpsInputs(NUNIT_TEST_WHERE);

var ANDROID_HOME = EnvironmentVariable("ANDROID_HOME") ??
    (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Android\\android-sdk\\" : "");

string MSBuildArgumentsENV = EnvironmentVariable("MSBuildArguments", "");
string MSBuildArgumentsARGS = Argument("MSBuildArguments", "");
string MSBuildArguments;

MSBuildArguments = $"{MSBuildArgumentsENV} {MSBuildArgumentsARGS}";
    
Information("MSBuildArguments: {0}", MSBuildArguments);

string androidEmulators = EnvironmentVariable("ANDROID_EMULATORS", "");

string androidSdks = EnvironmentVariable("ANDROID_API_SDKS",
    // build/platform tools
    "build-tools;29.0.3," + 
    "build-tools;30.0.2," + 
    "platform-tools," + 
    // apis
    "platforms;android-26," + 
    "platforms;android-27," + 
    "platforms;android-28," + 
    "platforms;android-29," + 
    "platforms;android-30," + 
    // emulators
    androidEmulators);

Information("ANDROID_API_SDKS: {0}", androidSdks);
string[] androidSdkManagerInstalls = androidSdks.Split(',');

(string name, string location, string featureList)[] windowsSdksInstalls = new (string name, string location, string featureList)[]
{
    ("10.0.19041.0", "https://go.microsoft.com/fwlink/p/?linkid=2120843", "OptionId.WindowsPerformanceToolkit OptionId.WindowsDesktopDebuggers OptionId.AvrfExternal OptionId.WindowsSoftwareLogoToolkit OptionId.MSIInstallTools OptionId.SigningTools OptionId.UWPManaged OptionId.UWPCPP OptionId.UWPLocalized OptionId.DesktopCPPx86 OptionId.DesktopCPPx64 OptionId.DesktopCPParm OptionId.DesktopCPParm64"), 
    ("10.0.18362.0", "https://go.microsoft.com/fwlink/?linkid=2083338", "+"),
    ("10.0.16299.0", "https://go.microsoft.com/fwlink/p/?linkid=864422", "+"),
    ("10.0.14393.0", "https://go.microsoft.com/fwlink/p/?LinkId=838916", "+")
};

string[] netFrameworkSdksLocalInstall = new string[]
{
    "https://go.microsoft.com/fwlink/?linkid=2099470", //NET461 SDK
    "https://go.microsoft.com/fwlink/?linkid=874338", //NET472 SDK
    "https://go.microsoft.com/fwlink/?linkid=2099465", //NET47
    "https://download.microsoft.com/download/A/1/D/A1D07600-6915-4CB8-A931-9A980EF47BB7/NDP47-DevPack-KB3186612-ENU.exe", //net47 targeting pack
    "https://go.microsoft.com/fwlink/?linkid=2088517", //NET48 SDK
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
Information ("Agent.Name: {0}", agentName);
Information ("isCIBuild: {0}", isCIBuild);
Information ("artifactStagingDirectory: {0}", artifactStagingDirectory);
Information("workingDirectory: {0}", workingDirectory);
Information("NUNIT_TEST_WHERE: {0}", NUNIT_TEST_WHERE);
Information("TARGET: {0}", target);
Information("MSBUILD: {0}", msbuildPath);


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
    if(IsXcodeVersionAtLeast("12.0"))
    {
    }
    else
    {
        monoMajorVersion = "";
        monoPatchVersion = "";
        iOSSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-7-xcode11.7/3016ffe2b0ee27bf4a2d61e6161430d6bbd62f78/7/package/notarized/xamarin.ios-13.20.3.5.pkg";
    	macSDK_macos = $"https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/d16-7-xcode11.7/3016ffe2b0ee27bf4a2d61e6161430d6bbd62f78/7/package/notarized/xamarin.mac-6.20.3.5.pkg";
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


androidSDK_macos = EnvironmentVariable("ANDROID_SDK_MAC", androidSDK_macos);
iOSSDK_macos = EnvironmentVariable("IOS_SDK_MAC", iOSSDK_macos);
monoSDK_macos = EnvironmentVariable("MONO_SDK_MAC", monoSDK_macos);
macSDK_macos = EnvironmentVariable("MAC_SDK_MAC", macSDK_macos);

androidSDK_windows = EnvironmentVariable("ANDROID_SDK_WINDOWS", "");
iOSSDK_windows = EnvironmentVariable("IOS_SDK_WINDOWS", "");
monoSDK_windows = EnvironmentVariable("MONO_SDK_WINDOWS", "");
macSDK_windows = EnvironmentVariable("MAC_SDK_WINDOWS", "");

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

Task("BuildUnitTests")
    .IsDependentOn("BuildTasks")
    .Description("Builds all necessary projects to run Unit Tests")
    .Does(() =>
{
    try
    {
        var msbuildSettings = GetMSBuildSettings();
        var binaryLogger = new MSBuildBinaryLogSettings {
            Enabled  = isCIBuild
        };

        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/Maui.Controls-{configuration}.binlog";
        MSBuild("./Microsoft.Maui.sln", msbuildSettings.WithRestore());
    }
    catch(Exception)
    {
        if(IsRunningOnWindows())
            throw;
    }
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
            Information("Updating Android SDKs");
            var androidSdkSettings = new AndroidSdkManagerToolSettings {
                SkipVersionCheck = true
            };

            if(!String.IsNullOrWhiteSpace(ANDROID_HOME))            
                androidSdkSettings.SdkRoot = ANDROID_HOME;

            try{
                AcceptLicenses (androidSdkSettings);
            }
            catch(Exception exc)
            {
                Information("AcceptLicenses: {0}", exc);
            }

            try{
                AndroidSdkManagerUpdateAll (androidSdkSettings);
            }
            catch(Exception exc)
            {
                Information("AndroidSdkManagerUpdateAll: {0}", exc);
            }
            
            try{
                AcceptLicenses (androidSdkSettings);
            }
            catch(Exception exc)
            {
                Information("AcceptLicenses: {0}", exc);
            }

            try{
                AndroidSdkManagerInstall (androidSdkManagerInstalls, androidSdkSettings);
            }
            catch(Exception exc)
            {
                Information("AndroidSdkManagerInstall: {0}", exc);
            }
        }

        if (!IsRunningOnWindows ()) {
            if(!String.IsNullOrWhiteSpace(androidSDK))
            {
                await Boots (androidSDK);
            }
            else
                await Boots (Product.XamarinAndroid, releaseChannel);
        }
        else if(!String.IsNullOrWhiteSpace(androidSDK))
        {
            await Boots (androidSDK);
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
                        .Append(@"/features ")
                        .Append(windowsSdk.featureList)
                        .Append(@" /q")
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
                InstallMsiOrExe(msiUrl, null, msiName);
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
            // The platform unit tests can't run when UseDotNetNativeToolchain is set to true so we force it off here
            .WithProperty("UseDotNetNativeToolchain", "false")
            .WithRestore()
    );

    MSBuild("Xamarin.Forms.Core.Windows.UITests\\Xamarin.Forms.Core.Windows.UITests.csproj", 
        GetMSBuildSettings(buildConfiguration:"Debug").WithRestore());
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
    .IsDependentOn("cg-uwp-build-tests")
    .IsDependentOn("cg-uwp-deploy")
    .IsDependentOn("provision-uitests-uwp")
    .IsDependentOn("_cg-uwp-run-tests");

Task("_cg-uwp-run-tests")
    .Does((ctx) =>
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

        var settings = new NUnit3Settings {
            Params = new Dictionary<string, string>()
            {
                {"IncludeScreenShots", "true"}
            }
        };


        try
        {
            RunTests(UWP_TEST_LIBRARY, settings, ctx);
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
    .IsDependentOn("_cg-uwp-run-tests")
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
                InstallMsiOrExe(UWP_APP_DRIVER_INSTALL_PATH, installPath);
            }
            catch(Exception e)
            {
                Information("Failed to Install Win App Driver: {0}", e);
            }
        }
    });


async Task InstallMsiWithBoots(string msiFile, string installTo = null, string fileName = "InstallFile.msi")
{
    bool success = false;

    try
    {
        await Boots(msiFile);
        success = true;
    }
    catch (System.Exception e)
    {
        Information("Boots failed: {0}", e);
    }


    if(success)
        return;

    try
    {
        InstallMsiOrExe(msiFile, installTo, fileName, !isCIBuild);
        success = true;
    }
    catch (System.Exception e)
    {
        Information("Our attempt failed: {0}", e);
    }
}

void InstallMsiOrExe(string msiFile, string installTo = null, string fileName = "InstallFile.msi", bool interactive = false)
{
     if(msiFile.EndsWith(".exe") && fileName == "InstallFile.msi")
        fileName = "InstallFile.exe";

    string installerPath = $"{System.IO.Path.GetTempPath()}{fileName}";
        
    try
    {
        Information ("Installing: {0}", msiFile);
        DownloadFile(msiFile, installerPath);
        Information("File Downloaded To: {0}", installerPath);
        int result = -1;

        if(msiFile.EndsWith(".exe"))
        {
            result = StartProcess(installerPath, new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append(@" /q")
                    }
                );
        }
        else{
            var argumentBuilder = 
                new ProcessArgumentBuilder()
                    .Append("/a")
                    .Append(installerPath);

            if(!interactive)
                argumentBuilder = argumentBuilder.Append("/qn");

            if(!String.IsNullOrWhiteSpace(installTo))
            {
                Information("Installing into: {0}", installTo);
                argumentBuilder = argumentBuilder.Append("TARGETDIR=\"" + installTo + "\"");
            }

            result = StartProcess("msiexec", new ProcessSettings {
                Arguments = argumentBuilder
            });
        }

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

Task("provision-powershell").Does(()=> {
    var settings = new DotNetCoreToolSettings
    {
        DiagnosticOutput = true,
        ArgumentCustomization = args=>args.Append("install --global PowerShell")
    };

    DotNetCoreTool("tool", settings);
});

Task("Restore")
    .Description($"Restore target on {MAUI_SLN}")
    .Does(() =>
    {
        try{
            MSBuild(MAUI_SLN, GetMSBuildSettings().WithTarget("restore"));
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
            System.IO.File.WriteAllText($"{ANDROID_CONTROLGALLERY}/Properties/MapsKey.cs", "[assembly: Android.App.MetaData(\"com.google.android.maps.v2.API_KEY\", Value = \"" + GoogleMapsAPIKey + "\")]");
        }
    });

Task("BuildTasks")
    .Description($"Build {BUILD_TASKS_PROJ}")
    .Does(() =>
{
    MSBuild(BUILD_TASKS_PROJ, GetMSBuildSettings().WithRestore());
});

Task("Build")
    .Description("Builds all necessary projects to run Control Gallery")
    .IsDependentOn("Restore")
    .Does(() =>
{
    try{
        MSBuild(MAUI_SLN, GetMSBuildSettings().WithRestore());
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
        MSBuild(MAUI_SLN,
                GetMSBuildSettings()
                    .WithRestore()
                    .WithProperty("AndroidTargetFrameworks", "MonoAndroid10.0"));
    });

Task("VS")
    .Description("Builds projects necessary so solution compiles on VS")
    .IsDependentOn("Clean")
    .IsDependentOn("VSMAC")
    .IsDependentOn("VSWINDOWS");

Task("VS-CG")
    .Description("Builds projects necessary so solution compiles on VS")
    .IsDependentOn("Clean")
    .IsDependentOn("VSMAC")
    .IsDependentOn("VSWINDOWS");


Task("VSWINDOWS")
    .Description("Builds projects necessary so solution compiles on VS Windows")
    .IsDependentOn("BuildTasks")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
    {
        string sln = "Microsoft.Maui.sln";
        if (target == "VS-CG")
            sln = "Compatibility.ControlGallery.sln";

        MSBuild(sln,
                GetMSBuildSettings()
                    .WithRestore());

        StartVisualStudio(sln);
    });

Task("VSMAC")
    .Description("Builds projects necessary so solution compiles on VSMAC")
    .WithCriteria(!IsRunningOnWindows())
    .IsDependentOn("BuildTasks")
    .Does(() =>
    {
        
        string sln = "Microsoft.Maui.sln";
        if (target == "VS-CG")
            sln = "Compatibility.ControlGallery.sln";

        MSBuild("src/Core/src/Core.csproj",
                GetMSBuildSettings()
                    .WithRestore());

        MSBuild("src/Controls/samples/Controls.Sample.Droid/Controls.Sample.Droid.csproj",
                GetMSBuildSettings()
                    .WithRestore());
        

        MSBuild("src/Controls/samples/Controls.Sample.iOS/Controls.Sample.iOS.csproj",
            new MSBuildSettings().WithRestore());

        MSBuild("src/Essentials/src/Essentials.csproj",
                GetMSBuildSettings()
                    .WithRestore());
                    
        MSBuild("src/SingleProject/Resizetizer/src/Resizetizer.csproj", GetMSBuildSettings().WithRestore());

        StartVisualStudio(sln);
    });
    
Task("cg-android")
    .Description("Builds Android Control Gallery")
    .IsDependentOn("WriteGoogleMapsAPIKey")
    .IsDependentOn("BuildTasks")
    .Does(() => 
    {
        var buildSettings = GetMSBuildSettings();

        buildSettings = buildSettings.WithRestore();

        if(isCIBuild)
        {
            buildSettings = buildSettings.WithTarget("Rebuild").WithTarget("SignAndroidPackage");
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true
            };

            buildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/android-{ANDROID_RENDERERS}.binlog";
        }

        MSBuild(ANDROID_CONTROLGALLERY_PROJ, buildSettings);
    });

Task("cg-android-build-tests")
    .IsDependentOn("BuildTasks")
    .Does(() =>
    {
        var buildSettings =  GetMSBuildSettings();

        buildSettings = buildSettings.WithRestore();

        if(isCIBuild)
        {
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true,
                FileName = $"{artifactStagingDirectory}/android-uitests.binlog"
            };

            buildSettings.BinaryLogger = binaryLogger;
        }

        MSBuild(ANDROID_TEST_PROJ, buildSettings);
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

        buildSettings = buildSettings.WithRestore();

        if(isCIBuild)
        {
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true
            };
            
            buildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/ios-cg.binlog";
        }

        MSBuild(IOS_CONTROLGALLERY_PROJ, buildSettings);
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
                .WithProperty("CI", $"true")
                .WithRestore();

        if(isCIBuild)
        {
            var binaryLogger = new MSBuildBinaryLogSettings {
                Enabled  = true,
                FileName = $"{artifactStagingDirectory}/ios-uitests.binlog"
            };

            buildSettings.BinaryLogger = binaryLogger;
        }

        MSBuild(IOS_TEST_PROJ, buildSettings);
    });

Task("cg-ios-run-tests")
    .IsDependentOn("cg-ios-build-tests")
    .IsDependentOn("cg-ios-deploy")
    .IsDependentOn("_cg-ios-run-tests");

Task("_cg-ios-run-tests")
    .Does((ctx) =>
    {
        var sim = GetIosSimulator();

        var settings = new NUnit3Settings {
                Params = new Dictionary<string, string>()
                {
                    {"UDID", GetIosSimulator().UDID},
                    {"IncludeScreenShots", "true"}
                }
            };

        if(isCIBuild)
        {
            Information("defaults write com.apple.CrashReporter DialogType none");
            IEnumerable<string> redirectedStandardOutput;
            StartProcess("defaults", 
                new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder().Append(@"write com.apple.CrashReporter DialogType none"),
                    RedirectStandardOutput = true
                },
                out redirectedStandardOutput
            );


            foreach (var item in redirectedStandardOutput)
            {
                Information(item);
            }
        }

        RunTests(IOS_TEST_LIBRARY, settings, ctx);
    });

Task("cg-ios-run-tests-ci")
    .IsDependentOn("cg-ios-deploy")
    .IsDependentOn("_cg-ios-run-tests")
    .Does(() =>
    {
    });

Task ("cg-ios-deploy")
    .Does (() =>
{
    // Look for a matching simulator on the system
    var sim = GetIosSimulator();

    //ShutdownAndResetiOSSimulator(sim);

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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("dotnet-pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

void RunTests(string unitTestLibrary, NUnit3Settings settings, ICakeContext ctx)
{
    try
    {
        if(!String.IsNullOrWhiteSpace(NUNIT_TEST_WHERE))
        {
            settings.Where = NUNIT_TEST_WHERE;
        }

        NUnit3(new [] { unitTestLibrary }, settings);
    }
    catch
    {
        SetTestResultsEnvironmentVariables();
        throw;
    }

    SetTestResultsEnvironmentVariables();

    void SetTestResultsEnvironmentVariables()
    {
        var doc = new System.Xml.XmlDocument();
        doc.Load("TestResult.xml");
        var root = doc.DocumentElement;

        foreach(System.Xml.XmlAttribute attr in root.Attributes)
        {
            SetEnvironmentVariable($"NUNIT_{attr.Name}", attr.Value, ctx);
        }
    }
}

void StartVisualStudio(string sln = "./Microsoft.Maui.sln")
{
    if(isCIBuild)
        return;

    if(IsRunningOnWindows())
    {
        StartProcess("powershell",
            new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("start")
                    .Append(sln)
            });
    }
    else
         StartProcess("open", new ProcessSettings{ Arguments = sln });
}

MSBuildSettings GetMSBuildSettings(
    PlatformTarget? platformTarget = PlatformTarget.MSIL, 
    string buildConfiguration = null,
    bool includePrerelease = false)
{
    var buildSettings =  new MSBuildSettings {
        PlatformTarget = platformTarget,
        MSBuildPlatform = Cake.Common.Tools.MSBuild.MSBuildPlatform.x86,
        Configuration = buildConfiguration ?? configuration,
    };


    if(IsRunningOnWindows())
    {
        var vsInstallation =
            VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.Component.MSBuild", IncludePrerelease = includePrerelease })
            ?? VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.Component.MSBuild" });

        if (vsInstallation != null)
        {
            buildSettings.ToolPath = vsInstallation.CombineWithFilePath(@"MSBuild\Current\Bin\MSBuild.exe");
            if (!FileExists(buildSettings.ToolPath))
                buildSettings.ToolPath = vsInstallation.CombineWithFilePath(@"MSBuild\15.0\Bin\MSBuild.exe");
        }
    }

    buildSettings = buildSettings.WithProperty("ANDROID_RENDERERS", $"{ANDROID_RENDERERS}");
    if(!String.IsNullOrWhiteSpace(XamarinFormsVersion))
    {
        buildSettings = buildSettings.WithProperty("XamarinFormsVersion", XamarinFormsVersion);
    }
    
    buildSettings.ArgumentCustomization = args => args.Append($"/nowarn:VSX1000 {MSBuildArguments}");
    return buildSettings;
}

bool IsXcodeVersionAtLeast(string version)
{
    if(IsRunningOnWindows())
        return true;

    return XcodeVersion() >= Version.Parse(version); 
}

Version XcodeVersion()
{
    if(IsRunningOnWindows())
        return null;

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

            return xcodeVersion; 
        }
    }

    return null;
}

IReadOnlyList<AppleSimulator> iosSimulators = null;

void ShutdownAndResetiOSSimulator(AppleSimulator sim)
{
    //close all simulators , reset needs simulator to be closed
    Information("Shutdown simulators: {0} ({1} - {2}) State: {3}", sim.Name, sim.Runtime, sim.UDID, sim.State);
    ShutdownAllAppleSimulators();

    var shutdown = false;
    for (int i = 0; i < 100; i++) {
        if (ListAppleSimulators().Any (s => s.UDID == sim.UDID && s.State.ToLower().Contains("shutdown"))) {
            shutdown = true;
            break;
        }
        System.Threading.Thread.Sleep(1000);
    }

    //Reset the simulator
    Information ("Factory reset simulator: {0}", sim.UDID);
    EraseAppleSimulator(sim.UDID);
}

AppleSimulator GetIosSimulator()
{
    if(iosSimulators == null)
    {
        iosSimulators = ListAppleSimulators ();
        foreach (var s in iosSimulators)
        {
            Information("Info: {0} ({1} - {2} - {3})", s.Name, s.Runtime, s.UDID, s.Availability);
        }

        StartProcess("xcrun", new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append(@"simctl list")
                    }
                );
    }
        
    if(IOS_SIM_UDID != Guid.Empty)
        return iosSimulators.First (s => Guid.Parse(s.UDID) == IOS_SIM_UDID);

    // Look for a matching simulator on the system
    return iosSimulators.First (s => s.Name == IOS_SIM_NAME && s.Runtime == IOS_SIM_RUNTIME);
}

public void PrintEnvironmentVariables()
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
    };
}

public void SetEnvironmentVariable(string key, string value, ICakeContext context)
{
    var buildSystem = context.BuildSystem();
    Information("Setting: {0} to {1}", key, value);
    if(isCIBuild)
    {
        buildSystem.AzurePipelines.Commands.SetVariable(key, value);
    }
    else
    {
        System.Environment.SetEnvironmentVariable(key, value);
    }
}

public string ParseDevOpsInputs(string nunitWhere)
{
    var ExcludeCategory = GetBuildVariable("ExcludeCategory", "")?.Replace("\"", "");
    var ExcludeCategory2 = GetBuildVariable("ExcludeCategory2", "")?.Replace("\"", "");
    var IncludeCategory = GetBuildVariable("IncludeCategory", "")?.Replace("\"", "");

    Information("ExcludeCategory: {0}", ExcludeCategory);
    Information("IncludeCategory: {0}", IncludeCategory);
    Information("ExcludeCategory2: {0}", ExcludeCategory2);
    string excludeString = String.Empty;
    string includeString = String.Empty;
    string returnValue = String.Empty;

    List<string> azureDevopsFilters = new List<string>();

    // Replace Azure devops syntax for unit tests to Nunit3 filters
    if(!String.IsNullOrWhiteSpace(ExcludeCategory))
    {
        azureDevopsFilters.AddRange(ExcludeCategory.Split(new string[] { "--exclude-category" }, StringSplitOptions.None));
    }

    if(!String.IsNullOrWhiteSpace(ExcludeCategory2))
    {
        azureDevopsFilters.AddRange(ExcludeCategory2.Split(new string[] { "--exclude-category" }, StringSplitOptions.None));
    }

    for(int i = 0; i < azureDevopsFilters.Count; i++)
    {
        if(!String.IsNullOrWhiteSpace(excludeString))
            excludeString += " && ";

        excludeString += $" cat != {azureDevopsFilters[i]} ";
    }

    String.Join(" cat != ", azureDevopsFilters);

    if(!String.IsNullOrWhiteSpace(IncludeCategory))
    { 
        foreach (var item in IncludeCategory.Split(new string[] { "--include-category" }, StringSplitOptions.None))
        {
            if(!String.IsNullOrWhiteSpace(includeString))
                includeString += " || ";

            includeString += $" cat == {item} ";
        }
    }

    foreach(var filter in new []{nunitWhere,includeString,excludeString}.Where(x=> !String.IsNullOrWhiteSpace(x)))
    {
        if(!String.IsNullOrWhiteSpace(returnValue))
            returnValue += " && ";

        returnValue += $"({filter})";
    }

    return returnValue;
}
