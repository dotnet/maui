# Hybrid web app

This sample is being implemented to demonstrate Vite live edits in a .NET 11
Mac Catalyst app using `HybridWebView`.

The intended development path retains the native app's virtual app origin while
loading assets from a local Vite server. Vite will own frontend updates; editing
HTML, CSS, or TypeScript should not rebuild or restart the native app.
**Development forwarding is not implemented yet.** The current native app loads
only its packaged fallback page.

## Layout

```text
Controls.Sample.HybridWebApp/
  README.md
  Native/
    Maui.Controls.Sample.HybridWebApp.csproj
    Platforms/MacCatalyst/
    Resources/Raw/wwwroot/index.html
  Web/
    package.json
    package-lock.json
    vite.config.ts
    index.html
    src/
```

The native and frontend project roots are separate so Node dependencies are not
implicitly discovered or packaged by the .NET project.

## Native packaged-content smoke check

Run commands below from the repository root on a Mac. Use the .NET 11 SDK pinned
in the repository's `global.json`, a compatible Mac Catalyst workload, and the
Xcode version required by that workload. The repository wrapper acquires the
pinned SDK locally; it does **not** install Apple workloads. Do not substitute a
different MAUI package version to work around a missing workload.

If that local SDK does not have Mac Catalyst packs, acquire the repository-pinned
workload manifests without replacing the SDK, then install only Mac Catalyst into
the local SDK:

```sh
bash eng/common/dotnet.sh --version
bash eng/common/dotnet.sh build src/DotNet/DotNet.csproj \
  -p:InstallDotNet=false -p:InstallWorkloadPacks=false
bash eng/common/dotnet.sh workload install maccatalyst \
  --skip-manifest-update --configfile NuGet.Config
```

Build the repository tasks first, then the native app. The task-build selectors
avoid requiring unrelated Apple or Android workloads just for this sample:

```sh
bash eng/common/dotnet.sh build Microsoft.Maui.BuildTasks.slnf \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false
bash eng/common/dotnet.sh build src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false
```

Launch the packaged app without Node or a Vite server:

```sh
bash eng/common/dotnet.sh run --project src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false
```

The page should say **HybridWebView is loading bundled content** under
`app://0.0.0.1/`. Native startup writes one `HYBRIDWEBAPP_START` log line containing
the actual process ID, a unique startup identity, and the UTC time. No JavaScript
invocation is required to produce this marker.

The project inherits the common sample `Directory.Build.props`/targets. By default
it references the framework source in this checkout. It also follows the sibling
samples' `UseWorkload=true`/`MauiVersion` convention for explicitly selected
workload-package builds; that path is not the in-tree validation path.

This first host targets only Mac Catalyst. It is intentionally not added to
`Microsoft.Maui.sln` or the general samples solution filter: restoring/building
those surfaces must not require Mac Catalyst on Android/Windows/Linux hosts.
Build this sample directly. Ordinary .NET builds never install frontend
dependencies, start Vite, or include the sibling `Web/` tree as native assets.

## Standalone frontend

Install Node.js 20.19+ within 20.x, or 22.12+ (including newer majors), with npm.
Perform dependency setup once, from the repository root:

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web ci
```

Run the normal Vite development command:

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run dev
```

Open <http://127.0.0.1:5173/> in a browser for the standalone frontend. Vite binds
only to loopback and fails if port 5173 is occupied. The HMR WebSocket explicitly
uses `ws://127.0.0.1:5173` rather than inferring the future native page's
`app://0.0.0.1` hostname. This configuration prepares the native development path;
it does not yet establish or validate a WebView-to-Vite connection.

Edit targets for later in-app validation:

- `Web/index.html`: `#html-message`, static HTML text.
- `Web/src/main.ts` and `message.ts`: `#typescript-message`, rendered TypeScript
  content and a regular module import.
- `Web/src/style.css`: `#style-target`, background, border, and spacing.
- `Web/src/details.ts`: `#module-message`, content supplied by a dynamic import.

Type-check and build the frontend independently:

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run type-check
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run build
```

`Web/dist/` is frontend build output, not automatically packaged native content.
`node_modules/`, `dist/`, logs, and artifacts are ignored by source control.
Stop Vite with Ctrl+C when finished.

## Initial scope

- Mac Catalyst and .NET 11.
- A temporary root launcher using the existing frontend command and `dotnet run`.
- Safe development-resource forwarding and explicit error handling.
- Sample-local Debug helpers for inspecting the actual WebView DOM and snapshots,
  without a DevFlow dependency.

Commands, generated JS modules, new interop APIs, native C# Hot Reload, and SDK
integration are not part of this first demo.

## Status

Milestone 1 adds the native packaged host and independent vanilla-TypeScript
frontend. The development resource provider, cancellation/liveness adaptation,
Debug-only WebView inspector, root `run-demo.sh`, and in-app live-edit verification
are still pending. There is no live-edit demo or automatic native/frontend process
orchestration yet.

### Milestone 1 validation

Validation on 2026-10-09 used Node 24.4.1/npm 11.5.2, the repository-pinned
.NET SDK `11.0.100-rtm.26480.113`, Mac Catalyst packs
`26.5.12253-net11-rc.2` from the repository workload manifests, and the existing
Xcode 27.0 selection. No global SDK/workload or Xcode settings were changed.

- The task-build command above passed with zero warnings/errors.
- The `type-check` and frontend `build` commands above passed with Vite 8.3.4
  and TypeScript 7.0.2. The production output includes a separate dynamic chunk.
- Vite served the HTML, transformed TypeScript, imported CSS, and dynamic source
  module. Its transformed HMR client uses the explicit loopback endpoint. A
  second `run dev` invocation correctly failed on occupied port 5173.
- MSBuild evaluation confirmed common sample imports, valid in-tree reference
  paths, the `wwwroot/index.html` asset logical name, and no `Web/` compile items.
- The native build command above, additionally using `--disable-build-servers`,
  was **blocked before sample compilation**: Xcode 27 rejects the existing
  `src/Core/AppleNative/PlatformInterop/MauiPlatformInterop.xcodeproj` macOS
  deployment target 10.15; that Xcode supports targets 12.0 through 27.0.
  `xcodebuild` exited with code 65. This sample does not alter that framework
  project or switch global Xcode selection.

The native launch, startup log marker, and actual packaged WebView rendering
remain unverified until the native-toolchain blocker is resolved. Standalone
frontend checks do not prove in-app loading or live edits. No development server
was left running.
