# Project Caboodle

Project Caboodle gives developers essential cross-platform APIs for their mobile applications. 

iOS, Android, and UWP offer unique operating system and platform APIs that developers have access to all in C# leveraging Xamarin. It is great that developers have 100% API access in C# with Xamarin, but these APIs are different per platform. This means developers have to learn three different APIs to access platform specific features. With Caboodle developers have a single cross-platform API that works with any iOS, Android, or UWP application that can be accessed from shared code no matter how the user interface is created.

## Build Status

| Build Server | Type         | Platform | Status                                                                                                                                                                                 |
|--------------|--------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Jenkins      | Build        | Windows  | [![Jenkins Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-Caboodle)](https://jenkins.mono-project.com/view/Components/job/Components-Caboodle/)        |
| VSTS         | Build        | Windows  | ![VSTS Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/8538/badge)                                                   |
| Jenkins      | Device Tests | macOS    | [![Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-Caboodle-DeviceTests-Mac)](https://jenkins.mono-project.com/job/Components-Caboodle-DeviceTests-Mac) |

## Installation
Coming soon... 

## Supported Platforms
Caboodle is focused on the following platforms for our first release:
 - iOS (10+)
 - Android (4.4+)
 - UWP (Fall Creators Update+)

## Current Features:
The following cross-platform APIs are planned for our first release:
 - [ ] Accelerometer
 - [X] App Information
 - [x] Battery
 - [X] Clipboard
 - [ ] Compass
 - [ ] Connectivity
 - [ ] Device Capabilities
 - [x] Device Information
 - [x] File System Helpers
 - [ ] Email
 - [x] Geocoding 
 - [ ] Geolocation 
 - [ ] Gyroscope
 - [ ] Magnetometer
 - [ ] Open Browser
 - [ ] Permissions
 - [ ] Phone Dialer
 - [x] Preferences
 - [ ] Share Text
 - [ ] SMS
 - [ ] Text-to-Speech
 
## Contributing
Please read through our [Contribution Guide](CONTRIBUTING.md). We are not accepting new PRs for full features, however any issue that is marked as `up for grabs` are open for community contributions. We encourage creating new issues for bugs found during usage that the team will triage. Additionally, we are open for code refactoring suggestions in PRs.

## Building Caboodle
Caboodle is built with the new SDK style projects with multi-targeting enabled. This means that all code for iOS, Android, and UWP exist inside of the Caboodle project. 

If building on Visual Studio 2017 simply open the solution and build the project. 

If using Visual Studio for Mac the project can be built at the command line with MSBuild. To change the project type that you are working with simply edit Caboodle.csproj and modify the TargetFrameworks for only the project type you want to use.

## FAQ
Here are some frequently asked questions about Caboodle.

### Where are the interfaces?
Some developers prefer an interface based programming model for dependency injection and testing of code. Caboodle does not offer any interfaces and delivers straight API access via direct classes and static properties/methods. There are many reasons that Caboodle is architected this way that include performance, simplicity, and ease of use. We also consider Caboodle a core API of the platform just like System classes, HttpClient, and our platform bindings. 

Additionally, we found most developers create their own interfaces even when using a library that have interfaces. They do this so they have control over the APIs they actually use, which may be a very small percentage of the overall APIs in the library. Creating your own `ICaboodle` and exposing only the methods and properties you would like to use gives you more flexibility as Caboodle grows and adds additional APIs. Using this type of architecture will enable you to both have dependency injection and work with unit testing.


### Does Caboodle replace plugins?
Plugins offer a wide variety of cross-platform APIs for developers to use in their applications. Plugins will still continue to grow and flourish as they can offer a wider range of APIs and handle unique scenarios that Cabooble may not offer including additional platform support or add features unique to a single platform.

## License
Please see the [License](LICENSE).
