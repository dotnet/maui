# Introducing .NET Multi-platform App UI (MAUI)

MAUI is:

* Multi-platform native UI
* Deploy to multiple devices across mobile & desktop
* Using a single project, single codebase
* Evolution of [Xamarin.Forms](https://github.com/xamarin/xamarin.forms)
* Targeting .NET 6, previews end of year

## Status: Planning and Development

While [Xamarin.Forms](https://github.com/xamarin/xamarin.forms) continues to be actively developed to meet customer needs today, we are [proposing evolutionary changes](../../issues) during this planning phase based on some early customer interviews of what would be most beneficial.

Active development is happening today to build Android and iOS SDKs against the next version of .NET. [Samples may be found here](https://github.com/xamarin/net5-samples).

### Goals

* Improve app performance
* Improve simplicity of control extensibility
* Improve simplicity of contributing
* Enable developer options to use Model-View-Update (MVU) and Blazor

### Roadmap

.NET MAUI and mobile SDK support will ship in concert with .NET 6. At present we do not have a shipping schedule for .NET 6.

### Milestones

* MAUI previews Q4 2020 through Q3 2021
  * [Renderer architecture revisions](https://github.com/dotnet/maui/issues/28)
  * Source solution and project simplification
  * Completete approved proposals
  * Implement MVU
* MAUI release candidate September 2021
* MAUI general availability November 2021

## Xamarin.Forms vs MAUI


|  |Xamarin.Forms  |MAUI  |
|---------|---------|---------|
|**Platforms**     |         |         |
|Android     |API 19+        |API 21+        |
|iOS     |9-13         |10+         |
|Linux     |Community         |Community         |
|macOS     |Community         |Microsoft         |
|Tizen     |Samsung           |Samsung           |
|Windows     |UWP Microsoft<br/>WPF Community         |Microsoft         |
|**Features**     |         |         |
|Renderers     |Tightly coupled to BindableObject         |Loosely coupled, no Core dependencies         |
|App Models     |MVVM, RxUI         |MVVM, RxUI, MVU, Blazor         |
|Single Project     |No         |Yes         |
|Multi-targeting     |No         |Yes         |
|Multi-window     |No         |Yes         |
|**Misc**     |         |         |
|.NET     |Xamarin.iOS, Xamarin.Android, Mono, .NET Framework, ...         |.NET 6+         |
|Acquisition |NuGet & Visual Studio Installer |dotnet |
|Project System     |Franken-proj         |SDK Style         |
|dotnet CLI     |No         |Yes         |
|**Tools**     |         |         |
|Visual Studio 2019     |Yes         |Yes         |
|Visual Studio 2019 for Mac     |Yes         |Yes         |
|Visual Studio Code     |No         |Yes         |

## How to Engage, Contribute, and Give Feedback

Some of the best ways to [contribute](./CONTRIBUTION.md) are to try things out, file issues, join in design conversations,
and make pull-requests. Proposals for changes specific to MAUI can be found [here for discussion](../../issues).

## Code of conduct

See [CODE-OF-CONDUCT](./CODE-OF-CONDUCT.md)
