# Developer Guide

This page contains steps to build and run the .NET MAUI repository from source.

## Requirements

- [Install .NET6](https://github.com/dotnet/maui/wiki/Installing-.NET-6)

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
.\bin\dotnet\dotnet build Microsoft.Maui.BuildTasks-net6.sln
# Builds the rest of Maui
.\bin\dotnet\dotnet build Microsoft.Maui-net6.sln
# (Windows-only) to launch Visual Studio
dotnet cake --target=VS-DOGFOOD
```

To build & run .NET 6 sample apps, you will also need to use `.\bin\dotnet\dotnet` or just `dotnet` if you've installed the workloads globally:
```dotnetcli
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.Droid\Maui.Controls.Sample.Droid-net6.csproj -t:Run
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.iOS\Maui.Controls.Sample.iOS-net6.csproj -t:Run
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
