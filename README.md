# Xamarin.Essentials

Xamarin.Essentials gives developers essential cross-platform APIs for their mobile applications. 

iOS, Android, and UWP offer unique operating system and platform APIs that developers have access to, all in C# leveraging Xamarin. It is great that developers have 100% API access in C# with Xamarin, but these APIs are different per platform. This means developers have to learn three different APIs to access platform-specific features. With Xamarin.Essentials, developers have a single cross-platform API that works with any iOS, Android, or UWP application that can be accessed from shared code no matter how the user interface is created.

[![Discord](https://img.shields.io/discord/732297728826277939?label=discord&style=plastic)](https://discord.com/invite/Y8828kE)

## Build Status

| Build Server | Type         | Platform | Status                                                                                                                                                                                 |
|--------------|--------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| VSTS         | Build        | Windows  | [![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/Xamarin.Essentials?branchName=master)](https://dev.azure.com/xamarin/public/_build?definitionId=7&_a=summary)                                                  |
| App Center   | Sample App   | Android  | [![Build status](https://build.appcenter.ms/v0.1/apps/7a1f46ca-ba2f-477e-aacc-ff013c7d5f7a/branches/master/badge)](https://appcenter.ms) |
| App Center   | Sample App   | iOS      | [![Build status](https://build.appcenter.ms/v0.1/apps/43b39e9e-2b2b-482f-8afa-e9906334c85e/branches/master/badge)](https://appcenter.ms) |

## Sample App

Try out Xamarin.Essentials on your device!

* [Android Sample App](https://install.appcenter.ms/orgs/microsoft-liveplayer/apps/essentials-android/distribution_groups/public%20testers) via App Center.

## Installation

Xamarin.Essentials is available via NuGet & is included in every template:

* NuGet Official Releases: [![NuGet](https://img.shields.io/nuget/vpre/Xamarin.Essentials.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.Essentials)

Please read our [Getting Started with Xamarin.Essentials guide](https://docs.microsoft.com/xamarin/essentials/get-started) for full setup instructions.

## Documentation

Browse our [full documentation for Xamarin.Essentials](https://docs.microsoft.com/xamarin/essentials), including feature guides, on how to use each feature.

## Supported Platforms

Platform support & feature support can be found on our [documentation](https://docs.microsoft.com/xamarin/essentials/platform-feature-support)


## Contributing

Please read through our [Contribution Guide](CONTRIBUTING.md). We are not accepting new PRs for full features, however any [issue that is marked as `up for grabs`](https://github.com/xamarin/Essentials/issues?q=is%3Aissue+is%3Aopen+label%3A%22up+for+grabs%22) are open for community contributions. We encourage creating new issues for bugs found during usage that the team will triage. Additionally, we are open for code refactoring suggestions in PRs.

## Building Xamarin.Essentials

Xamarin.Essentials is built with the new SDK-style projects with multi-targeting enabled. This means that all code for iOS, Android, and UWP exist inside of the Xamarin.Essentials project.

## Visual Studio

A minimum version of Visual Studio 2019 16.3 or Visual Studio for Mac 2019 8.3 are required to build and compile Xamarin.Essentials.

### Workloads need:

* Xamarin
* .NET Core
* UWP

### You will need the following SDKs

* Android 10.0, 9.0, 8.1, 8.0, 7.1, 7.0, & 6.0 SDK Installed
* UWP 10.0.16299 SDK Installed

Your can run the included `android-setup.ps1` script in **Administrator Mode** and it will automatically setup your Android environment.

## FAQ

Here are some frequently asked questions about Xamarin.Essentials, but be sure to read our full [FAQ on our Wiki](https://github.com/xamarin/Essentials/wiki#feature-faq).

## License

Please see the [License](LICENSE).

