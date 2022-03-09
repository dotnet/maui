# Development Guide

This page contains steps to build and run the .NET MAUI repository from source. If you are looking to build apps with .NET MAUI please head over to the [.NET MAUI documentation](https://docs.microsoft.com/dotnet/maui) to get started.

## Requirements

### .NET 6 SDK

In most cases, when you have Visual Studio installed with the .NET workloads checked, these steps are not required.

1. Install the latest .NET 6:  
   <!--- [Win (x64)](https://aka.ms/dotnet/6.0.2xx/daily/dotnet-sdk-win-x64.exe)   -->
   - [Install the latest Public Preview of Visual Studio](https://docs.microsoft.com/en-us/dotnet/maui/get-started/installation/)
   - [macOS (x64)](https://aka.ms/dotnet/6.0.2xx/daily/dotnet-sdk-osx-x64.pkg)  
   - [macOS (arm64)](https://aka.ms/dotnet/6.0.2xx/daily/dotnet-sdk-osx-arm64.pkg)
2. Clear your nuget cache:  
   ```
   dotnet nuget locals all --clear
   ```
   > NOTE: this is going to contain the "stable" versions of the packages, so you will have to clear the NuGet cache when this feed changes and when .NET ships. The various `darc-pub-dotnet-*` feeds are temporary and are generated on various builds. These feeds my disappear and be replaced with new ones as new builds come out. Make sure to verify that you are on the latest here and clear the nuget cache if it changes:  
   

### .NET MAUI Workload

> You'll probably need to run these commands with elevated privileges:

Install the .NET MAUI workload using the versions from a particular branch:  

For example, the "preview.14" branch:

```
iwr https://aka.ms/dotnet/maui/maui-install.ps1 -OutFile maui-install.ps1;
.\maui-install.ps1 -b 'release/6.0.2xx-preview14' -v '6.0.200-preview'
``` 

> NOTE: the branch (`-b 'release/6.0.2xx-preview14'`) and version (`-v 6.0.200-preview`) parameters. The "preview 14" branch currently requires the 6.0.200 SDK band since the manifests are all in that SDK band - this will change in the future

Or, the "main" branch:

```
iwr https://aka.ms/dotnet/maui/maui-install.ps1 -OutFile maui-install.ps1;
.\maui-install.ps1 -b 'release/6.0.2xx-preview14' -v '6.0.200-preview'
``` 

> NOTE: the branch (`-b 'main'`) and version (`-v 6.0.200-preview`) parameters. The `main` branch currently requires the 6.0.200 SDK band since the manifests are all in that SDK band - this will change in the future

### iOS / MacCatalyst

iOS and MacCatalyst will require Xcode 13.1 Stable. You can get this [here](https://developer.apple.com/download/more/?name=Xcode).

### Android

Android API-31 (Android 12) is now the default in .NET 6.


## Running

### .NET 6

#### Compile with globally installed `dotnet`

This will build and launch Visual Studio using global workloads

```dotnetcli
dotnet tool restore
dotnet cake --target=VS-NET6 --workloads=global
```

#### Compile using a local `bin\dotnet`

You can run a `Cake` target to bootstrap .NET 6 in `bin\dotnet` and launch Visual Studio:

```dotnetcli
dotnet tool restore
dotnet cake --target=VS-NET6
```

_NOTES:_
- _VS Mac is not yet supported._
- _If the IDE doesn't show any Android devices try unloading and reloading the `Sample.Droid-net6` project._

You can also run commands individually:
```dotnetcli
# install local tools required to build (cake, pwsh, etc..)
dotnet tool restore
# Provision .NET 6 in bin\dotnet
dotnet build src\DotNet\DotNet.csproj
# Builds Maui MSBuild tasks
.\bin\dotnet\dotnet build Microsoft.Maui.BuildTasks.sln
# Builds the rest of Maui
.\bin\dotnet\dotnet build Microsoft.Maui.sln
# (Windows-only) to launch Visual Studio
dotnet cake --target=VS-DOGFOOD
```

To build & run .NET 6 sample apps, you will also need to use `.\bin\dotnet\dotnet` or just `dotnet` if you've installed the workloads globally:
```dotnetcli
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.Droid\Maui.Controls.Sample.Droid.csproj -t:Run
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.iOS\Maui.Controls.Sample.iOS.csproj -t:Run
```

Try out a "single project", you will need the `-f` switch to choose the platform:

```dotnetcli
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.SingleProject\Maui.Controls.Sample.SingleProject.csproj -t:Run -f net6.0-android
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.SingleProject\Maui.Controls.Sample.SingleProject.csproj -t:Run -f net6.0-ios
```

### Blazor Desktop

To build and run Blazor Desktop samples, check out the [Blazor Desktop](https://github.com/dotnet/maui/wiki/Blazor-Desktop) wiki topic.

### Win UI 3

To build and run WinUI 3 support, please install the additional components mentioned on the [Getting Started](https://docs.microsoft.com/en-us/dotnet/maui/get-started/installation) page and run:

```dotnetcli
dotnet tool restore
dotnet cake --target=VS-WINUI
```

### Android

To workaround a performance issue, all `Resource.designer.cs`
generation is disabled for class libraries in this repo.

If you need to add a new `@(AndroidResource)` value to be used from C#
code in .NET MAUI:

1. Comment out the `<PropertyGroup>` in `Directory.Build.targets` that
   sets `$(AndroidGenerateResourceDesigner)` and
   `$(AndroidUseIntermediateDesignerFile)` to `false`.

2. Build .NET MAUI as you normally would. You will get compiler errors
   about duplicate fields, but `obj\Debug\net6.0-android\Resource.designer.cs`
   should now be generated.

3. Open `obj\Debug\net6.0-android\Resource.designer.cs`, and find the
   field you need such as:

```csharp
// aapt resource value: 0x7F010000
public static int foo = 2130771968;
```

4. Copy this field to the `Resource.designer.cs` checked into source
   control, such as: `src\Controls\src\Core\Platform\Android\Resource.designer.cs`

5. Restore the commented code in `Directory.Build.targets`.
