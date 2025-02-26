# Development Guide

This page contains steps to build and run the .NET MAUI repository from source. If you are looking to build apps with .NET MAUI please head over to the links in the [README](https://github.com/dotnet/maui/blob/main/README.md) to get started.

## Initial setup

   ### Windows
   - Install VS 17.12 or newer
      - Follow [these steps](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin) to include MAUI
   - If building iOS with pair to Mac: Install current stable Xcode on your Mac. Install from the [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
   - If you're missing any of the Android SDKs, Visual Studio should prompt you to install them. If it doesn't prompt you then use the [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk) to install the necessary SDKs.
   - Install [Open JDK 17](https://learn.microsoft.com/en-us/java/openjdk/download#openjdk-17)

   ### Mac
   - Install [VSCode](https://code.visualstudio.com/download)
   - Follow the steps for installing the .NET MAUI Dev Kit for VS Code: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui
      
## Building the Build Tasks
Before opening the solution in Visual Studio / VS Code you **MUST** build the build tasks.

Note: the main branch is always [pinned to the latest stable release](https://github.com/dotnet/maui/blob/main/global.json) of the .NET SDK. This is regardless of said release being a long-term support (LTS) release or not. This means that you will need to have that version of the .NET SDK installed and configured on your machine in order to be able to execute the commands below and build the .NET MAUI codebase.

1. Open a command prompt/terminal/shell window
1. Navigate to the location of your cloned `dotnet/maui` repo, for example:
     ```shell
     cd \repos\maui
     ```
1. Run these commands:
    ```dotnetcli
    dotnet tool restore
    dotnet build ./Microsoft.Maui.BuildTasks.slnf
    ```

## Windows
Open the `Microsoft.Maui-windows.slnf` file in Visual Studio from the root of the repo.

## Mac

Open the root folder of the repository in VS Code.

*NOTE*: IntelliSense takes a decent amount of time to fully process your solution. It will eventually work through all the necessary tasks. If you are having IntelliSense issues, usually unloading/reloading the `maui.core` and `maui.controls` projects will resolve. 

## What branch should I use?

As a general rule:
- [main](https://github.com/dotnet/maui/tree/main)

Use ‘main’ for bug fixes that don’t require API changes. For new features and changes to public APIs, you must use the branch of the next .NET version.

- [net10.0](https://github.com/dotnet/maui/tree/net10.0)

## Sample projects

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
- *Essentials.Sample*: Full gallery demonstrating the library previously known as Essentials. These are all the non UI related MAUI APIs.

### Testing

- [Testing Wiki](https://github.com/dotnet/maui/wiki/Testing)

## Generating PublicAPI Files

If you've added new public APIs and are getting build errors about missing API declarations, you'll need to update the PublicAPI files. You can generate the PublicAPI files manually by building a project with the `PublicApiType=Generate` property:

```dotnetcli
dotnet build ./src/Controls/src/Core/Controls.Core.csproj /p:PublicApiType=Generate
```

This approach will generate the `PublicAPI.Unshipped.txt` files for that specific project. You may need to run this for each project that has new public APIs.

**Note:** If you're still having troubles with PublicAPI errors, you can delete all the content in the relevant `PublicAPI.Unshipped.txt` files and then run the command above to regenerate them completely.


## Stats

<img src="https://repobeats.axiom.co/api/embed/f917a77cbbdeee19b87fa1f2f932895d1df18b71.svg" />
