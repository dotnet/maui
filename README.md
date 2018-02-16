
<img src="banner.png" alt="Xamarin.Forms banner" height="145" >

# Xamarin.Forms #

Xamarin.Forms provides a way to quickly build native apps for iOS, Android, Windows and macOS, completely in C#.

Read more about the platform at https://www.xamarin.com/forms.

## Build Status 

![Visual Studio Team Services ](https://img.shields.io/vso/build/devdiv/0bdbc590-a062-4c3f-b0f6-9383f67865ee/7981.svg?style=flat&label=VSTS%20Master "VSTS")
 

## Packages ##


Platform/Feature               | Package name                              | Stable (2.5.0 branch)     |Nightly Feed [MyGet](https://www.myget.org/F/xamarinforms-ci/api/v2)  (master branch)
-----------------------|-------------------------------------------|-----------------------------|-------------------------
Core             | `Xamarin.Forms` | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms/)| [![MyGet](https://img.shields.io/myget/xamarinforms-ci/vpre/Xamarin.Forms.svg?style=flat-square&label=myget)](https://myget.org/feed/xamarinforms-ci/package/nuget/Xamarin.Forms)
Maps                 | `Xamarin.Forms.Maps`    | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Maps.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Maps/) | [![MyGet](https://img.shields.io/myget/xamarinforms-ci/vpre/Xamarin.Forms.Maps.svg?style=flat-square&label=myget)](https://myget.org/feed/xamarinforms-ci/package/nuget/Xamarin.Forms.Maps)
Pages  | `Xamarin.Forms.Pages`  | [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.Pages.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Xamarin.Forms.Pages/) | [![MyGet](https://img.shields.io/myget/xamarinforms-ci/vpre/Xamarin.Forms.Pages.svg?style=flat-square&label=myget)](https://myget.org/feed/xamarin.forms-ci/package/nuget/Xamarin.Forms.Pages)

If you want to use the latest dev build then you should read [this blog post]( https://blog.xamarin.com/try-the-latest-in-xamarin-forms-with-nightly-builds) :
- Add the nightly feed to your nuget sources or add a NuGet.Config to your app with the following content:

  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
      <packageSources>
          <clear />
          <add key="xamarin-ci" value="https://www.myget.org/F/xamarinforms-ci/api/v2" />
          <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
      </packageSources>
  </configuration>
  ```

  *NOTE: This NuGet.Config should be with your application unless you want nightly packages to potentially start being restored for other apps on the machine.*

- Change your application's dependencies to have a `*` to get the latest version.


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

##### Build from the Command line #####
Make sure you have Nuget.exe 4.0 or above and the latest dotnet core sdk (2.0.3). On OSX you should specify the platform in the msbuild command (`/p:Platform=iPhoneSimulator`)
  

     nuget restore Xamarin.Forms.sln
     msbuild Xamarin.Forms.sln
 

##### API documentation changes #####
If you change or add a public API on Xamarin.Forms the associated documentation should be updated and pushed as part of the pull request. This will also be needed to create a local nuget if you want. We rely on mdoc that is shipped in the repo on the tools folder. After building the .sln on Windows run:
     
     ./update-docs-windows.bat

on OSX execute:
     
     make docs


## Coding Style ##
We follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with a few exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We use hard tabs over spaces. You can change this setting in VS 2015 via `Tools > Options` and navigating to `Text Editor > C#` and selecting the "Keep tabs" radio option. In Visual Studio for Mac it's set via preferences in `Source Code > Code Formatting > C# source code` and disabling the checkbox for `Convert tabs to spaces`.
- Lines should be limited to a max of 120 characters (or as close as possible within reason). This may be set in Visual Studio for Mac via preferences in `Source Code > Code Formatting > C# source code` and changing the `Desired file width` to `120`.

## Contributing ##

- [How to Contribute](https://github.com/xamarin/Xamarin.Forms/blob/master/CONTRIBUTING.md)

### Reporting Bugs

We use [GitHub Issues](https://github.com/xamarin/Xamarin.Forms/issues) to track issues. If at all possible, please submit a [reproduction of your bug](https://gist.github.com/jassmith/92405c300e54a01dcc6d) along with your bug report.

