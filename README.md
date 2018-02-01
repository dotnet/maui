<img src="banner.png" alt="Xamarin.Forms banner" height="145" >

# Xamarin.Forms #

Xamarin.Forms provides a way to quickly build native apps for iOS, Android, Windows and macOS, completely in C#.

Read more about the platform at https://www.xamarin.com/forms.

## Build Status ##

![OSX Debug Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_OsxDebug.svg?style=flat&label=OSX%20Debug%20%20%20%20%20 "OSX Debug")  

![Windows Debug Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_WindowsDebug.svg?style=flat&label=Win%20Debug%20%20%20%20%20%20 "Win Debug")  

![Android UI Tests Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_UiTests_OsxTestCloudPackageRunAndroid601.svg?style=flat&label=UITest%20Android "Android UI Tests")

![iOS8 UI Tests Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_UiTests_OsxTestCloudPackageRunIOSUnifiedIOS8.svg?style=flat&label=UITest%20iOS8%20%20%20%20 "iOS8 UI Tests")  

![iOS9 UI Tests Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_UiTests_OsxTestCloudPackageRunIOSUnifiedIOS9.svg?style=flat&label=UITest%20iOS9%20%20%20%20 "iOS9 UI Tests")  

![iOS10 UI Tests Teamcity](https://img.shields.io/teamcity/https/teamcity.xamarin.com/e/XamarinForms_Debug_Cycle8ezTest_UiTests_OsxTestCloudPackageRunIOSUnifiedIOS10.svg?style=flat&label=UITest%20iOS10%20%20 "iOS10 UI Tests") 

![Visual Studio Team Services Windows Debug](https://img.shields.io/vso/build/devdiv/0bdbc590-a062-4c3f-b0f6-9383f67865ee/6713.svg?style=flat&label=VSTS%20Win%20dbg "Win Debug VSTS")

![Visual Studio Team Services OSX Debug](https://img.shields.io/vso/build/devdiv/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5514.svg?style=flat&label=VSTS%20OSX%20dbg "OSX Debug VSTS") 


## Getting Started ##

##### Install Visual Studio 2017 #####
VS 2017 is required for developing Xamarin.Forms. If you do not already have it installed, you can download it [here](https://www.visualstudio.com/downloads/download-visual-studio-vs). VS 2017 Community is completely free. If you are installing VS 2017 for the first time, select the "Custom" installation type and select the following from the features list to install:

- Universal Windows Platform Development - In the `Summary > Optional select the Windows 10 Mobile Emulator`.
- Mobile Development with .NET - In the `Summary > Optional select Xamarin Remoted Simulator, Xamarin SDK Manager, Intel Hardware Accelerated Execution Manager (HAXM)`

We also recommend installing [Xamarin Android Device Manager](https://developer.xamarin.com/guides/android/getting_started/installation/android-emulator/xamarin-device-manager/) This will use the HAXM tools installed above and allow you to configure Android Virtual Devices (AVDs) that emulate Android devices.
If you already have VS 2017 installed, you can verify that these features are installed by modifying the VS 2017 installation via the Visual Studio Installer.

##### Solution Configuration #####
Upon opening the Xamarin.Forms solution, you will find that there are a number of errors and warnings under the Error List pane; you can resolve this by changing the filter of `Build + IntelliSense` to `Build Only`. At this point, you should be able to successfully build the solution.

By default, the `Xamarin.Forms.Controls` project does not have a configuration for various API keys to access certain features on each platform (e.g. maps). When building the solution the first time, a `controlgallery.config` file will be generated inside that project, which looks like this:

    UWPMapsAuthKey:

You will have to obtain your own API keys for each of these services, inserted directly after the identifier (e.g. `UWPMapsAuthKey:abcdefghijklmnopqrstuvwxyz`). You can find out how to obtain each of these as follows:

- `UWPMapsAuthKey` at https://www.microsoft.com/maps/create-a-bing-maps-key.aspx

Due to the way that Android works, the maps API key cannot be injected at runtime. As a result, you will have to add this key to the `MapsKey.cs` file under `Xamarin.Forms.ControlGallery.Android/Properties`:

    [assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "INSERT_KEY_HERE")]

You can find out how to obtain a Google Maps API key [here](https://developer.xamarin.com/guides/android/platform_features/maps_and_location/maps/obtaining_a_google_maps_api_key/).

## Coding Style ##
We follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with a few exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We use hard tabs over spaces. You can change this setting in VS 2015 via `Tools > Options` and navigating to `Text Editor > C#` and selecting the "Keep tabs" radio option. In Visual Studio for Mac it's set via preferences in `Source Code > Code Formatting > C# source code` and disabling the checkbox for `Convert tabs to spaces`.
- Lines should be limited to a max of 120 characters (or as close as possible within reason). This may be set in Visual Studio for Mac via preferences in `Source Code > Code Formatting > C# source code` and changing the `Desired file width` to `120`.

## Contributing ##

- [How to Contribute](https://github.com/xamarin/Xamarin.Forms/blob/master/CONTRIBUTING.md)

### Reporting Bugs

We use [GitHub Issues](https://github.com/xamarin/Xamarin.Forms/issues) to track issues. If at all possible, please submit a [reproduction of your bug](https://gist.github.com/jassmith/92405c300e54a01dcc6d) along with your bug report.


