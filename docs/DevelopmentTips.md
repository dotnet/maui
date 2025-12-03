### Reproducing an Issue/Debugging .NET MAUI Code
Open the .NET MAUI [workspace](https://code.visualstudio.com/docs/editor/workspaces) in VS Code by simply opening the root of your locally cloned .NET MAUI repository folder. VS Code will detect the workspace automatically and suggest that you open it.

In VS Code, select the device that you will be testing on. Using the Command Palette (<kbd>CTRL</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd> or on macOS <kbd>Command</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd>) type `pick device` and you will be presented with a set of choices for your target device (Android, iOS, etc.). First select the option that describes the platform you want to run the project on, and then select the device that is available for that platform in the next menu.

![VS Code Command Palette to pick a target device](https://github.com/dotnet/maui/assets/939291/d008102f-0295-4034-a60a-8c2b58f86641)

There is a sample project in `src/Controls/samples/Controls.Sample.Sandbox`. This is an empty project, which directly references the .NET MAUI code. In this project you can add your code to reproduce an issue while allowing you to set breakpoints in .NET MAUI source code and debug through it easily.

To let VS Code know this is the project you want to run, select the Sandbox project by going to the Command Palette (<kbd>CTRL</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd> or on macOS <kbd>Command</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd>) and start typing `pick startup` and select ".NET MAUI: Pick Startup Project" and then select the Sandbox project.

![VS Code Command Palette to pick the startup project](https://github.com/dotnet/maui/assets/939291/eae00559-4811-4034-95ae-b6fd1ea6d1b7)

Before using the Command Palette for the first time, you may have to wait a minute
for IntelliSense and other tasks to initialize. If the project hasn't 'settled' yet, you will see an error "Pick Startup Project has resulted in an error."

*Note:* When you are committing your PR, do not include your changes to the Sandbox project.


### Cake Commands

The below parameters can be used with the `dotnet cake` command in the root of your locally cloned .NET MAUI repository folder.

#### PublicAPI Management
`--target=publicapi`
- Clears and regenerates PublicAPI.Unshipped.txt files across all MAUI projects (Core, Controls, Essentials, Graphics)
- Use this when you've added new public APIs and are getting build errors about missing API declarations
- Automatically skips Windows-specific files when not running on Windows, and always skips Tizen files

```bash
dotnet cake --target=publicapi
```

#### Clean
`--clean`
- Occasionally, when switching branches or syncing with the main branch, incremental builds may stop working. A common fix for this is to use git clean -xdf to delete all locally cached build information. However, the issue with git clean -xdf is that it will also wipe out any uncommitted changes. Using --clean to recursively delete the local obj/bin folders should hopefully resolve the issue while preserving your changes.

#### Target a specific platform
`--android`
`--ios`
`--windows`
`--catalyst`

```bash
dotnet cake --target=VS --workloads=global --android --ios
```

*Note* you will have to `git clean -xdf` your project if you change or add platforms. 

### Blazor Hybrid

To build and run Blazor Desktop samples, check out the [Blazor Desktop](https://github.com/dotnet/maui/wiki/Blazor-Desktop) wiki topic.

# Advanced Scenarios

### Compile using a local `.dotnet\dotnet` via `build.*` scripts on the root folder 

This method uses the Arcade build infrastructure. For more information, you can look [here](https://github.com/dotnet/arcade/blob/main/Documentation/ArcadeSdk.md#build-scripts-and-extensibility-points)

```
./build.sh -restore -pack
``` 
or 

```
./build.cmd -restore -pack
```


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
- This will pack .NET and then open a VS instance using the local pack. This is useful if you want to check to see if the changes in a branch will address your particular issues. Pack only runs the first time, so you will need to explicitly add the `--pack` flag if you make changes and need to repack.

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

Create a new .NET MAUI app using your new packs
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

You can then use VS or VSCode to attach to this process and debug your tasks.
You can start your test app with the `dotnet-local` script (so it uses your MAUI build).

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

Once MSBuild starts, it will print the following

```dotnetcli
Waiting for debugger to attach (dotnet PID xxxx).  Press enter to continue...
```

You need to copy the PID value so we can use this in the IDE. For Visual Studio, you can use the `Attach to Process` menu option while you have the Microsoft.Maui.sln solution open. For VSCode, open the workspace, then use the `Attach to Process` Run and Debug option. You will be prompted for the PID and it will then connect.

Once connected, go back to your command prompt and press ENTER so that the MSBuild process can continue.

You will be able to set breakpoints in Tasks (but not Targets) and step through code from this point on.

If you want to test in-tree in VSCode, the `Build Platform Sample` command will ask you if you want to debug MSBuild tasks and fill in the `MSBUILDDEBUGONSTART` for you. The PID text will appear in the `Terminal` window in VSCode. You can then use the `Attach to Process` Run and Debug option to attach to the process.


### Integration Tests

The Integration test project under `src/TestUtils/src/Microsoft.Maui.IntegrationTests` contains tests that build and/or run MAUI templates or other projects.

These tests can be run using the Test Explorer in VS, or from the command line with `dotnet test`. Here's how to run an individual test with parameters from command line:

```bash
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests --logger "console;verbosity=diagnostic" --filter "Name=Build\(%22maui%22,%22net7.0%22,%22Debug%22,False\)"
```

## Running Device Tests on Helix

.NET MAUI now supports running device tests on [.NET Engineering Services Helix](https://helix.dot.net) using XHarness. Helix provides cloud-based device testing infrastructure that enables running tests across multiple platforms and devices in parallel.

### Overview

Device tests can be run on the following platforms via Helix:


The device test projects include:
- `Controls.DeviceTests` - UI control tests
- `Core.DeviceTests` - Core framework tests  
- `Graphics.DeviceTests` - Graphics and drawing tests
- `Essentials.DeviceTests` - Platform API tests
- `MauiBlazorWebView.DeviceTests` - Blazor WebView tests


### Available Helix Queues

Check available queues at [helix.dot.net](https://helix.dot.net). The current configuration uses:

- **iOS**: `osx.15.arm64.Open`
- **Mac Catalyst**: `osx.15.arm64.Open`  
- **Android**: `ubuntu.2204.amd64.android.33.open`

### Running Device Tests Locally

The following commands assume you are on the root of the maui repository.

#### Step 1: Build MSBuild Tasks
First, restore tools and build the required MSBuild tasks:

```bash
# Restore dotnet tools
dotnet tool restore

# Build the MSBuild tasks (required)
./build.sh -restore -build -configuration Release -projects $(PWD)/Microsoft.Maui.BuildTasks.slnf /bl:BuildBuildTasks.binlog -warnAsError false
```

#### Step 2: Build Device Tests
Build the device test projects:

```bash
# Build device tests for all platforms
./build.sh -restore -build -configuration Release /p:BuildDeviceTests=true /bl:BuildDeviceTests.binlog -warnAsError false
```

#### Step 3: Send to Helix
Submit the tests to Helix for execution:

We need to set some variables. More info about Helix can be found [here](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/Readme.md) 


```bash
export BUILD_REASON=pr
export BUILD_REPOSITORY_NAME=maui
export BUILD_SOURCEBRANCH=main
export SYSTEM_TEAMPROJECT=dnceng
export SYSTEM_ACCESSTOKEN='' 
```

```bash
# Send to Helix for Android
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=android /bl:sendhelix_android.binlog -verbosity:diag

# Send to Helix for iOS  
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=ios /bl:sendhelix_ios.binlog -verbosity:diag

# Send to Helix for Mac Catalyst
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=maccatalyst /bl:sendhelix_catalyst.binlog -verbosity:diag
```

### Windows Commands

For Windows development, use the corresponding `.cmd` files:

```cmd
set BUILD_REASON=pr
set BUILD_REPOSITORY_NAME=maui
set BUILD_SOURCEBRANCH=main
set SYSTEM_TEAMPROJECT=dnceng
set SYSTEM_ACCESSTOKEN=
```

```cmd
REM Build MSBuild tasks
.\build.cmd -restore -build -configuration Release -projects ".\Microsoft.Maui.BuildTasks.slnf" /bl:BuildBuildTasks.binlog -warnAsError false

REM Build device tests
.\build.cmd -restore -build -configuration Release /p:BuildDeviceTests=true /bl:BuildDeviceTests.binlog -warnAsError false

REM Send to Helix (Android example)
.\eng\common\msbuild.cmd .\eng\helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=android /bl:sendhelix.binlog -verbosity:diag
```

### Configuration Details

The Helix configuration is defined in `eng/helix_xharness.proj` and includes:

- **Timeouts**: 2-hour work item timeout, 1-hour test timeout
- **Test Discovery**: Automatically discovers test bundles for each scenario
- **Platform Targeting**: Specific target frameworks per platform
- **Queue Selection**: Platform-appropriate Helix queues
- **XHarness Integration**: Uses XHarness for device orchestration

### Troubleshooting

#### Common Issues

1. **Build failures**: Ensure you've built the MSBuild tasks first
2. **Missing devices**: Check queue availability at [helix.dot.net](https://helix.dot.net)  
3. **Authentication**: For CI scenarios, ensure proper Azure DevOps access tokens
4. **Timeouts**: Tests have generous timeouts but may need adjustment for complex scenarios

#### Logging and Diagnostics

- Use `/bl:filename.binlog` for detailed MSBuild logs
- Add `-verbosity:diag` for maximum diagnostic output
- Check Helix job results at the provided URL after submission

### CI Integration

The device tests are integrated into the CI pipeline via:
- `eng/pipelines/common/stage-device-tests.yml` - Pipeline template
- `eng/test-configuration.json` - Test retry configuration
- Automatic execution on PR builds for qualifying changes

### Additional Resources

- [XHarness on Helix Documentation](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/tools/xharness-runner/Readme.md#android-apk-payloads)
- [Helix Documentation](https://github.com/dotnet/arcade/tree/main/src/Microsoft.DotNet.Helix)
- [Example Helix Run](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1115383&view=results)
