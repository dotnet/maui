# .NET MAUI on Linux (GTK 4)

Linux backend for .NET MAUI on GTK 4 (GtkSharp4 bindings, GTK >= 4.14). Handlers subclass the stock MAUI handlers and
reach the GTK widgets through `PlatformViewBackend` (see `docs/plans/linux-gtk4-backend.md`).

| Project | Path |
|---------|------|
| Microsoft.Maui.Gtk | `src/Gtk/src/Core` |
| Microsoft.Maui.Controls.Gtk | `src/Gtk/src/Controls` |
| Microsoft.Maui.Essentials.Gtk | `src/Gtk/src/Essentials` |
| Microsoft.AspNetCore.Components.WebView.Maui.Gtk | `src/Gtk/src/BlazorWebView` |
| Microsoft.Maui.Controls.Maps.Gtk | `src/Gtk/src/Maps` |
| Unit tests / sample | `src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests`, `src/Gtk/samples/Maui.Gtk.Sample` |

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
