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
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

var IOS_SIM_NAME = GetBuildVariable("IOS_SIM_NAME", "iPhone 7");
var IOS_SIM_RUNTIME = GetBuildVariable("IOS_SIM_RUNTIME", "com.apple.CoreSimulator.SimRuntime.iOS-12-4");
var IOS_TEST_PROJ = "./Xamarin.Forms.Core.iOS.UITests/Xamarin.Forms.Core.iOS.UITests.csproj";
var IOS_TEST_LIBRARY = Argument("IOS_TEST_LIBRARY", $"./Xamarin.Forms.Core.iOS.UITests/bin/{configuration}/Xamarin.Forms.Core.iOS.UITests.dll");
var IOS_IPA_PATH = Argument("IOS_IPA_PATH", $"./Xamarin.Forms.ControlGallery.iOS/bin/iPhoneSimulator/{configuration}/XamarinFormsControlGalleryiOS.app");
var IOS_BUNDLE_ID = "com.xamarin.quickui.controlgallery";
var IOS_BUILD_IPA = Argument("IOS_BUILD_IPA", (target == "cg-ios-deploy") ? true : (false || isCIBuild) );
Guid IOS_SIM_UDID = Argument("IOS_SIM_UDID", Guid.Empty);

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
bool isHostedAgent = agentName.StartsWith("Azure Pipelines") || agentName.StartsWith("Hosted Agent");

string defaultUnitTestWhere = "";

if(target.ToLower().Contains("uwp"))
    defaultUnitTestWhere = "cat != Shell && cat != UwpIgnore";

var NUNIT_TEST_WHERE = Argument("NUNIT_TEST_WHERE", defaultUnitTestWhere);
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

string MSBuildArgumentsENV = EnvironmentVariable("MSBuildArguments", "");
string MSBuildArgumentsARGS = Argument("MSBuildArguments", "");
string MSBuildArguments;

MSBuildArguments = $"{MSBuildArgumentsENV} {MSBuildArgumentsARGS}";
    
Information("MSBuildArguments: {0}", MSBuildArguments);

string androidSdks = EnvironmentVariable("ANDROID_API_SDKS", "platform-tools,platforms;android-28,platforms;android-29,build-tools;29.0.3,platforms;android-30,build-tools;30.0.2");

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

Task("Clean")
    .Description("Deletes all the obj/bin directories")
    .Does(() =>
{
    CleanDirectories("./**/obj", (fsi)=> !fsi.Path.FullPath.StartsWith("tools"));
    CleanDirectories("./**/bin", (fsi)=> !fsi.Path.FullPath.StartsWith("tools"));
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

        try
        {
            var settings = new NUnit3Settings {
                Params = new Dictionary<string, string>()
                {
                    {"IncludeScreenShots", "true"}
                }
            };

            if(!String.IsNullOrWhiteSpace(NUNIT_TEST_WHERE))
            {
                settings.Where = NUNIT_TEST_WHERE;
            }

            NUnit3(new [] { UWP_TEST_LIBRARY }, settings);
        }
        catch
        {
            SetEnvironmentVariables();
            throw;
        }
        finally
        { 
            try
            {
                process?.Kill();
            }
            catch{}
        }

        SetEnvironmentVariables();

        void SetEnvironmentVariables()
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load("TestResult.xml");
            var root = doc.DocumentElement;

            foreach(System.Xml.XmlAttribute attr in root.Attributes)
            {
                SetEnvironmentVariable($"NUNIT_{attr.Name}", attr.Value, ctx);
            }
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
    .WithCriteria(IsRunningOnWindows())
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
        MSBuild("./Xamarin.Forms.sln", msbuildSettings.WithRestore());
        
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

        if(IsRunningOnWindows())
        {
            msbuildSettings = GetMSBuildSettings();
            msbuildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/dualscreen-{configuration}-csproj.binlog";
            MSBuild("./Xamarin.Forms.DualScreen/Xamarin.Forms.DualScreen.csproj",
                        msbuildSettings
                            .WithRestore()
                            .WithTarget("rebuild"));


	        msbuildSettings = GetMSBuildSettings();
	        msbuildSettings.BinaryLogger = binaryLogger;
	        binaryLogger.FileName = $"{artifactStagingDirectory}/win-maps-{configuration}-csproj.binlog";
	        MSBuild("./Xamarin.Forms.Maps.UWP/Xamarin.Forms.Maps.UWP.csproj",
	                    msbuildSettings
	                        .WithProperty("UwpMinTargetFrameworks", "uap10.0.14393")
	                        .WithRestore());
	
	        msbuildSettings = GetMSBuildSettings();
	        msbuildSettings.BinaryLogger = binaryLogger;
	        binaryLogger.FileName = $"{artifactStagingDirectory}/win-16299-{configuration}-csproj.binlog";
	        MSBuild("./Xamarin.Forms.Platform.UAP/Xamarin.Forms.Platform.UAP.csproj",
	                    msbuildSettings
	                        .WithRestore()
	                        .WithTarget("rebuild")
	                        .WithProperty("DisableEmbeddedXbf", "false")
	                        .WithProperty("EnableTypeInfoReflection", "false")
	                        .WithProperty("UwpMinTargetFrameworks", "uap10.0.16299"));
	
	        msbuildSettings = GetMSBuildSettings();
	        msbuildSettings.BinaryLogger = binaryLogger;
	        binaryLogger.FileName = $"{artifactStagingDirectory}/win-14393-{configuration}-csproj.binlog";
	        MSBuild("./Xamarin.Forms.Platform.UAP/Xamarin.Forms.Platform.UAP.csproj",
	                    msbuildSettings
	                        .WithRestore()
	                        .WithTarget("rebuild")
	                        .WithProperty("DisableEmbeddedXbf", "false")
	                        .WithProperty("EnableTypeInfoReflection", "false")
	                        .WithProperty("UwpMinTargetFrameworks", "uap10.0.14393"));

            msbuildSettings = GetMSBuildSettings();
            msbuildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/ios-{configuration}-csproj.binlog";
            MSBuild("./Xamarin.Forms.Platform.iOS/Xamarin.Forms.Platform.iOS.csproj",
                        msbuildSettings
                            .WithTarget("rebuild"));

            msbuildSettings = GetMSBuildSettings();
            msbuildSettings.BinaryLogger = binaryLogger;
            binaryLogger.FileName = $"{artifactStagingDirectory}/macos-{configuration}-csproj.binlog";
            MSBuild("./Xamarin.Forms.Platform.MacOS/Xamarin.Forms.Platform.MacOS.csproj",
                        msbuildSettings
                            .WithTarget("rebuild"));
        }

    }
    catch(Exception)
    {
        if(IsRunningOnWindows())
            throw;
    }
});

Task("BuildPages")
    .IsDependentOn("BuildTasks")
    .Description("Build Xamarin.Forms.Pages")
    .Does(() =>
{
    try
    {
        var msbuildSettings = GetMSBuildSettings();
        var binaryLogger = new MSBuildBinaryLogSettings {
            Enabled  = isCIBuild
        };

        msbuildSettings.BinaryLogger = binaryLogger;
        binaryLogger.FileName = $"{artifactStagingDirectory}/win-pages-{configuration}.binlog";
        MSBuild("./build/Xamarin.Forms.Pages.sln", msbuildSettings.WithRestore());

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
                    .WithProperty("AndroidTargetFrameworks", "MonoAndroid10.0"));
    });

Task("VSMAC")
    .Description("Builds projects necessary so solution compiles on VSMAC")
    .IsDependentOn("BuildTasks")
    .Does(() =>
    {
        StartVisualStudio();
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
            binaryLogger.FileName = $"{artifactStagingDirectory}/android-{ANDROID_RENDERERS}.binlog";
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
            binaryLogger.FileName = $"{artifactStagingDirectory}/ios-cg.binlog";
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
    .Does(() =>
    {
        var sim = GetIosSimulator();

        var settings = new NUnit3Settings {
                Params = new Dictionary<string, string>()
                {
                    {"UDID", GetIosSimulator().UDID},
                    {"IncludeScreenShots", "true"}
                }
            };

        if(!String.IsNullOrWhiteSpace(NUNIT_TEST_WHERE))
        {
            settings.Where = NUNIT_TEST_WHERE;
        }

        NUnit3(new [] { IOS_TEST_LIBRARY }, settings);
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


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NugetPack")
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

    buildSettings = buildSettings.WithProperty("ANDROID_RENDERERS", $"{ANDROID_RENDERERS}");
    if(!String.IsNullOrWhiteSpace(XamarinFormsVersion))
    {
        buildSettings = buildSettings.WithProperty("XamarinFormsVersion", XamarinFormsVersion);
    }
    
    if(isCIBuild)
    {
        buildSettings = buildSettings.WithProperty("RestoreConfigFile", $"DevopsNuget.config");
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
