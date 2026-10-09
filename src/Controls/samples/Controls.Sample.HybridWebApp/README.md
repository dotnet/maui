# Hybrid web app

This sample is being implemented to demonstrate Vite live edits in a .NET 11
Mac Catalyst app using `HybridWebView`.

The Debug development path retains the native app's virtual app origin while
loading assets from a local Vite server. Vite owns frontend updates; editing
HTML, CSS, or TypeScript does not require rebuilding the native app.
Without development configuration, the host loads its packaged fallback page.
Release builds contain neither development forwarding nor the inspector.

## Layout

```text
Controls.Sample.HybridWebApp/
  README.md
  run-demo.sh
  Native/
    Maui.Controls.Sample.HybridWebApp.csproj
    Development/                 Debug-only provider and inspector
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

Select a compatible installed Xcode **per process**, without changing global
`xcode-select`. For the pinned Apple 26.5 packs, validation succeeded with
`/Applications/Xcode-26.6.0.app`; the globally selected Xcode 27.0 failed while
building the framework's native interop project. The native build/run examples
below show the validated `DEVELOPER_DIR` override. Adjust that path to your
compatible installation rather than switching the machine-wide selection.

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
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
bash eng/common/dotnet.sh build src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false \
  --disable-build-servers
```

Launch the packaged app without Node or a Vite server:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
bash eng/common/dotnet.sh run --project src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false \
  --no-build --no-restore
```

The page should say **HybridWebView is loading bundled content** under
`app://0.0.0.1/`. Native startup writes one `HYBRIDWEBAPP_START` log line containing
the actual process ID, a unique startup identity, and the UTC time. No JavaScript
invocation is required to produce this marker.

In the validated setup, `dotnet run` returns after launching the native process;
its exit does not mean the app window has closed. Close the app when finished.
The root launcher tracks authenticated native process/startup identity instead
of treating the SDK runner's lifetime as the app's.

The project inherits the common sample `Directory.Build.props`/targets. By default
it references the framework source in this checkout. It also follows the sibling
samples' `UseWorkload=true`/`MauiVersion` convention for explicitly selected
workload-package builds; that path is not the in-tree validation path. Development
forwarding refuses workload-package mode, which may lack the new internal
stopped-task guard. Packaged content does not require that guard.

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
uses `ws://127.0.0.1:5173` rather than inferring the native page's
`app://0.0.0.1` hostname.

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

## Launch the demo

After the one-time SDK/workload/build-task setup and `npm ci` above, run the
sample-root launcher from any working directory:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
./src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.sh
```

For an existing in-tree Debug build, skip rebuilding:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
./src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.sh --no-build
```

The path example above is relative to the repository root; an absolute launcher
path works from any directory. `DEVELOPER_DIR` is inherited per process and is
never changed globally. `HYBRIDWEBAPP_INSPECT_PORT` can select an unused inspection
port; otherwise the launcher selects one. Vite always uses port 5173.

The launcher uses Bash and Python 3's standard library, plus the existing Node,
npm, SDK wrapper, and macOS `ps`/`lsof` tools. It does not install frontend
dependencies on each launch. Its preflight rejects missing dependencies,
unsupported Node/architecture, unavailable ports, mismatched repository SDK,
incompatible selected Xcode, missing local Apple packs/build tasks, and an
already-running sample. An occupied port never triggers termination of another
process.

The existing npm dev command starts in the sibling Web project. The launcher
checks the expected Vite page and its owned listener before invoking the pinned
repository SDK with the explicit Native project, Debug configuration, Mac
Catalyst TFM, in-tree/platform selectors, and supported `dotnet run -e` variables.
No `eval`, JavaScript watcher, C# watch mode, or new SDK target is used.

When ready, it prints the actual native PID/startup ID, inspection address, and
an owner-only session directory under this sample's ignored `artifacts/`.
The generated token is never printed; it is held in that directory's protected
`token` file while the session runs. SDK output is redacted before being printed
or saved. `state.json` records the authenticated native identity and inspection
port. Use the token to make read-only probes while the launcher is running:

```sh
# Replace this path with the session directory printed by the launcher.
session=/absolute/path/to/Controls.Sample.HybridWebApp/artifacts/session-...
token=$(cat "$session/token")
port=$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["inspectionPort"])' "$session/state.json")
curl --fail -H "Authorization: Bearer $token" "http://127.0.0.1:$port/probe"
```

The launcher keeps Vite alive after the SDK runner returns. Normal native process
exit ends the session; Ctrl+C exits with 130. Build/startup/server failures exit
nonzero. Cleanup targets only owned process groups and the verified launched
native identity, with bounded TERM/KILL waits, then deletes the ephemeral token.
Session logs and non-secret state remain for diagnosis.

## Manual Debug development session

For lower-level diagnosis, start Vite with the command above in its own
terminal and build the Debug native project with the documented Xcode override.
Choose an unused inspection port and generate a token; keep that token out of
source control and use the same token in the native and probe terminals:

```sh
export HYBRIDWEBAPP_DEV_URL=http://127.0.0.1:5173/
export HYBRIDWEBAPP_INSPECT_PORT=5917
export HYBRIDWEBAPP_INSPECT_TOKEN=$(python3 -c 'import secrets; print(secrets.token_urlsafe(32))')
```

Only the literal HTTP loopback Vite endpoint on port 5173 is accepted. For the
current arm64 Debug layout, launch the built executable directly so it inherits
the configuration and its native logs remain visible:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
artifacts/bin/Maui.Controls.Sample.HybridWebApp/Debug/net11.0-maccatalyst/maccatalyst-arm64/Maui.Controls.Sample.HybridWebApp.app/Contents/MacOS/Maui.Controls.Sample.HybridWebApp
```

This manual validation command bypasses the SDK-based root launcher.
For packaged-content inspection, omit `HYBRIDWEBAPP_DEV_URL` but retain the
inspection token and port. With all three variables absent, no inspector starts.

Probe the actual embedded view, not a separate browser:

```sh
curl --fail -H "Authorization: Bearer $HYBRIDWEBAPP_INSPECT_TOKEN" \
  "http://127.0.0.1:$HYBRIDWEBAPP_INSPECT_PORT/state"
curl --fail -H "Authorization: Bearer $HYBRIDWEBAPP_INSPECT_TOKEN" \
  "http://127.0.0.1:$HYBRIDWEBAPP_INSPECT_PORT/source"
curl --fail -H "Authorization: Bearer $HYBRIDWEBAPP_INSPECT_TOKEN" \
  "http://127.0.0.1:$HYBRIDWEBAPP_INSPECT_PORT/probe"
curl --fail -H "Authorization: Bearer $HYBRIDWEBAPP_INSPECT_TOKEN" \
  "http://127.0.0.1:$HYBRIDWEBAPP_INSPECT_PORT/snapshot" --output artifacts/hybridwebapp-snapshot.png
```

- `/state`: native PID/startup identity, native URI, mode, active fetches, and
  bounded recent upstream result metadata (not WebKit delivery acknowledgments).
- `/source`: current `document.documentElement.outerHTML`, served as plain text
  with a no-script content security policy rather than an executable web page.
- `/probe`: fixed text, computed style, resource URLs, and location/ready-state
  values obtained with `HybridWebView.EvaluateJavaScriptAsync` on the UI thread.
- `/snapshot`: PNG from the embedded `WKWebView`'s native snapshot API.

The inspector binds only to `127.0.0.1`, requires the bearer token and literal
loopback Host header, accepts only body-free GET requests, and has no arbitrary
JavaScript execution endpoint. Requests and UI waits are bounded to ten/eight
seconds, headers to 8 KiB, and responses to 2 MiB. Only Debug has the network
client/server entitlements; the app sandbox remains enabled.

The development provider forwards only ordinary app-origin GET/HEAD resources
to the fixed upstream, preserving escaped paths, queries, status, and MIME type.
It buffers at most 8 MiB per asset, recomputes body headers after decompression,
disables caching/redirects/cookies, and returns explicit errors instead of
substituting packaged HTML. Every upstream 3xx is explicitly rejected with 502;
neither relative nor off-origin redirects are followed. HEAD responses have no
body on success or error, while retaining representation Content-Length.
These routes remain framework-owned:

```text
/_framework/hybridwebview.js
/__hwvInvokeDotNet
/__hwvSendMessage
```

The registered scheme handler is not replaced. An internal Core lifetime guard,
carried into Controls' response helpers, drops callbacks after WebKit stops a
task or the handler disconnects. It does not cancel application-owned HTTP work:
the sample cancels the prior document's fetches before main-frame navigation and all
fetches on window destruction, with a ten-second bound for individual fetches.
Its sample-owned navigation observer is installed only when no existing delegate
is present; it never replaces the registered app URL scheme handler.
Close the native app and stop Vite when finished.

## Initial scope

- Mac Catalyst and .NET 11.
- A temporary root launcher using the existing frontend command and `dotnet run`.
- Safe development-resource forwarding and explicit error handling.
- Sample-local Debug helpers for inspecting the actual WebView DOM and snapshots,
  without a DevFlow dependency.

Commands, generated JS modules, new interop APIs, native C# Hot Reload, and SDK
integration are not part of this first demo.

## Status

The packaged host, independent frontend, Debug development resource provider,
internal stopped-task guard, and authenticated Debug-only WebView inspector are
implemented. The root `run-demo.sh` adds sample-local SDK-based launch and process
supervision; launcher validation is recorded separately below.

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
- The initial native build without a `DEVELOPER_DIR` override was
  **blocked before sample compilation**: Xcode 27 rejects the existing
  `src/Core/AppleNative/PlatformInterop/MauiPlatformInterop.xcodeproj` macOS
  deployment target 10.15; that Xcode supports targets 12.0 through 27.0.
  `xcodebuild` exited with code 65. This sample does not alter that framework
  project or switch global Xcode selection.

### Native-toolchain unblock validation

On the same date, the native build command above **passed with zero warnings and
errors** using per-process Xcode 26.6.0. The SDK and Apple packs were unchanged.
The shared deployment targets were not edited, and global `xcode-select` still
reported `/Applications/Xcode.app/Contents/Developer` (Xcode 27.0).

The packaged launch command above **returned successfully** and started the
sample's actual native process with Vite stopped. A separate direct invocation
of the built bundle executable captured the `HYBRIDWEBAPP_START` marker with the
actual PID, unique startup identity, UTC time, and `content=packaged`. The process
remained alive, finished native application startup, and owned an on-screen
window. The bundle's `Contents/Resources/wwwroot/index.html` matched the packaged
source byte-for-byte.

For the validated arm64 Debug layout, the direct console-capture invocation was:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
artifacts/bin/Maui.Controls.Sample.HybridWebApp/Debug/net11.0-maccatalyst/maccatalyst-arm64/Maui.Controls.Sample.HybridWebApp.app/Contents/MacOS/Maui.Controls.Sample.HybridWebApp \
  > artifacts/hybridwebapp-packaged-native.log 2>&1
```

Validation logs/binlog were retained under the ignored repository `artifacts/`:
`hybridwebapp-native-xcode26.log`, `hybridwebapp-native-xcode26.binlog`,
`hybridwebapp-packaged-launch.log`, `hybridwebapp-packaged-native.log`, and
`hybridwebapp-packaged-window.log`.

This verifies the build and packaged native launch, not the live DOM or WebView
pixels. The read-only host accessibility check was unavailable because permission
was not already granted; no permission prompt or global setting change was
requested. At that milestone, embedded content/origin inspection was still
pending. The Debug inspector now supplies that evidence without accessibility
permissions. Both native test instances from this validation were stopped.

### Development provider and inspector validation

The same pinned SDK/Apple packs and per-process Xcode 26.6 were used. Exact native
Debug build/run commands are above; Release was built with the same native build
command plus `-c Release`. Both configurations passed with zero warnings/errors.
The targeted framework regression command was:

```sh
bash eng/common/dotnet.sh test src/Core/tests/UnitTests/Core.UnitTests.csproj \
  --filter FullyQualifiedName~WebViewRequestLifetimeTests \
  -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false \
  --disable-build-servers
```

All **six** lifetime tests passed. The initial scoped whole-solution formatter
passed; a repeated invocation stalled in its build host and was stopped after
five minutes. Restricted C# whitespace formatting and all final builds passed.

Runtime validation used the authenticated commands above against the actual
embedded view:

- Native URI and JavaScript `location.href` both remained `app://0.0.0.1/`.
  HTML, TypeScript, the normal module import, and the dynamic import rendered;
  proxy logs/result metadata showed Vite supplying their transformed resources.
  Source and native WKWebView PNG captures were inspected.
- One CSS-only edit applied via HMR without a document reload. Seven subsequent
  rounds each changed HTML text, TypeScript-rendered text, and CSS color:
  **22 source changes** were confirmed with in-app probes, with one unchanged
  native PID/startup ID and no native rebuild, reinstall, or restart during those
  edits. Ordinary HTML/TypeScript edits used Vite's document-reload fallback.
- Temporary local Vite fixtures verified actual in-app 503/custom MIME/escaped
  query preservation, a 404 response without packaged fallback, and body-free HEAD
  with correct length. All three reserved routes bypassed Vite (zero upstream
  reserved-route hits); framework JavaScript returned 200, the rejected native
  invocation request returned 400, and the native message path returned 404.
- A fetch aborted in the embedded page completed upstream eight seconds later
  without invalid stopped-task callbacks or native restart. A Vite-triggered
  document reload canceled a genuinely pending upstream fetch; upstream abort
  metrics and native cancellation logs confirmed it, and active fetch count
  returned to zero.
- Stopping the fixture Vite listener produced an actual embedded HTTP 502 with an
  explicit error body, not a successful packaged page. The inspector remained
  available and native identity was unchanged.
- The final inspector rejected absent/incorrect authentication with 401,
  an incorrect Host with 400, and an `/eval` request with 404. Source responses
  were inert plain text with a no-script CSP; final native snapshots were PNGs.
- With Vite stopped and no upstream setting, packaged mode rendered the expected
  fallback heading under the native app origin, with zero forwarded requests and
  a native WKWebView snapshot. Release compilation excluded all development
  sources/entitlements; a Release launch ignored the development environment,
  stayed packaged, contained no development service types, and opened no
  inspection listener.

The temporary fixture HTML/CSS/TypeScript edits were restored byte-for-byte;
frontend `type-check` and `build` passed afterward. The fixture middleware is not
part of the sample. Inspection does not expose arbitrary JavaScript evaluation,
and development mode adds no JS-to-.NET command or invocation target.

Evidence is retained only in ignored `artifacts/`, including
`hybridwebapp-lifetime-tests.log`, `hybridwebapp-proxy-debug.*`,
`hybridwebapp-proxy-release.*`, `hybridwebapp-proxy-state.json`,
`hybridwebapp-proxy-probe.json`, `hybridwebapp-proxy-source.html`,
`hybridwebapp-proxy-snapshot.png`, `hybridwebapp-live-edits.jsonl`,
`hybridwebapp-fixture-results.json`, `hybridwebapp-navigation-cancellation.json`,
and `hybridwebapp-upstream-down.json`. Root launcher validation is recorded below.
Every owned
native instance and frontend process was stopped after validation; the ephemeral
inspection token was removed rather than committed.

### Launcher validation

Native was rebuilt after the redirect/HEAD correction `d446c92d8e` using the
documented per-process Xcode 26.6 build command: **passed, zero warnings/errors**.
Launcher checks used that corrected app:

- `bash -n run-demo.sh`, `shellcheck run-demo.sh`, and parsing the embedded Python
  with `ast.parse`: **passed**.
- The documented `run-demo.sh --no-build` invocation from `src/Core/`, using an
  absolute launcher path: **passed**. Supported SDK `-e` settings reached the
  actual app; authenticated DOM, CSS, source, and native PNG probes passed.
  HTML/TypeScript/CSS edits reached the already-running native instance with an
  unchanged PID/startup ID after the SDK runner returned.
- Normal app quit through `NSRunningApplication.terminate()`: **launcher exit 0**;
  Vite and inspection listeners closed and the token was deleted. This tested app
  quit, not merely closing a window while leaving its native process alive.
- The default build/run invocation followed by SIGINT to the recorded
  `launcherPid`: **exit 130**, with bounded owned-process cleanup and token removal.
- An unrelated HTTP listener on Vite port 5173: **nonzero preflight exit**. The
  listener was neither killed nor adopted and continued serving its fixture page.
- Selected Xcode 27 without `DEVELOPER_DIR`: **nonzero preflight exit**, with
  actionable compatibility guidance and no global selection change.
- Injected native build failure:

  ```sh
  DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
  MSBuildSDKsPath="$PWD/artifacts/nonexistent-sdk" \
  ./src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.sh
  ```

  **Nonzero exit** after the SDK's `MSB4236` failure; the owned Vite process stopped
  and no native app or token remained.
- Terminating the recorded owned Vite process group after native readiness:
  **exit 1**, with cleanup of the verified native instance and token. A fixture
  which closed only Vite's HTTP listener while its process stayed alive also
  **exited 1** after the bounded repeated page-readiness checks failed.

Logs for these checks are in ignored repository `artifacts/hybridwebapp-launcher-*`
and the launcher's owner-only sample-local session directories. The frontend edits
were restored to the original files; no launcher starts npm installation or changes
global Xcode selection.

### Redirect/HEAD correction runtime fixtures

Narrow temporary Vite middleware and frontend `fetch` calls exercised the rebuilt
`d446c92d8e` app through the same SDK launcher. The middleware/front-end edits
were removed after validation. Actual results were read from the embedded DOM
using the authenticated `/source` endpoint:

```sh
DEVELOPER_DIR=/Applications/Xcode-26.6.0.app/Contents/Developer \
./src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.sh --no-build
# In a probe terminal, using that launcher's session/token/port as above:
curl --fail -H "Authorization: Bearer $token" \
  "http://127.0.0.1:$port/source"
```

- Relative 302 to a fixture destination: **502 redirect-not-supported error**;
  destination request count stayed zero.
- Off-origin 307 to a separately monitored loopback listener:
  **502 redirect-not-supported error**, with **zero off-origin requests**.
- HEAD against a fixture delaying response headers beyond the ten-second bound:
  **504, empty body, nonzero representation Content-Length**.
- HEAD after the owned Vite listener closed:
  **502, empty body, nonzero representation Content-Length**. The native DOM
  collector captured this before the launcher's bounded server-failure cleanup.

Evidence: ignored `artifacts/hybridwebapp-redirect-head-results.json`,
`hybridwebapp-head-down-result.json`, `hybridwebapp-off-origin-hits.log`, and
`hybridwebapp-launcher-redirect-head.log`. All owned native/frontend/off-origin
fixture processes were stopped, all session tokens removed, and frontend
`type-check`/`build` passed again with the original sources restored.
