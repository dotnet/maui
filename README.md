# Xamarin.Essentials

Xamarin.Essentials gives developers essential cross-platform APIs for their mobile applications. 

iOS, Android, and UWP offer unique operating system and platform APIs that developers have access to all in C# leveraging Xamarin. It is great that developers have 100% API access in C# with Xamarin, but these APIs are different per platform. This means developers have to learn three different APIs to access platform specific features. With Xamarin.Essentials developers have a single cross-platform API that works with any iOS, Android, or UWP application that can be accessed from shared code no matter how the user interface is created.

[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/xamarin/Essentials)

## Build Status

| Build Server | Type         | Platform | Status                                                                                                                                                                                 |
|--------------|--------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Jenkins      | Build        | Windows  | [![Jenkins Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-Essentials)](https://jenkins.mono-project.com/view/Components/job/Components-Essentials/)        |
| VSTS         | Build        | Windows  | ![VSTS Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/8538/badge)                                                   |
| Jenkins      | Device Tests | macOS    | [![Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-Essentials-DeviceTests-Mac)](https://jenkins.mono-project.com/job/Components-Essentials-DeviceTests-Mac) |

## Installation
Xamarin.Essentials is available via:
* NuGet Official Releases: [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Essentials.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.Essentials)
* MyGet Development Feed: https://www.myget.org/F/xamarin-essentials/api/v3/index.json

Please read our [Getting Started with Xamarin.Essentials guide](https://docs.microsoft.com/xamarin/essentials/get-started) for full setup instructions.

## Documentation

Browse our [full documentation for Xamarin.Essentials](https://docs.microsoft.com/xamarin/essentials) including feature guides on how to use each feature.

## Supported Platforms
Xamarin.Essentials is focused on the following platforms:
 - iOS (10+)
 - Android (4.4+)
 - UWP (Fall Creators Update+)

## Current APIs:
The following cross-platform APIs are available in Xamarin.Essentials:
 - [x] [Accelerometer](https://docs.microsoft.com/xamarin/essentials/accelerometer)
 - [X] [App Information](https://docs.microsoft.com/xamarin/essentials/app-information)
 - [x] [Battery](https://docs.microsoft.com/xamarin/essentials/battery)
 - [x] [Clipboard](https://docs.microsoft.com/xamarin/essentials/clipboard)
 - [x] [Compass](https://docs.microsoft.com/xamarin/essentials/compass)
 - [x] [Connectivity](https://docs.microsoft.com/xamarin/essentials/connectivity)
 - [x] [Data Transfer (Share)](https://docs.microsoft.com/xamarin/essentials/data-transfer)
 - [x] [Device Display Information](https://docs.microsoft.com/en-us/xamarin/essentials/device-display)
 - [x] [Device Information](https://docs.microsoft.com/xamarin/essentials/device-information)
 - [x] [Email](https://docs.microsoft.com/xamarin/essentials/email)
 - [x] [File System Helpers](https://docs.microsoft.com/xamarin/essentials/file-system-helpers)
 - [x] [Flashlight](https://docs.microsoft.com/xamarin/essentials/flashlight)
 - [x] [Geocoding](https://docs.microsoft.com/xamarin/essentials/geocoding)
 - [x] [Geolocation](https://docs.microsoft.com/xamarin/essentials/geolocation)
 - [x] [Gyroscope](https://docs.microsoft.com/xamarin/essentials/gyroscope)
 - [x] [Magnetometer](https://docs.microsoft.com/xamarin/essentials/magnetometer)
 - [x] [Open Browser](https://docs.microsoft.com/xamarin/essentials/open-browser)
 - [x] [Orientation Sensor](https://docs.microsoft.com/en-us/xamarin/essentials/orientation-sensor)
 - [x] [Phone Dialer](https://docs.microsoft.com/xamarin/essentials/phone-dialer)
 - [x] [Preferences](https://docs.microsoft.com/xamarin/essentials/preferences)
 - [x] [Screen Lock](https://docs.microsoft.com/xamarin/essentials/screen-lock)
 - [x] [Secure Storage](https://docs.microsoft.com/xamarin/essentials/secure-storage)
 - [x] [SMS](https://docs.microsoft.com/xamarin/essentials/sms)
 - [x] [Text-to-Speech](https://docs.microsoft.com/xamarin/essentials/text-to-speech)
 - [x] [Version Tracking](https://docs.microsoft.com/xamarin/essentials/version-tracking)
 - [x] [Vibrate](https://docs.microsoft.com/xamarin/essentials/vibrate)
 
## Contributing
Please read through our [Contribution Guide](CONTRIBUTING.md). We are not accepting new PRs for full features, however any issue that is marked as `up for grabs` are open for community contributions. We encourage creating new issues for bugs found during usage that the team will triage. Additionally, we are open for code refactoring suggestions in PRs.

## Building Xamarin.Essentials
Xamarin.Essentials is built with the new SDK style projects with multi-targeting enabled. This means that all code for iOS, Android, and UWP exist inside of the Xamarin.Essentials project. 

If building on Visual Studio 2017 simply open the solution and build the project. 

If using Visual Studio for Mac the project can be built at the command line with MSBuild. To change the project type that you are working with simply edit Xamarin.Essentials.csproj and modify the TargetFrameworks for only the project type you want to use.

## FAQ
Here are some frequently asked questions about Xamarin.Essentials, but be sure to read our full [FAQ on our Wiki](https://github.com/xamarin/Essentials/wiki#feature-faq).

## License
Please see the [License](LICENSE).
