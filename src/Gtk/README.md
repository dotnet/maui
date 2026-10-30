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
| `samples/Maui.Gtk.Sample` | | code-only sample (plain `net10.0` for now) |
| `src/Controls`, `src/Essentials`, `src/BlazorWebView`, `src/Maps` | `*.Gtk` | other areas (separate tracks, see their own sections) |

Requires GTK >= 4.14 (guard newer APIs with `GtkRuntime.AtLeast(major, minor)`).

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

**Ordering rule.** `UseMauiGtkCore()` / `UseMauiGtk()` must come right after `UseMauiApp<T>()` and before any
`AppendToMapping`/`PrependToMapping`. The backend patches the stock *static* mappers in place with a GTK branch
(`MapGtk`, runs only when `PlatformView` is the expected GTK widget); user customisations added afterwards wrap it.
`UseMauiApp` re-maps those statics on every call, so `UseMauiGtkCore()` re-installs any patch that was shadowed
(`GtkMapperRegistration.Apply()` is idempotent).

`MauiGtkApplication.Run(args)` owns a `Gtk.Application` (`startup` builds the app, `activate` creates the first window).
Pass only GApplication options in `args`. Lifecycle: `ConfigureLifecycleEvents(e => e.AddGtk(g => g.OnStartup(...).OnActivate(...).OnShutdown(...).OnWindowCreated(...)))`.

## Architecture

| Concern | Implementation |
|---|---|
| Handler substitution | `GtkHandlerMap` (stock type -> GTK type); `GtkHandlerMap.Apply` rewrites registered `ServiceDescriptor`s and adds interface defaults. Other assemblies call `GtkHandlerMap.Add`. |
| Property mapping | `GtkMapperExtensions.MapGtk<TView, THandler, TWidget>`; view-level patches in `GtkViewMappers`. |
| Layout | `MauiGtkPanel` (custom `Widget`, `ICrossPlatformLayoutBacking`): `OnMeasure` -> `CrossPlatformMeasure`, `OnSizeAllocate` -> `CrossPlatformArrange`; children allocated inside the parent allocate by `GtkPlatformViewBackend` (Core's `PlatformViewBackend` hook). Minimum size 0 (scroll content: follows content). `MauiGtkRootPanel` asks for 0x0 and measures before arranging. |
| Container (clip/shadow) | `GtkWrapperView`, a single-child widget set as `ContainerView`. Clip = any `IShape` via `Gsk.Path`; shadow = outset shadow. |
| Transforms | translation/scale/rotation (+ anchor) become the `Gsk.Transform` of the child allocation (`GtkViewState`). `RotationX/Y` ignored. |
| Styling | `GtkStyle`: one `CssProvider` + class per widget (the only `StyleContext.AddProvider` use). Colour, font, spacing, decorations, padding, background (solid/linear/radial), borders are CSS. |
| Dispatcher / ticker | `GtkDispatcher` (`GLib.Idle/Timeout`, inline on the GTK thread), `GtkTicker` (frame-clock tick callback, `Timeout` fallback). |
| Fonts | `GtkFontManager` (`Font` -> CSS / `Pango.FontDescription`); `GtkEmbeddedFontLoader` (`pango_font_map_add_font_file` on Pango >= 1.56, else fontconfig `FcConfigAppFontAddFile`). |
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

Not in this area: Picker/Slider/..., Shell, CollectionView, Essentials, WebView, Maps. Image-paint backgrounds are unsupported.

## Tests

| Where | Command |
|---|---|
| Windows | `dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests` (GTK 4 runtime in `%LOCALAPPDATA%\Gtk\4.22.4\bin` or on `PATH`) |
| Linux / WSL (headless) | `GDK_BACKEND=x11 dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests` |
| Smoke | `dotnet run --project src/Gtk/samples/Maui.Gtk.Sample -- --smoke` (prints `SMOKE OK`) |
| Core hook tests | `dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj --filter "FullyQualifiedName~PlatformViewBackendTests"` |

- Tests use `[GtkFact]`: skipped when no GTK/display, a failure when `MAUI_GTK_REQUIRE_NATIVE` is set.
- GTK runs on one dedicated (STA on Windows) thread; test bodies go through `GtkTestHost.Run`. No `async` bodies, never `Destroy()` a window.
- The host captures the native stderr, so layout violations ("Allocating size ... without calling gtk_widget_measure"), CSS parser errors
  and other GTK criticals fail the tests that call `GtkTestHost.AssertNoComplaints()`.
- GTK validates CSS from the frame clock: after a style change wait with `GtkTestHost.PumpUntil`, do not assume one `Pump` is enough.
- Public API: `PublicAPI/net/PublicAPI.*.txt` (generated in Debug by `PublicAPI.targets`).

## Known limitations

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

Do not run `./build.sh -restore` from WSL: it would provision the Linux SDK into the shared `.dotnet`.

## Local feed (forked packages)

`Packages/` is a junction to the shared feed `D:\Starb\Packages` (`NuGet.config`, key `starb`); it holds the GtkSharp4 packages and the forked MAUI packages.

- `pwsh eng/gtk/pack-local.ps1 [-Only <substr>,...] [-Configuration Debug] [-Revision n] [-NoRestore] [-WhatIf]` packs, in dependency order, Graphics, Graphics.Cairo, Essentials, Core, Controls (Build.Tasks, Xaml, Core, SourceGen, BindingSourceGen, NuGet), Resizetizer, Maps, Controls.Maps, BlazorWebView and every project under `src/Gtk/src`; projects that do not exist are skipped.
- Version `10.0.130-gtk.<stamp>.<rev>`: Arcade properties `OfficialBuild=true`, `OfficialBuildId=<yyyyMMdd>.<rev>`, `PreReleaseVersionLabel=gtk`. `<stamp>` is Arcade's `yy*1000 + mm*50 + dd`; `<rev>` is the next free revision of the day, so a repack always yields a new, higher version (never a stable `10.0.130`). Pack a whole set: dependents reference their dependencies at the same revision.
- `eng/gtk/LocalFeed.targets` (imported by `Directory.Build.targets` only when `-p:MauiGtkLocalFeedDir` is set) runs after `Pack`: copies the nupkg into the feed, deletes older `<id>.*-gtk.*` files of exactly that id, and deletes the old and new version folders from the Windows NuGet cache. It refuses non `-gtk.` versions.
- Only the TFMs enabled by `Directory.Build.Override.props` are packed (see Building), so packages built on a machine without mobile workloads carry no mobile/Windows assets.
- Under WSL run `eng/gtk/refresh-gtk-packages.sh` after each repack.
- Self-test of the feed logic (12 checks, no MAUI build): `pwsh eng/gtk/tests/LocalFeed.Tests.ps1`.
