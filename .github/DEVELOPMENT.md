# Development Guide

This page contains steps to build and run the .NET MAUI repository from source. If you are looking to build apps with .NET MAUI please head over to the links in the [README](https://github.com/dotnet/maui/blob/main/README.md) to get started.

## Initial setup
   ### Windows
   - Install VS 17.10 or newer
      - Follow [these steps](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin) to include MAUI
   - If building iOS with pair to Mac: Install current stable Xcode on your Mac. Install from the [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
   - If you're missing any of the Android SDKs, Visual Studio should prompt you to install them. If it doesn't prompt you then use the [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk) to install the necessary SDKs.

   ### Mac
   - Follow the steps for installing the .NET MAUI Dev Kit for VS Code: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui
      
## Building the Build Tasks
Before opening the solution in Visual Studio / VS Code you **MUST** build the build tasks.

```dotnetcli
dotnet tool restore
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

## Windows
- Microsoft.Maui-windows.slnf

## Mac
- Just open the root folder of the repository in vscode

*NOTE*: IntelliSense takes a decent amount of time to fully process your solution. It will eventually work through all the necessary tasks. If you are having IntelliSense issues, usually unloading/reloading the `maui.core` and `maui.controls` projects will resolve. 

## What branch should I use?

As a general rule:
- [main](https://github.com/dotnet/maui/tree/main)

Use ‘main’ for bug fixes that don’t require API changes. For new features and changes to public APIs, you must use the branch of the next .NET version.

- [net9.0](https://github.com/dotnet/maui/tree/net9.0)

## Repository projects

### Samples
 ```
├── Controls 
│   ├── samples
│   │   ├── Maui.Controls.Sample
│   │   ├── Maui.Controls.Sample.Sandbox
├── Essentials 
│   ├── samples
│   │   ├── Essentials.Sample
├── BlazorWebView 
│   ├── samples
│   │   ├── BlazorWinFormsApp
│   │   ├── BlazorWpfApp
```

- *Maui.Controls.Sample*: Full gallery sample with all of the controls and features of .NET MAUI
- *Maui.Controls.Sample.Sandbox*: Empty project useful for testing reproductions or use cases
- *Essentials.Sample*: Full gallery demonstrating  the library previously known as essentials. These are all the non UI related MAUI APIs.

### Device Test Projects

[Writing Device Tests](https://github.com/dotnet/maui/wiki/DeviceTests)

### UI Test Projects

[Writing UI Tests](https://github.com/dotnet/maui/wiki/UITests)

### Unit Test Projects

These are tests that will not run on a device. This is useful for testing device independent logic.

 ```
├── Controls 
│   ├── test
│   │   ├── Controls.Core.UnitTests
├── Core 
│   ├── test
│   │   ├── Core.UnitTests
├── Essentials 
│   ├── test
│   │   ├── Essentials.UnitTests
```


## Stats

<img src="https://repobeats.axiom.co/api/embed/f917a77cbbdeee19b87fa1f2f932895d1df18b71.svg" />
