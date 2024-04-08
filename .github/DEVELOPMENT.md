# Development Guide

This page contains steps to build and run the .NET MAUI repository from source. If you are looking to build apps with .NET MAUI please head over to the links in the [README](https://github.com/dotnet/maui/blob/main/README.md) to get started.

## Initial setup
   ### Windows
   - Install VS 17.8 or newer
      - Follow [these steps](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin) to include MAUI
      - Select the 20348 SDK option inside Individual Components or [install 20348 manually](https://go.microsoft.com/fwlink/?linkid=2164145). If you know you have 20348 installed but are still getting an error around this SDK missing, trying uninstalling and reinstalling the SDK.
   - If building iOS with pair to Mac: Install current stable Xcode on your Mac. Install from the [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
   - If you're missing any of the Android SDKs, Visual Studio should prompt you to install them. If it doesn't prompt you then use the [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk) to install the necessary SDKs.

   ### Mac
   - Install VS Code and dependencies
      - Follow [these steps](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-8.0&tabs=visual-studio-code), installing VS Code, MAUI extension, .NET8, Xcode, OpenJDK, and Android SDK
      - Install Mono from [Mono Project](https://www.mono-project.com)
      - For Xcode, you can install from the [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
      - As of 26 March 2024, Xcode 15.3 is not yet supported. Check [this issue](https://github.com/dotnet/maui/issues/21057) for updates
      - Edit your `.zprofile` file to ensure that the following environment variables are set/modified (you may need to adjust the version of OpenJDK):
      ```shell
      export JAVA_HOME=/Library/Java/JavaVirtualMachines/microsoft-11.jdk/Contents/Home
      export ANDROID_HOME=~/Library/Android/sdk
      export ANDROID_SDK_ROOT=~/Library/Android/sdk
      export PATH="$PATH:~/.dotnet/tools"
      export PATH="$PATH:$ANDROID_HOME/platform-tools"
      export PATH="$PATH:$ANDROID_HOME/tools"
      export PATH="$PATH:$ANDROID_HOME/tools/bin"
      export PATH="$PATH:$ANDROID_HOME/tools/emulator" 
      ```
      - In VSCode do `command--shift-P` then type `code path` and select the option `Shell Command: Install 'code' in PATH`
      
## Building the Build Tasks
Before opening the solution in Visual Studio / VS Code you **MUST** build the build tasks.

### Windows
- Do this to build the build tasks and launch Visual Studio, automatically opening the default solution:

   ```dotnetcli
   dotnet tool restore
   dotnet cake --target=VS --workloads=global
   ```

- OR do this to just build the build tasks. You can then launch Visual Studio manually and open the solution of your choosing:

   ```dotnetcli
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   ```

### Mac
- Do this to build the tasks and open the Visual Studio Code codespace:
   
   ```dotnetcli
   dotnet tool restore
   dotnet cake --target=VSCode --workloads=global
   ```

*NOTE*: `--workloads=global` means use the normal (globally installed) .NET workloads.


## Available Solutions
- Microsoft.Maui.sln
  - Kitchen sink solution. This includes all of the `Compatibility` projects and all of the platforms that we compile for. It is very unlikely you will need to use this solution for development. 
- Microsoft.Maui-dev.sln
  - `Microsoft.Maui.sln` but without the `Compatibility` projects. Because we can't detect solution filters inside `MSBuild` we had to create a separate `sln` without the `Compatibility` projects. 
- Microsoft.Maui-windows.slnf
  - `Microsoft.Maui-dev.sln` with all of the targets you can't build on `Windows` removed (GTK/Catalyst). Default solution on Windows.
- Microsoft.Maui-mac.slnf
  - `Microsoft.Maui-dev.sln` with all of the `Windows` targets filtered out. Legacy solution for VS Mac.
- Microsoft.Maui-vscode.sln
  - Solution for VS Code (VS Code doesn't support solution filters)

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
│    ├── samples
│    │   ├── Controls.Sample.UITests
│    ├── tests
│    │   ├── Controls.AppiumTests
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

### Reproducing an Issue/Debugging .NET MAUI Code
Open the .NET MAUI workspace in VSCode.
In VSCode, select the device that you will be testing on. Using the command palette (ctrl-shift-P/command-shift-P) type `pick device` and
you will be presented with a set of choices for your target device (Android, iOS, etc). Select one.
There is a sample project in `src/Controls/samples/Controls.Sample.Sandbox`. This is an empty project
into which you can add your code to reproduce an issue and also set breakpoints in .NET MAUI source code.
Let VSCode know this is the project you want to select by going to the command palette (ctrl-shift-P/command-shift-P)
and typing `pick startup` and select ".NET MAUI: Pick Startup Project" and select the Sandbox project.

 Before using the command palette for the first time, you may have to wait a minute
for intellisense and other tasks to complete before using the command palette. If the project hasn't
'settled' yet, you will see an error "Pick Startup Project has resulted in an error."

*Note:* When you are committing your PR, do not include your changes to the Sandbox project.

### Integration Tests

The Integration test project under `src/TestUtils/src/Microsoft.Maui.IntegrationTests` contains tests which build and/or run MAUI templates or other projects.

These tests can be ran using the test explorer in VS, or from command line with `dotnet test`. Here's how to run an individual test with parameters from command line:

```bash
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests --logger "console;verbosity=diagnostic" --filter "Name=Build\(%22maui%22,%22net7.0%22,%22Debug%22,False\)"
```

You can find detailed information about testing in the [Wiki](https://github.com/dotnet/maui/wiki/Testing).

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

### Compile using a local `bin\dotnet` via `dotnet-local.*`

This method will use the .NET and workload versions that are specific to the branch you are on, which is a good way to ensure compatibility.

Use `dotnet-local.cmd` on Windows or `dotnet-local.sh` on Unix to ensure that `PATH` is set consistently.

#### Cake

You can run a `Cake` target to bootstrap .NET SDK in `bin\dotnet` and launch Visual Studio:

```dotnetcli
dotnet tool restore
dotnet cake --target=VS
```

There is also a `VSCode` target for launching Visual Studio Code.

```dotnetcli
dotnet tool restore
dotnet cake --target=VSCode
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

## Debugging MSBuild Tasks using VS/VSCode

One thing that is very useful is the ability to debug your Tasks while
they are being run on a build process. This is possible thanks to the
`MSBUILDDEBUGONSTART` environment variable. When set to `2` this will
force MSBuild to wait for a debugger connection before continuing.
You will see the following prompt.

```dotnetcli
Waiting for debugger to attach (dotnet PID 13001).  Press enter to continue...
```

You can then use VS or VSCode to attach to this process and debug you tasks.
You can start your test app with the `dotnet-local` script (so it uses your maui build)

### [MacOS](#tab/macos)

```dotnetcli
MSBUILDDEBUGONSTART=2 ~/<some maui checkout>/dotnet-local.sh build -m:1
```

### [Linux](#tab/linux)

```dotnetcli
MSBUILDDEBUGONSTART=2 ~/<some maui checkout>/dotnet-local.sh build -m:1
```

### [Windows](#tab/windows)

```dotnetcli
set MSBUILDDEBUGONSTART=2
~/<some maui checkout>/dotnet-local.cmd build -m:1
```

---

Note: the `-m:1` is important as it restricts MSBuild to 1 node.

Once MSBuild starts it will print the following

```dotnetcli
Waiting for debugger to attach (dotnet PID xxxx).  Press enter to continue...
```

You need to copy the PID value so we can use this in the IDE. For Visual Studio you can use the `Attach to Process` menu option, while you have the Microsoft.Maui.sln solution open. For VSCode open the workspace then use the `Attach to Process` Run and Debug option. You will be prompted for the PID and it will then connect.

Once connected go back to your command prompt and press ENTER so that the MSBuild process can continue.

You will be able to set breakpoints in Tasks (but not Targets) and step through code from this point on.

If you want to test in-tree in VSCode the `Build Platform Sample` command will ask you if you want to debug MSBuild tasks and fill in the `MSBUILDDEBUGONSTART` for you. The PID text will appear in the `Terminal` window in VSCode. You can then use the `Attach to Process` Run and Debug option to attach to the process.

## Stats

<img src="https://repobeats.axiom.co/api/embed/f917a77cbbdeee19b87fa1f2f932895d1df18b71.svg" />
