# Development Guide

This page contains steps to build and run the .NET MAUI repository from source. If you are looking to build apps with .NET MAUI please head over to the links in the [README](https://github.com/dotnet/maui/blob/main/README.md) to get started.

## Visual Studio
Follow the instructions here to install .NET MAUI with Visual Studio Stable:
   - [Windows](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin)
      - Select the 20348 SDK option inside Individual Components or [install 20348 manually](https://go.microsoft.com/fwlink/?linkid=2164145)
      - If you know you have 20348 installed but are still getting an error around this SDK missing, trying uninstalling and reinstalling the SDK.
   - [macOS](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vsmac)  
   
## iOS / MacCatalyst

iOS and MacCatalyst will require current stable Xcode. You can get this [here](https://developer.apple.com/download/more/?name=Xcode).

## Android

If you're missing any of the Android SDKs, Visual Studio should prompt you to install them. If it doesn't prompt you then use the [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk) to install the necessary SDKs.

## Building the Build Tasks
Before opening the solution in Visual Studio you **MUST** build the build tasks. You have two options:

- Do this to build the build tasks and launch Visual Studio, automatically opening the default solution:

   ```dotnetcli
   dotnet tool restore
   dotnet cake --target=VS --workloads=global
   ```

   *NOTE*: `--workloads=global` means use the normal (globally installed) .NET workloads.

- OR do this to just build the build tasks. You can then launch Visual Studio manually and open the solution of your choosing:

   ```dotnetcli
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   ```

## Available Solutions
- Microsoft.Maui.sln
  - Kitchen sink solution. This includes all of the `Compatibility` projects and all of the platforms that we compile for. It is very unlikely you will need to use this solution for development. 
- Microsoft.Maui-dev.sln
  - `Microsoft.Maui.sln` but without the `Compatibility` projects. Because we can't detect solution filters inside `MSBuild` we had to create a separate `sln` without the `Compatibility` projects. 
- Microsoft.Maui-windows.slnf
  - `Microsoft.Maui-dev.sln` with all of the targets you can't build on `Windows` removed (GTK/Catalyst). Default solution on Windows.
- Microsoft.Maui-mac.slnf
  - `Microsoft.Maui-dev.sln` with all of the `Windows` targets filtered out. Default solution on Mac.

*NOTE*: IntelliSense takes a decent amount of time to fully process your solution. It will eventually work through all the necessary tasks. If you are having IntelliSense issues, usually unloading/reloading the `maui.core` and `maui.controls` projects will resolve. 

## What branch should I use?
- main

Always use main no matter what you are working on or where you are hoping your change will get applied. We make sure that main always works against the current stable releases of Visual Studio and the .NET MAUI SDK. Even if you are working on features that will only be released with a future version of .NET. `main` is the only relevant branch for current development.

## Repository projects

### Samples
 ```
├── Controls 
│   ├── samples
│   │   ├── Maui.Controls.Sample
│   │   ├── Maui.Controls.Sample.Sandbox
│   │   ├── Controls.Sample.UITests
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
- *Contols.Sample.UITests*: Sample used for the automated UI tests
- *Essentials.Sample*: Full gallery demonstrating  the library previously known as essentials. These are all the non UI related MAUI APIs.

### Device Test Projects

These are tests that will run on an actual device

 ```
├── Controls 
│   ├── test
│   │   ├── Controls.DeviceTests
├── Core 
│   ├── test
│   │   ├── Core.DeviceTests
├── Essentials 
│   ├── test
│   │   ├── Essentials.DeviceTests
├── BlazorWebView 
│   ├── test
│   │   ├── MauiBlazorWebView.DeviceTests
```

- *Controls.DeviceTests*: .NET MAUI Controls Visual Runner for running device based xunit tests. This is useful for tests that require XAML features
- *Core.DeviceTests*: .NET MAUI Core Visual Runner for running device based xunit tests. This is for tests that don't require any MAUI Controls based features
- *Essentials.DeviceTests*: Visual Runner running all the .NET MAUI essentials xunit tests.
- *MauiBlazorWebView.DeviceTests*: Visual Runner for BlazorWebView tests. 

### UI Test Projects

These are tests used for exercising the UI through accessibility layers to simulate user interactions

```
├──  Controls
│    ├── tests
│    │   ├── UITests
```

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

### Integration Tests

The Integration test project under `src/TestUtils/src/Microsoft.Maui.IntegrationTests` contains tests which build and/or run MAUI templates or other projects.

These tests can be ran using the test explorer in VS, or from command line with `dotnet test`. Here's how to run an individual test with parameters from command line:

```bash
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests --logger "console;verbosity=diagnostic" --filter "Name=Build\(%22maui%22,%22net7.0%22,%22Debug%22,False\)"
```

### Additional Cake Commands

#### Clean
`--clean`
- This will do a recursive delete of all your obj/bin folders. This is helpful if for some reason your repository is in a bad state and you don't want to go as scorched earth as `git clean -xdf`

#### Target a specific platform
`--android`
`--ios`
`--windows`
`--catalyst`

```bash
dotnet cake --target=VS --workloads=global --android --ios
```

*Note* you will have to `git clean -xdf` your project if you change or add platforms. 

### Blazor Desktop

To build and run Blazor Desktop samples, check out the [Blazor Desktop](https://github.com/dotnet/maui/wiki/Blazor-Desktop) wiki topic.

### Android

To workaround a performance issue, all `Resource.designer.cs`
generation is disabled for class libraries in this repo.

If you need to add a new `@(AndroidResource)` value to be used from C#
code in .NET MAUI:

1. Comment out the `<PropertyGroup>` in `Directory.Build.targets` that
   sets `$(AndroidGenerateResourceDesigner)` and
   `$(AndroidUseIntermediateDesignerFile)` to `false`.

2. Build .NET MAUI as you normally would. You will get compiler errors
   about duplicate fields, but `obj\Debug\net[current_sdk_version]-android\Resource.designer.cs`
   should now be generated.

3. Open `obj\Debug\net[current_sdk_version]-android\Resource.designer.cs`, and find the
   field you need such as:

```csharp
// aapt resource value: 0x7F010000
public static int foo = 2130771968;
```

4. Copy this field to the `Resource.designer.cs` checked into source
   control, such as: `src\Controls\src\Core\Platform\Android\Resource.designer.cs`

5. Restore the commented code in `Directory.Build.targets`.


# Advanced Scenarios

### Compile using a local `bin\dotnet`

This method will use the .NET and workload versions that are specific to the branch you are on, which is a good way to ensure compatibility.

#### Cake

You can run a `Cake` target to bootstrap .NET SDK in `bin\dotnet` and launch Visual Studio:

```dotnetcli
dotnet tool restore
dotnet cake --target=VS
```

#### Testing branch against your project
`--sln=<Path to SLN>`
- This will pack .NET and then open a VS instance using the local pack. This is useful if you want to check to see if the changes in a branch will address your particular issues. Pack only runs the first time so you will need to explicitly add the `--pack` flag if you make changes and need to repack.

```dotnetcli
dotnet tool restore
dotnet cake --sln="<download_directory>\MauiApp2\MauiApp2.sln" --target=VS
```

#### Pack
`--pack`
- This creates .NET MAUI packs inside the local dotnet install. This lets you use the CLI commands with the local dotnet to create/deploy with any changes that have been made on that branch (including template changes).

```dotnetcli
dotnet tool restore
dotnet cake --target=VS --pack --sln="<download_directory>\MauiApp2\MauiApp2.sln"
```

Create new .NET MAUI app using your new packs
```dotnetcli
dotnet tool restore
dotnet cake --pack
mkdir MyMauiApp
cd MyMauiApp
..\bin\dotnet\dotnet new maui
..\bin\dotnet\dotnet build -t:Run -f net[current_sdk_version]-android
```

You can also run commands individually:
```dotnetcli
# install local tools required to build (cake, pwsh, etc..)
dotnet tool restore
# Provision .NET SDK in bin\dotnet
dotnet build src\DotNet\DotNet.csproj
# Builds Maui MSBuild tasks
.\bin\dotnet\dotnet build Microsoft.Maui.BuildTasks.slnf
# Builds the rest of Maui
.\bin\dotnet\dotnet build Microsoft.Maui.sln
# Launch Visual Studio
dotnet cake --target=VS
```


## Stats

<img src="https://repobeats.axiom.co/api/embed/f917a77cbbdeee19b87fa1f2f932895d1df18b71.svg" />
