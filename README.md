# Introducing .NET Multi-platform App UI (MAUI)

.NET MAUI is:

* Multi-platform native UI
* Deploy to multiple devices across mobile & desktop
* Using a single project, single codebase
* Evolution of [Xamarin.Forms](https://github.com/xamarin/xamarin.forms)
* .NET 6

## Build Status ##


| Private  | Public |
|--|--|
|  [![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/MAUI?repoName=dotnet%2Fmaui&branchName=main)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=13330&repoName=dotnet%2Fmaui&branchName=main) |   [![Build Status](https://dev.azure.com/xamarin/public/_apis/build/status/MAUI-public?repoName=dotnet%2Fmaui&branchName=main)](https://dev.azure.com/xamarin/public/_build/latest?definitionId=57&repoName=dotnet%2Fmaui&branchName=main) |


## Maui.sln

### SDKS required
- Install the SDKs listed here https://github.com/xamarin/net6-samples

- And/or run the following

```
dotnet tool install Cake.Tool -g
```

### Running

#### IDE
- If you have Visual Studio 16.9 P4 installed you can open `Maui.sln` and run it from there.

#### .NET 6

You can run a `Cake` target to bootstrap .NET 6 in `bin\dotnet` and launch Visual Studio:

```dotnetcli
dotnet cake --target=VS-NET6
```
_NOTE: VS Mac is not yet supported._

You can also run commands individually:
```dotnetcli
# Provision .NET 6 in bin\dotnet
dotnet build src\DotNet\DotNet.csproj
# Builds Maui MSBuild tasks
.\bin\dotnet\dotnet build Microsoft.Maui.BuildTasks-net6.sln
# Builds the rest of Maui
.\bin\dotnet\dotnet build Microsoft.Maui-net6.sln
# (Windows-only) to launch Visual Studio
.\eng\dogfood.ps1
```

To build & run .NET 6 sample apps, you will also need to use `.\bin\dotnet\dotnet`:
```dotnetcli
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.Droid\Maui.Controls.Sample.Droid-net6.csproj -t:Run
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.iOS\Maui.Controls.Sample.iOS-net6.csproj -t:Run
```

Try try out a "single project", you will need the `-f` switch to choose the platform:

```dotnetcli
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.SingleProject\Maui.Controls.Sample.SingleProject.csproj t:Run -f net6.0-android --no-restore
.\bin\dotnet\dotnet build src\Controls\samples\Controls.Sample.SingleProject\Maui.Controls.Sample.SingleProject.csproj t:Run -f net6.0-ios --no-restore
```

Note that `--no-restore` is a workaround until [dotnet#15485](https://github.com/dotnet/sdk/issues/15485) is fixed in a future preview.

## Current News

[March 11, 2021 - Announcing .NET 6 Preview 2](https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-2/)

Watch our [March 2021 .NET Community Standup report](https://youtu.be/NEbRo0ltniM?t=1242) for the latest information about our progress on .NET MAUI and .NET 6. 

[![](https://user-images.githubusercontent.com/41873/110172514-e1c76280-7dc2-11eb-8407-50760881d1ec.png
)](https://www.youtube.com/watch?v=5bK2ICHtMxo)

Additional live streams and presentations:

* February 25, 2021 - David Ortinau and Maddy Leger at .NET Conf: Focus on Windows: [The Future of Client App Development in .NET 6](https://www.youtube.com/watch?v=fPEdgXeqhE4)
* January 28, 2021 - David Ortinau at .NET Frontend Day: [A .NET MAUI Progress Report](https://youtu.be/RnyZZKjdUxk)
* September 19, 2020 - Shane Neuville at ReactiveUI Virtual Conference: [Dual Screen, .NET MAUI, and RxUI](https://www.youtube.com/watch?v=Rkz6Dkk1uWU)
* October 3, 2020 - James Clancey at Xamarin Expert Day: [.NET MAUI MVU](https://www.youtube.com/watch?v=9kwrgm_-FCk)
* October 3, 2020 - David Ortinau at Xamarin Expert Day: [Introducing .NET MAUI](https://youtu.be/qbHO8J3bId0)
* December 2, 2020 - Javier Suarez Ruiz at MonkeyConf: [.NET MAUI Handlers](https://youtu.be/TBMauxRGkiI) (Spanish)

## Status: Active Development

While [Xamarin.Forms](https://github.com/xamarin/xamarin.forms) continues to be actively supported through November 2022, we are making evolutionary changes based on customer research of what would be most beneficial. Current areas of focus are:

* Porting renderers to handlers ([spec](https://github.com/dotnet/maui/issues/28))
* Adapting layouts for handlers
* WinUI 3 preview 3 early spike ([branch](https://github.com/xamarin/Xamarin.Forms/tree/winui3))
* Mac Catalyst early evaluation ([wiki](https://github.com/xamarin/xamarin-macios/wiki/Mac-Catalyst-(Early-Preview)))

Active development is happening today to build Android and iOS SDKs against .NET 6. [Samples may be found here](https://github.com/xamarin/net6-samples).

### Goals

* Improve app performance
* Improve simplicity of control extensibility
* Improve simplicity of contributing
* Enable developer options to use Model-View-Update (MVU) and Blazor

### Roadmap

.NET MAUI and mobile SDK support will ship in concert with .NET 6. At present we do not have a shipping schedule for .NET 6.

### Milestones

* .NET MAUI previews Q4 2020 through Q3 2021
  * [Renderer architecture revisions](https://github.com/dotnet/maui/issues/28)
  * Source solution and project simplification
  * Complete approved proposals
  * Implement MVU (experimental)
* .NET MAUI release candidate September 2021
* .NET MAUI general availability November 2021

## Xamarin.Forms vs .NET MAUI


|  |Xamarin.Forms  |.NET MAUI  |
|---------|---------|---------|
|**Platforms**     |         |         |
|Android     |API 19+        |API 21+        |
|iOS     |9-15         |10+         |
|Linux     |Community         |Community         |
|macOS     |Community         |Microsoft         |
|Tizen     |Samsung           |Samsung           |
|Windows     |UWP Microsoft<br/>WPF Community         |Microsoft<sup>*</sup>         |
|**Features**     |         |         |
|Renderers     |Tightly coupled to BindableObject         |Loosely coupled, no Xamarin.Forms dependencies         |
|App Models     |MVVM         |MVVM         |
|     |RxUI         |RxUI         |
|     |             |MVU <sup>**</sup> |
|     |             |Blazor <sup>**</sup> |
|Single Project     |No         |Yes         |
|Multi-targeting     |No         |Yes         |
|Multi-window     |No         |Yes         |
|**Misc**     |         |         |
|.NET     |Xamarin.iOS, Xamarin.Android, Mono, .NET Framework, ...         |.NET 6+         |
|XAML Hot Reload|Experimental: SDK 4.x & Visual Studio 2019 prior to version 16.9<br>Feature Complete: SDK 5.x & Visual Studio 2019 version 16.9 or newer|Yes|
|.NET Hot Reload|iOS/Android – No<br>UWP – Limited support for runtime edits using .NET “Edit & Continue”|Yes|
|Acquisition |NuGet & Visual Studio Installer |dotnet |
|Project System     |Franken-proj         |SDK Style         |
|dotnet CLI     |No         |Yes         |
|**Tools**     |         |         |
|Visual Studio 2019     |Yes         |Yes         |
|Visual Studio 2019 for Mac     |Yes         |Yes         |
|Visual Studio Code     |No         |Experimental<sup>***</sup>         |

<sup>*</sup> The Windows implementation is expected to be WinUI 3, pending GA release. 

<sup>**</sup> These app models are experimental.

<sup>***</sup> Visual Studio Code will work by virtue of .NET unification, however not all experiences that make .NET MAUI development delightful (intellisense for example) may be enabled at the time of .NET 6 release.

## FAQs

Do you have questions? Do not worry, we have prepared a complete [FAQ](https://github.com/dotnet/maui/wiki/FAQs) answering the most common questions.

## How to Engage, Contribute, and Give Feedback

Some of the best ways to [contribute](./.github/CONTRIBUTING.md) are to try things out, file issues, join in design conversations,
and make pull-requests. Proposals for changes specific to MAUI can be found [here for discussion](https://github.com/dotnet/maui/issues).

See [CONTRIBUTING](./.github/CONTRIBUTING.md)

## Code of conduct

See [CODE-OF-CONDUCT](./.github/CODE_OF_CONDUCT.md)
