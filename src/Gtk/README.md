# .NET MAUI on Linux with GTK 4

An add-on backend: it runs MAUI's platform-less (`net10.0`) assemblies on real GTK 4 widgets through
[GtkSharp4](https://github.com/pieroviano/GtkSharp) (native GTK 4 API, not the GTK 3 `Compat` surface). The stock
handlers on `net10.0` throw from `CreatePlatformView`; each GTK handler **subclasses** its stock handler, so `is ButtonHandler`
checks, `IButtonHandler` and user mapper customisations keep working.

## Layout

| Path | Assembly | Role |
|---|---|---|
| `src/Core` | `Microsoft.Maui.Gtk` | hosting, layout bridge, handlers, fonts, images, styling (this document) |
| `tests/Microsoft.Maui.Gtk.UnitTests` | | xUnit tests on real widgets (one shared project) |
| `samples/Maui.Gtk.Sample` | | code-only sample on `net10.0-gtk4.22` (SingleProject: `Platforms/Linux`, icon, image, font, asset); needs the `gtk` workload (without it, `MauiGtkWorkload.props` falls back to `net10.0` so solution load/restore works, and building the project fails with `MAUIGTK001`), so it is listed in `Microsoft.Maui.sln`/`-vscode.sln` without a build configuration (a solution build or `dotnet format` skips it) and is not part of `Microsoft.Maui-linux.slnf`; build the project directly |
| `samples/Maui.Gtk.BlazorSample` | | Blazor sample, same `gtk` workload requirement and solution/slnf treatment as `Maui.Gtk.Sample`; build the project directly |
| `src/Essentials` | `Microsoft.Maui.Essentials.Gtk` | Essentials on GTK 4 / Linux, see [`src/Essentials/README.md`](src/Essentials/README.md) |
| `../Graphics/src/Graphics.Cairo` | `Microsoft.Maui.Graphics.Cairo` | `ICanvas` on Cairo/Pango, see its [README](../Graphics/src/Graphics.Cairo/README.md) |
| `src/Controls` | `Microsoft.Maui.Controls.Gtk` | Controls handlers and `UseMauiGtk()`, see [`src/Controls/README.md`](src/Controls/README.md) |
| `src/BlazorWebView` | `Microsoft.AspNetCore.Components.WebView.Maui.Gtk` | `BlazorWebView` on WebKitGTK; also documents `WebView` and `HybridWebView`, see its [README](src/BlazorWebView/README.md) |
| `src/Maps` | `Microsoft.Maui.Controls.Maps.Gtk` | `Map` on OSM slippy tiles, see [`src/Maps/README.md`](src/Maps/README.md) |

Requires GTK >= 4.14 (guard newer APIs with `GtkRuntime.AtLeast(major, minor)`).

## Creating an app

An app head targets **`net10.0-gtk4.22`** (the GtkSharp `gtk` workload, target platform identifier `gtk`, constant `GTK`). NuGet resolves the
`lib/net10.0` assets of the MAUI packages for it.

**No `maui` workload (blocking check B5).** `UseMaui=true` makes the SDK load the `maui` workload manifest, which asks for workloads that do not
exist on Linux (`NETSDK1147: ... maui-tizen`). The GTK head therefore leaves `UseMaui` off and takes MAUI from packages
(MAUI 8+ already does not add package references for `UseMaui`; the app lists them). `SingleProject`, XAML, implicit usings and Resizetizer
come with the packages (`buildTransitive` of `Microsoft.Maui.Controls.Build.Tasks` and `Microsoft.Maui.Resizetizer`):

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0-gtk4.22</TargetFramework>
  <SingleProject>true</SingleProject>
  <ImplicitUsings>enable</ImplicitUsings>
  <ApplicationTitle>My App</ApplicationTitle>
  <ApplicationId>com.contoso.myapp</ApplicationId>
  <MauiVersion>10.0.130-gtk.*</MauiVersion>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Microsoft.Maui.Controls.Gtk" Version="$(MauiVersion)" />   <!-- Controls, Microsoft.Maui.Gtk, Essentials.Gtk, GtkSharp4 -->
  <PackageReference Include="Microsoft.Maui.Resizetizer" Version="$(MauiVersion)" PrivateAssets="all" />
  <!-- Blazor: Microsoft.AspNetCore.Components.WebView.Maui.Gtk -->
</ItemGroup>
```

There are no implicit package references for `gtk` (the SDK part would live in the `maui` workload, which does not exist on Linux): the explicit
references above are the contract. The interpreter is not forced for `gtk` (`Microsoft.Maui.Controls.Common.targets` names android, ios, maccatalyst,
windows and tizen only): the app runs on the JIT.

### From the template

| Step | Command |
|------|---------|
| .NET 10 SDK + `gtk` workload | Windows: GtkSharp `Docs/workload-install.md`. Linux/WSL: `eng/gtk/install-gtk-workload.sh [<SDK dir that has the workload>]` (copies manifest and packs into `~/.dotnet`) |
| GTK 4 runtime (>= 4.14) | Debian 13 / Ubuntu 24.04: `libgtk-4-1 libadwaita-1-0` (`libwebkitgtk-6.0-4` for web views); Windows: downloaded by the workload |
| Packages | `pwsh eng/gtk/pack-local.ps1` (see Local feed) puts `Microsoft.Maui.*` `10.0.130-gtk.*`, the GTK packages and `Microsoft.Maui.Templates.net10` in `Packages/` |
| Template | `dotnet new install Packages/Microsoft.Maui.Templates.net10.<version>.nupkg` (add `--debug:custom-hive <dir>` to keep your real templates untouched) |
| App | `dotnet new maui --linux -n MyApp` (or `dotnet new maui-blazor --linux`; `--maui-gtk-version` pins the package version). On a Linux host the project has only the GTK head; elsewhere `net10.0-gtk4.22` is added to the mobile/Windows TFMs |
| Feed | A `NuGet.config` next to the project with the feed folder (`Packages/`) and nuget.org |
| Build / run | `dotnet build -f net10.0-gtk4.22`, `dotnet run -f net10.0-gtk4.22`; headless: `GDK_BACKEND=x11 dbus-run-session -- xvfb-run -a dotnet run -f net10.0-gtk4.22` |

Verified on Debian 13 (GTK 4.18.6, headless Xvfb): `dotnet new maui --linux` and `dotnet new maui-blazor --linux` from the packed template build with the packed `-gtk` packages and run; the image, the `OpenSans` font from `MauiFont`, the icon files and the `.desktop` entry are next to the executable.

The template adds `Platforms/Linux/Program.cs` (a `MauiGtkApplication`), `.UseMauiGtk()` under `#if GTK` in `MauiProgram.cs`, and for the GTK head
only the `Microsoft.Maui.Controls.Gtk` (+ `Microsoft.AspNetCore.Components.WebView.Maui.Gtk`) and `Microsoft.Maui.Resizetizer` references.
`IncludeSampleContent` pulls Syncfusion and CommunityToolkit packages that have no GTK support: use it on Linux at your own risk.

### What the SDK build does for `gtk`

| Item | Result next to the executable |
|------|-------------------------------|
| `Platforms/Linux/**` | Compiled for `gtk` only (`LinuxProjectFolder`, `MauiPlatformSpecificFolder` with `TargetPlatformIdentifier="gtk"`); XAML there is removed from the other TFMs |
| `MauiImage` | `Resources/Images/<name>.png`, one 1.0 scale (GDK scales logical pixels; `GtkFileImageSourceService` looks there). Vectors are rasterised at `BaseSize` |
| `MauiFont` | `Resources/Fonts/<file>`; `ConfigureFonts(f => f.AddFont("X.ttf", "Alias"))` hands the file to the embedded font loader (Pango >= 1.56 `add_font_file`, else fontconfig) |
| `MauiAsset` | At its logical name (`FileSystem.OpenAppPackageFileAsync`) |
| `MauiIcon` | `Resources/Icons/hicolor/{48,128,256}/apps/<name>.png`; the app adds `Resources/Icons` to the icon theme and uses the icon as the default window icon |
| `.desktop` | `<ApplicationId>.desktop` (Name = `ApplicationTitle`, Exec = assembly name, Icon, `StartupWMClass`); `ApplicationCategories` sets `Categories`. Installers copy it to `share/applications` and make `Exec` absolute |
| `MauiSplashScreen` | Ignored (no splash mechanism in GTK) |
| Identity | `AssemblyMetadata` `ApplicationId`, `ApplicationTitle`, `ApplicationDisplayVersion`, `ApplicationVersion`: the `GApplication` id (when a valid D-Bus style id, else `org.maui.<assembly>`) and `AppInfo` |

## Entry point

```csharp
// Program.cs
public static int Main(string[] args) => new LinuxApp().Run(args);
class LinuxApp : MauiGtkApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

// MauiProgram.cs
builder.UseMauiApp<App>().UseMauiGtkCore();   // UseMauiGtk() (Controls.Gtk) = UseMauiGtkCore + Controls + Essentials
```

**Mappers.** Every GTK handler passes its own mapper to the stock base handler: `GtkMapperBuilder` puts the GTK implementation of a key
in front of the stock mapper (`MapGtk<TWidget>`: runs when the platform view is the expected GTK widget, then the stock mapping of the
same key runs). Controls' remaps and `AppendToMapping`/`PrependToMapping`/`ReplaceMapping` customisations of the *stock* mappers therefore still run, but
**always after** the GTK implementation: a `PrependToMapping` runs after the GTK widget was updated, and a `ModifyMapping`/`ReplaceMapping`
that skips the previous mapping cannot suppress the GTK update (customise the GTK handler's own mapper instead). There is no registration ordering rule between `UseMauiGtkCore()`/`UseMauiGtk()`, `UseMauiApp<T>()` and `ConfigureMauiHandlers` (`UseMauiGtkCore` wraps the `IMauiHandlersCollection` so `GtkHandlerMap.Apply` runs after all registrations; a more specific registration, e.g. a Controls-aware subclass, wins over the rewritten stock one). (Patching the static mappers in place was rejected:
Controls re-maps them lazily, e.g. `Label`'s static constructor, with `ReplaceMapping`, which discards the previous mapping.)

`MauiGtkApplication.Run(args)` owns a `Gtk.Application` (`startup` builds the app, `activate` creates the first window).
Pass only GApplication options in `args`. Lifecycle: `ConfigureLifecycleEvents(e => e.AddGtk(g => g.OnStartup(...).OnActivate(...).OnShutdown(...).OnWindowCreated(...)))`.

## Architecture

| Concern | Implementation |
|---|---|
| Handler substitution | `GtkHandlerMap` (stock type -> GTK type); `GtkHandlerMap.Apply` rewrites registered `ServiceDescriptor`s and adds interface defaults. Other assemblies call `GtkHandlerMap.Add`. |
| Property mapping | `GtkMapperBuilder`/`GtkCommandMapperBuilder` per handler (static `GtkMapper`/`GtkCommandMapper`); the shared view properties in `GtkViewMappers`. |
| Layout | `MauiGtkPanel` (custom `Widget`, `ICrossPlatformLayoutBacking`): `OnMeasure` -> `CrossPlatformMeasure`, `OnSizeAllocate` -> `CrossPlatformArrange`; children allocated inside the parent allocate by `GtkPlatformViewBackend` (Core's `PlatformViewBackend` hook). Minimum size 0 (scroll content: follows content). `MauiGtkRootPanel` asks for 0x0 and measures before arranging. |
| Container (clip/shadow) | `GtkWrapperView`, a single-child widget set as `ContainerView`. Clip = any `IShape` via `Gsk.Path`; shadow = outset shadow. |
| Platform offset | `GtkViewState.PlatformOffsetX/Y`: a translation the platform applies on top of `TranslationX/Y` (swipe, refresh), never overwritten by the view's transform mappers. |
| Menus | `GtkMenuNode` tree (`GtkMenuModel.cs`) mirrors MAUI menus as `GLib.Menu` + `GSimpleAction`s (group `menu`); the root rebuilds the models on every change. `GtkWindowMenuBar` stacks the `PopoverMenuBar` above the root panel; `GtkContextFlyout` installs the secondary click / long press. `MenuFlyoutItemHandler` has no mapper constructor, so its GTK mappings are added once to the stock static mapper (they only act for `GtkMenuFlyoutItemHandler`). |
| Transforms | translation/scale/rotation (+ anchor) become the `Gsk.Transform` of the child allocation (`GtkViewState`). A transform change stores the values and queues an allocation of the parent, whose arrange re-applies them via `PlatformArrange` (a child is only allocated inside its parent's size-allocate). `RotationX/Y` ignored. |
| Styling | `GtkStyle`: one `CssProvider` + class per widget (the only `StyleContext.AddProvider` use). Colour, font, spacing, decorations, padding, background (solid/linear/radial), borders are CSS. |
| Dispatcher / ticker | `GtkDispatcher` (`GLib.Idle/Timeout`, inline on the GTK thread), `GtkTicker` (frame-clock tick callback on a main (non-modal, non-transient) window, re-anchored when that window unmaps; `Timeout` fallback). |
| Fonts | `GtkFontManager` (`Font` -> CSS / `Pango.FontDescription`); `GtkEmbeddedFontLoader` (`pango_font_map_add_font_file` on Pango >= 1.56, else fontconfig `FcConfigAppFontAddFile`). Fonts registered by file name are opened from `Resources/Fonts`, `Fonts` or the app folder through `FontRegistrar.NativeFontStreamProvider` (Core hook, installed by `UseMauiGtkCore`; null = upstream Standard behaviour, font unresolved). |
| Images | `IGtkImageSourceService` -> `Gdk.Texture` for File/Stream/Uri/Font; `GtkImageLoader` replaces `ImageSourcePartLoader` (a no-op on `net10.0`). |

## Supported controls

| Control | GTK widget | Notes |
|---|---|---|
| Application, Window | `Gtk.Application`, `Gtk.Window` | no X/Y/max size (GTK 4 cannot); Width/Height = default size, Minimum* = size request |
| ContentPage, ContentView, Layouts (Stack/Grid/Absolute/Flex) | `MauiGtkPanel` | |
| Label | `Gtk.Label` | wraps by default; `GtkLabelHandler.SetMaxLines` for `MaxLines` |
| Button | `MauiGtkButton` (image + label) | Clicked/Pressed/Released, stroke, corner radius |
| Image | `Gtk.Picture` | static; `IsAnimationPlaying` ignored |
| Entry | `Gtk.Entry` | return type, vertical alignment: no GTK equivalent |
| Editor | `MauiGtkEditor` (overlay, scrolled `TextView`) | placeholder label, grows with text |
| ScrollView | `Gtk.ScrolledWindow` | programmatic scrolling is instant |
| Border | `MauiGtkBorder` | any `IShape`; gradient fills through Cairo |
| Switch, CheckBox | `Gtk.Switch`, `Gtk.CheckButton` | |
| ActivityIndicator | `Gtk.Spinner` | `IsRunning` = spinning; colour = CSS `color` |
| ProgressBar | `Gtk.ProgressBar` | colour on the `progress` node; RTL inverts |
| Slider | `Gtk.Scale` | track/thumb colours are CSS; `ThumbImageSource` ignored; `DragStarted/Completed` from a capture-phase click |
| Stepper | `MauiGtkStepper` (linked "-"/"+" buttons) | MAUI semantics: clamped, a button is insensitive at its bound |
| Picker | `MauiGtkPicker` (`Gtk.DropDown` + `Gtk.StringList`) | a drop-down always has a selection, so row 0 is the title row = "no selection"; text alignment ignored |
| DatePicker | `MauiGtkDatePicker` (`MenuButton` + `Popover` + `Calendar`) | min/max clamp the date; label uses `Format` |
| TimePicker | `MauiGtkTimePicker` (`MenuButton` + `Popover` + 2 `SpinButton`) | label uses `Format` on the time of day |
| RadioButton | `MauiGtkRadioButton` (`CheckButton` in a private group) | MAUI owns grouping; string content = label, view content next to the indicator, template content replaces the indicator |
| SearchBar | `Gtk.SearchEntry` | immediate text updates, `activate` = `SearchButtonPressed`; `ReturnType` ignored; `MaxLength` by truncation |
| ImageButton | `MauiGtkImageButton` (`Button` + `Picture`) | like Button: clicked/pressed/released, stroke, corner radius |
| GraphicsView | `MauiGtkGraphicsView` (`DrawingArea` + `CairoCanvas`) | start/drag/end/cancel and hover interactions |
| Shapes (`ShapeViewHandler`) | `MauiGtkGraphicsView` + `ShapeDrawable` | fill/stroke/dash/cap/join/miter repaint; the background is painted by the shape (no CSS background) |
| IndicatorView | `MauiGtkGraphicsView` drawing dots | natural size = the dots; shape from `IndicatorsShape` |
| RefreshView | `MauiGtkRefreshPanel` | pull = capture-phase scroll up while the content is at its top (100 px); spinner over the top edge |
| SwipeView, SwipeItem(View) | `MauiGtkSwipePanel` | `GestureDrag`; content slides by `GtkViewState.PlatformOffsetX/Y`; reveal/execute, `RequestOpen/Close`; not animated |
| MenuBar, MenuBarItem | `GMenu` tree in a `Gtk.PopoverMenuBar` above the window content | items are `GSimpleAction`s of the `menu` group; icons not shown; accelerators shown, not bound |
| MenuFlyout*, `ContextFlyout` | `Gtk.PopoverMenu` over the `GMenu` | secondary click or long press on any view; separators split sections |
| WebView, HybridWebView | `WebKit.WebView` (WebKitGTK 6; a `Gtk.Label` when it cannot be loaded) | see [`src/BlazorWebView/README.md`](src/BlazorWebView/README.md) |

Not in this area: Shell, CollectionView, Maps, NavigationPage/Toolbar/FlyoutPage/TabbedPage. Image-paint backgrounds are unsupported.

## Tests

| Where | Command |
|---|---|
| Windows | `dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests` (GTK 4 runtime in `%LOCALAPPDATA%\Gtk\4.22.4\bin` or on `PATH`) |
| Linux / WSL (headless) | `GDK_BACKEND=x11 dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests` |
| Smoke | `dotnet run --project src/Gtk/samples/Maui.Gtk.Sample -- --smoke` (prints `SMOKE OK`; checks the SDK-copied image, font, icon, asset and `.desktop`; needs the `gtk` workload) |
| Core hook tests | `dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj --filter "FullyQualifiedName~PlatformViewBackendTests"` |

| Project | Tests (Windows GTK 4.22 / Debian 13 GTK 4.18, xvfb) |
|---------|------|
| `Microsoft.Maui.Gtk.UnitTests` (Core + Essentials + Handlers + Controls + Web + Maps) | Windows 648 passed, 21 skipped (17 WebKit + 4 Linux-only popups); Debian 13 (GTK 4.18) 669 passed, 0 skipped |
| `Graphics.Cairo.Tests` | 125 passed, 0 skipped |
| `Essentials.UnitTests` (net10.0) | 441 passed, 0 skipped |
| `Core.UnitTests` / `Controls.Core.UnitTests` (net10.0) | 884 (+3 skipped) / 5869 (+30 skipped) passed |
| `Resizetizer.UnitTests` (43 for `gtk`: images, icon, `.desktop`, dpi paths) | 637 passed, 2 skipped |

- One shared test project; Essentials tests (`Essentials/`) run on the same GTK thread through `GtkTestHost` (a second GTK thread crashes the process); the host is OLE initialized on Windows and iterates the main context in the background once `GtkTestHost.Post` is used.
- Tests use `[GtkFact]`: skipped when no GTK/display, a failure when `MAUI_GTK_REQUIRE_NATIVE` is set.
- GTK runs on one dedicated (STA on Windows) thread; test bodies go through `GtkTestHost.Run`. No `async` bodies, never `Destroy()` a window.
- The host captures the native stderr, so layout violations ("Allocating size ... without calling gtk_widget_measure"), CSS parser errors
  and other GTK criticals fail the tests that call `GtkTestHost.AssertNoComplaints()`.
- GTK validates CSS from the frame clock: after a style change wait with `GtkTestHost.PumpUntil`, do not assume one `Pump` is enough.
- Public API: `PublicAPI/net/PublicAPI.*.txt` (generated in Debug by `PublicAPI.targets`).

## Known limitations

- Popups (`Picker.IsOpen`, `DatePicker/TimePicker.IsOpen`, context flyouts) pop a `Gtk.Popover` up, which crashes the process with the gvsbuild runtime on Windows: those tests are `[GtkLinuxFact]` (skipped on Windows, run headless on Linux).
- The Controls shape handlers (`Line`, `Path`, `Polygon`, `Polyline`, `Rectangle`, `RoundRectangle`, `BoxView`) derive from `ShapeViewHandler`; their GTK subclasses belong to `Microsoft.Maui.Controls.Gtk` and can reuse `GtkShapeViewBehavior` (platform view, repaint mappings). Only `Ellipse` is covered by Core.
- `SwipeItemView` is resolved by Controls to the content view handler (it is a `ContentView`); `Microsoft.Maui.Controls.Gtk` registers `SwipeItemView -> SwipeItemViewHandler`.
- Windows builds pack/test `net10.0`/`netstandard` only (mobile/Windows TFMs are disabled locally by `Directory.Build.Override.props`).
- `RotationX`/`RotationY`, window position, maximum window size, image-paint backgrounds, animated scrolling.
- Accessibility goes through `Widget.Accessible` (GtkSharp's compat wrapper) for name and description only.

## Building

No mobile or Windows workloads are needed. Create the local, gitignored `Directory.Build.Override.props` at the repo root
(and in every git worktree; it is not tracked):

```xml
<Project>
  <PropertyGroup>
    <IncludeAndroidTargetFrameworks>false</IncludeAndroidTargetFrameworks>
    <IncludeIosTargetFrameworks>false</IncludeIosTargetFrameworks>
    <IncludeMacCatalystTargetFrameworks>false</IncludeMacCatalystTargetFrameworks>
    <IncludeMacOSTargetFrameworks>false</IncludeMacOSTargetFrameworks>
    <IncludeTizenTargetFrameworks>false</IncludeTizenTargetFrameworks>
    <IncludeWindowsTargetFrameworks>false</IncludeWindowsTargetFrameworks>
  </PropertyGroup>
</Project>
```

| Task | Command |
|------|---------|
| Build the Linux subset | `dotnet build Microsoft.Maui-linux.slnf` (projects listed from `Microsoft.Maui-vscode.sln`; add new `src/Gtk` projects to both) |
| Core unit tests | `dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj` |
| SDK | `global.json` lists `.dotnet` then `$host$`: with no provisioned `.dotnet`, the `dotnet` on `PATH` is used (any 10.0+ SDK; TFMs built are `netstandard2.0/2.1` and `net10.0`) |
| Visual Studio on `Microsoft.Maui.sln` | Launch it with `pwsh eng/scripts/Start-VisualStudio.ps1 -Solution Microsoft.Maui.sln` (builds the build tasks, sets `DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR` to `.dotnet`). A plain launch takes the SDK from `.dotnet` but the workload manifests from `C:\Program Files\dotnet`, so the mobile projects that hardcode their TFM (`GraphicsTester.*`, `Compatibility.*.Android/iOS`, `Benchmarks.Droid`) fail with MSB4236 |
| GTK runtime | Linux: `libgtk-4-1`, `libadwaita-1-0`; Windows: `%LOCALAPPDATA%\Gtk\4.22.4\bin` |

## WSL / Linux

The tree can be built from Windows and from WSL (`/mnt/d/...`) without the two clobbering each other:
`Directory.Build.props` moves the Arcade `ArtifactsDir` to `artifacts-wsl/` when MSBuild runs on a non-Windows OS from a `/mnt/` path
(`-p:UseOSSpecificOutputPaths=true|false` forces it). NuGet caches are per-OS anyway.

| Script | Purpose |
|--------|---------|
| `eng/gtk/wsl-setup.sh` | Once: installs a .NET 10 SDK into `~/.dotnet` (never into the repo's Windows `.dotnet`), checks `xvfb`, `dbus`, GTK 4 libs |
| `eng/gtk/wsl-test.sh <proj>... [-- args]` | Runs test projects headless: `dbus-run-session` + `xvfb-run`, `GDK_BACKEND=x11`, output in `artifacts-wsl/` |
| `eng/gtk/refresh-gtk-packages.sh` | Purges the forked packages from the Linux NuGet cache after a repack |

```bash
wsl -d Debian -- bash /mnt/d/CommonLibrary/maui/eng/gtk/wsl-setup.sh
wsl -d Debian -- bash /mnt/d/CommonLibrary/maui/eng/gtk/wsl-test.sh src/Core/tests/UnitTests/Core.UnitTests.csproj
```

GtkSharp4 restores from nuget.org. Under WSL the `Packages` junction is not readable (an NTFS reparse point): an app consuming the forked packages restores with `RestoreAdditionalProjectSources=/mnt/d/Starb/Packages`.

Do not run `./build.sh -restore` from WSL: it would provision the Linux SDK into the shared `.dotnet`.

## Local feed (forked packages)

`Packages/` is a junction to the shared folder `D:\Starb\Packages` that holds the forked MAUI packages. It is not a package source of this repository (`NuGet.config` restores GtkSharp4 from nuget.org); apps built from the `--linux` template add it in their own `NuGet.config`.

- `pwsh eng/gtk/pack-local.ps1 [-Only <substr>,...] [-Configuration Debug] [-Revision n] [-NoRestore] [-WhatIf]` packs, in dependency order, Graphics, Graphics.Cairo, Essentials, Core, Controls (Build.Tasks, Xaml, Core, SourceGen, BindingSourceGen, NuGet), Resizetizer, Maps, Controls.Maps, BlazorWebView and every project under `src/Gtk/src`; projects that do not exist are skipped.
- Version `10.0.130-gtk.<stamp>.<rev>`: Arcade properties `OfficialBuild=true`, `OfficialBuildId=<yyyyMMdd>.<rev>`, `PreReleaseVersionLabel=gtk`. `<stamp>` is Arcade's `yy*1000 + mm*50 + dd`; `<rev>` is the next free revision of the day, so a repack always yields a new, higher version (never a stable `10.0.130`). Pack a whole set: dependents reference their dependencies at the same revision.
- `eng/gtk/LocalFeed.targets` (imported by `Directory.Build.targets` only when `-p:MauiGtkLocalFeedDir` is set) runs after `Pack`: copies the nupkg into the feed, deletes older `<id>.*-gtk.*` files of exactly that id, and deletes the old and new version folders from the Windows NuGet cache. It refuses non `-gtk.` versions.
- Only the TFMs enabled by `Directory.Build.Override.props` are packed (see Building), so packages built on a machine without mobile workloads carry no mobile/Windows assets.
- Under WSL run `eng/gtk/refresh-gtk-packages.sh` after each repack.
- Self-test of the feed logic (12 checks, no MAUI build): `pwsh eng/gtk/tests/LocalFeed.Tests.ps1`.
