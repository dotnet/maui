
<img src="banner.png" alt="Xamarin.Forms banner" height="145" >

# Xamarin.Forms #

Xamarin.Forms provides a way to quickly build native apps for iOS, Android, Windows and macOS, completely in C#.

Read more about the platform at https://www.xamarin.com/forms.

## Build Status ##

![Azure DevOps](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/Xamarin/XamarinForms/Xamarin%20Forms?branchName=master "Azure Pipelines")

## Packages ##

Platform/Feature               | Package name                              | Stable      | Prerelease | Nightly Feed [Azure](https://aka.ms/xf-ci/index.json)  (master branch)
-----------------------|-------------------------------------------|-----------------------------|------------------------- |-------------------------|
Core             | `Xamarin.Forms` | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms/) |
AppLinks  | `Xamarin.Forms.AppLinks`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.AppLinks.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.AppLinks/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.AppLinks.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.AppLinks/) | 
Maps                 | `Xamarin.Forms.Maps`    | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Maps.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Maps.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps/) |
Maps.GTK             | `Xamarin.Forms.Maps.GTK` | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Maps.GTK.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps.GTK/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Maps.GTK.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps.GTK/) |
Maps.WPF             | `Xamarin.Forms.Maps.WPF` | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Maps.WPF.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps.WPF/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Maps.WPF.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps.WPF/) |
Pages  | `Xamarin.Forms.Pages`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Pages.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Pages/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Pages.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Pages/) |
Pages.Azure  | `Xamarin.Forms.Pages.Azure`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Pages.Azure.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Pages.Azure/) |[![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Pages.Azure.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Pages.Azure/) |
Platform.GTK  | `Xamarin.Forms.Platform.GTK`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Platform.GTK.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Platform.GTK/) |[![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Platform.GTK.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Platform.GTK/) |
Platform.WPF  | `Xamarin.Forms.Platform.WPF`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Platform.WPF.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Platform.WPF/) |[![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Platform.WPF.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Platform.WPF/) |
Visual.Material  | `Xamarin.Forms.Visual.Material`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Visual.Material.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Visual.Material/) | [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Forms.Visual.Material.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Visual.Material/) |

If you want to use the latest dev build then you should read [this blog post](https://devblogs.microsoft.com/xamarin/try-the-latest-in-xamarin-forms-with-nightly-builds):

- Add the nightly feed to your NuGet sources or add a NuGet.Config to your app (placing it in the same directory where your solution file is) with the following content:

  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <packageSources>
      <clear />
      <add key="xamarin-ci" value="https://aka.ms/xf-ci/index.json" />
      <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
  </configuration>
  ```

  *NOTE: This NuGet.Config should be with your application unless you want nightly packages to potentially start being restored for other apps on the machine.*

- Change your application's dependencies to have a `*` to get the latest version.

## Getting Started ##

### Windows ###
##### Install Visual Studio 2019+ #####

VS 2019+ is required for developing Xamarin.Forms. If you do not already have it installed, you can download it [here](https://www.visualstudio.com/downloads/download-visual-studio-vs). VS 2019+ Community is completely free. If you are installing VS 2019+ for the first time, select the "Custom" installation type and select the following from the features list to install:

- .NET desktop development - In the `Summary > Optional select .NET Framework 4.7 SDK, .NET Framework 4.7 targeting pack`. 
- Universal Windows Platform Development - In the `Summary > Optional select the Windows 10 Mobile Emulator`.
- Mobile Development with .NET - In the `Summary > Optional select Xamarin Remoted Simulator, Xamarin SDK Manager, Intel Hardware Accelerated Execution Manager (HAXM)`

The Android 10.0 API 29 SDK is required for developing Xamarin.Forms. It can be installed by using the [Xamarin Android SDK Manager](https://docs.microsoft.com/xamarin/android/get-started/installation/android-sdk).

We also recommend installing [Xamarin Android Device Manager](https://developer.xamarin.com/guides/android/getting_started/installation/android-emulator/xamarin-device-manager/) This will use the HAXM tools installed above and allow you to configure Android Virtual Devices (AVDs) that emulate Android devices.
If you already have VS 2019+ installed, you can verify that these features are installed by modifying the VS 2019+ installation via the Visual Studio Installer.

### Mac ###
#### Install Visual Studio for Mac 2019 ####

If you do not already have it installed, instructions to download and setup can be found [here](https://docs.microsoft.com/en-us/visualstudio/mac/installation?view=vsmac-2019).

Because of current Multi-Targeting limitations with Visual Studio for Mac you will need to manually build/restore some projects before you are able to work on the Xamarin Forms solution.

Here are a few different options we've put together to help make this process easier
- Branches 3.5+ come with a Cake script target that you can use to build and open VSMac
```sh
./build.sh --target vsmac
```
- When working on an earlier branch that does not have the cake scripts then you can use the following [build.sh] script(https://gist.github.com/PureWeen/92c1e1aff0c257c3decf0bcb8d6e9296)

- If you don't want to run any scripts then
  - Open Xamarin.Forms.sln
  - Wait for VSMAC to finish restoring all projects
  - from the command line run:
    - `msbuild Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.csproj`
  - Now you should be able to run and deploy everything. The only reason you would need to do this process again is if you clean the solution folder or delete the bin/obj folders that are part of the `Xamarin.Forms.Build.Tasks.csproj`

If you are on Visual Studio for Mac 2017 you will need to turn off automatic package restore (Visual Studio => Preferences => Nuget => General => uncheck the Package Restore box) before working on the Xamarin.Forms solution. This step is no longer needed with Visual Studio for Mac 2019

##### Solution Configuration #####

Upon opening the Xamarin.Forms solution, you will find that there are a number of errors and warnings under the Error List pane; you can resolve this by changing the filter of `Build + IntelliSense` to `Build Only`. At this point, you should be able to successfully build the solution.

By default, the `Xamarin.Forms.Controls` project does not have a configuration for various API keys to access certain features on each platform (e.g. maps). When building the solution the first time, a `controlgallery.config` file will be generated inside that project, which looks like this:

    UWPMapsAuthKey:

If you aren't working with maps, you can ignore this. If you want to work with maps, you will have to obtain your own API keys for each of these services, inserted directly after the identifier (e.g. `UWPMapsAuthKey:abcdefghijklmnopqrstuvwxyz`). You can find out how to obtain each of these as follows:

- `UWPMapsAuthKey` at https://microsoft.com/maps/create-a-bing-maps-key.aspx

Due to the way that Android works, the maps API key cannot be injected at runtime. As a result, you will have to add this key to the `MapsKey.cs` file under `Xamarin.Forms.ControlGallery.Android/Properties`:

    [assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "INSERT_KEY_HERE")]

You can find out how to obtain a Google Maps API key [here](https://developer.xamarin.com/guides/android/platform_features/maps_and_location/maps/obtaining_a_google_maps_api_key/).

##### Build from the Command line #####

Make sure you have nuget.exe 4.0 or above and the latest dotnet core sdk (2.0.3). On macOS you should specify the platform in the msbuild command (`/p:Platform=iPhoneSimulator`)

    msbuild /restore Xamarin.Forms.sln

### UI Tests ###

##### Run Android UI Tests #####

Depending on your environment setup, you might need to configure a few things before being able to debug / run UI tests, especially on Windows.

- If you receive an error about ANDROID_HOME, please make sure to set your environment variable to the Android SDK directory (e.g. C:\Program Files (x86)\Android\android-sdk).
- If you receive an error about JAVA_HOME, please install the latest Java JDK and set your environment variable to the JDK directory (e.g. C:\Program Files\Java\jdk-13).
- If you receive an error about a missing ApkFile, please generate an APK file for Xamarin.Forms.ControlGallery.Android. The easiest way to do this is to right click the project and select "Deploy". Note that if you rebuild the solution, you might lose the APK and will need to generate it again.

After these steps are taken care of, you should be good to go. You can see all UI tests in Test Explorer, search them for your own convenience, and quickly run individual tests.

##### Run UWP UI Tests #####

To run the UWP UI Tests:

1. Install and run the [Windows Application Driver](https://github.com/Microsoft/WinAppDriver#installing-and-running-windows-application-driver).
2. Launch the `Xamarin.Forms.ControlGallery.WindowsUniversal` project to install the ControlGallery application onto your system.

You should now be able to run any of the UWP UI Tests.

## Coding Style ##

We follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with a few exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We use hard tabs over spaces. You can change this setting in VS 2015 via `Tools > Options` and navigating to `Text Editor > C#` and selecting the "Keep tabs" radio option. In Visual Studio for Mac it's set via preferences in `Source Code > Code Formatting > C# source code` and disabling the checkbox for `Convert tabs to spaces`.
- Lines should be limited to a max of 120 characters (or as close as possible within reason). This may be set in Visual Studio for Mac via preferences in `Source Code > Code Formatting > C# source code` and changing the `Desired file width` to `120`.

## Contributing ##

- [How to Contribute](https://github.com/xamarin/Xamarin.Forms/blob/master/.github/CONTRIBUTING.md)

### Reporting Bugs ###

We use [GitHub Issues](https://github.com/xamarin/Xamarin.Forms/issues) to track issues. If at all possible, please submit a [reproduction of your bug](https://gist.github.com/jassmith/92405c300e54a01dcc6d) along with your bug report.
