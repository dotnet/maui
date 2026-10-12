# MAUI + Tauri playground implementation backlog

**Purpose:** A feature-first playground, not a themed app or a production product. Each experiment gets a name, a short explanation, editable inputs, an observable result and an API/documentation link. This document is the implementation backlog; priorities and completion tracking do not appear in the playground UI.

**Scope:** .NET 11 MAUI Controls, Essentials and tooling; Tauri v2 core APIs and official plugins. Shared capabilities appear once. This is a feature-family inventory, not a list of every property, method or third-party package.

| Group | Meaning | Complete |
|---|---|---|
| **P0 - Critical for v1** | 19 items: make a useful playground that proves the real native bridge. | 16 |
| **P1 - Must have** | 29 items: cover the normal day-to-day native app toolkit. | 0 |
| **P2 - Need to have** | 47 items: add broad UI, device and desktop integration coverage. | 0 |
| **P3 - Good to have** | 26 items: add specialised integrations, extension mechanisms and distribution scenarios. | 1 |

**Completion rule:** A checked item means the complete experiment described in that row is implemented end-to-end and verified within the documented scope below. It does not mean every member of the corresponding .NET API is wrapped, or that every OS/release configuration is certified. Partially implemented acceptance targets stay unchecked. For example, the completed clipboard experiment covers text read/write, not clipboard change notifications; preferences covers bounded text settings, not every `IPreferences` value type.

**API notes:** A MAUI documentation link does not imply a built-in JavaScript wrapper. The standalone experimental `@maui/api` package currently exposes a deliberately small Essentials subset. Tauri plugins also require native registration and permissions. Entries marked **adapter/package** need additional .NET or platform code; they are not first-party MAUI API equivalents.

**Implementation home:** [This sample][sample-readme] hosts the native app and frontend. The [standalone JavaScript SDK][sdk-readme] is one npm package with isolated infrastructure/feature modules. Native command routing and provider contracts remain independent of Controls; the app owns provider composition and the MAUI WebView attachment. Alternative native hosts and future Graphics/Controls wrappers are not implemented. A Labs migration and DevFlow integration remain deferred.

## Completion evidence and remaining acceptance

The initial native milestone verified app identity, text clipboard/preferences, asynchronous DTOs, errors, events, cancellation and frontend reload state on Mac Catalyst, iOS 27 simulator and Android emulator. Android also verified packaged execution and preferences across native restart. After extraction, Android repeated native acceptance using the SDK dependency, tested lazy feature helpers and reconnect, and repeated acceptance offline. Its four APK frontend assets matched staging byte-for-byte. The sample README records exact commands, scopes and historical versus current evidence.

The reusable SDK ships built JavaScript and declarations, not app-local TypeScript. Its packed-consumer checks use a real tarball without the source tree. Source comments document public APIs. These are local preview artifacts, not evidence of npm publication or registry-scope ownership.

**Platform/release gaps:** Windows runtime, physical devices, installed Release execution, NativeAOT and signed installation are unverified. The updated iOS app connected, but the new simulator touch/bounce/keyboard checks were not completed. Local success does not establish CI or merge readiness.

| Unchecked item with partial implementation | What exists | What remains |
|---|---|---|
| Custom command registration | Explicit typed providers and generated .NET invocation/JSON metadata | Opt-in attributes or generated provider registration |
| Explicit exposure and permissions | Unknown/disabled command rejection, bounded inputs, distinct native error codes | An actual OS permission check/request/denial experiment |
| Platform-aware results | Unsupported/permission/configuration error classification | Observable native scenarios for each classification, not just policy failures/test doubles |
| Plugin/provider extension | Host-owned composition and an extensible command/topic registry | A complete optional provider experiment with its JS integration |
| Installable artifacts; signing and installed execution | Debug native deployment and Mac Catalyst Release build/asset checks | Install and exercise the intended signed Release artifacts |
| Diagnostics and performance | Startup identity, request metrics and bounded operation log | Reproducible command-latency, startup and memory scenarios |
| Browser mocks and native automation | Committed contract tests and actual native acceptance evidence | A committed, repeatable native automation entrypoint |

## P0 - Critical for v1

### Playground and native bridge

- [x] **Feature browser** - Search implemented experiments, navigate categories and open API docs. [MAUI controls][m-controls] / [Tauri example][t-example].
- [x] **Bridge bootstrap and readiness** - Load the bridge and clearly show native-host versus browser-only mode. [HybridWebView][m-hybrid] / [Tauri core][t-core].
- [x] **One native target hub** - Route feature modules and application commands through one explicit, instance-backed entry point. [MAUI invocation][m-invoke] / [Tauri commands][t-commands].
- [ ] **Custom command registration** - Add an app service without editing a growing forwarding switch; prototype opt-in attributes or generated registration. [MAUI generated invocation][m-net11] / [Tauri registration][t-commands].
- [x] **Arguments and return values** - Round-trip primitives, nulls, arrays, DTOs and asynchronous results. [MAUI invocation][m-invoke] / [Tauri commands][t-commands].
- [x] **Typed JavaScript modules** - Import named helpers with parameter/result types rather than hand-writing wire messages. [MAUI foundation][m-net11] / [Tauri API][t-api]. Generated app-specific TypeScript is additional tooling, not claimed by this checkbox.
- [x] **Errors and operation log** - Display rejected calls, native errors and transport failures with useful details. [MAUI invocation][m-invoke] / [Tauri logging][t-log].
- [ ] **Explicit exposure and permissions** - Reject unknown/disabled commands and distinguish app policy from OS permissions using an actual permission scenario. [MAUI permissions][m-permissions] / [Tauri capabilities][t-capabilities].
- [x] **Threading and service lifetime** - Run UI APIs on the UI thread and preserve the registered service instance across frontend reloads. [MAUI MainThread][m-mainthread] / [Tauri state][t-state].
- [x] **Reload and disconnect behaviour** - Reconnect listeners without duplicates; terminate obsolete calls and release resources. [MAUI bridge][m-hybrid] / [Tauri events][t-events].
- [x] **Events and subscriptions** - Receive native changes and explicitly unsubscribe. [MAUI messaging][m-hybrid] / [Tauri events][t-events].
- [x] **Progress and cancellation** - Show a long-running native operation, stream progress and cancel it. [MAUI messaging foundation][m-hybrid] / [Tauri channels][t-channels]. The experiment uses the explicit command protocol; it does not claim a shipped general-purpose MAUI channel API.

### A runnable first version

- [x] **App and device identity** - Show app version, platform, OS, architecture and native startup identity. [AppInfo][m-appinfo], [DeviceInfo][m-deviceinfo] / [Tauri app][t-app], [OS][t-os]. Process/startup diagnostics are app-owned, not Essentials APIs.
- [x] **Text clipboard** - Copy and explicitly paste sample text through the native clipboard. [MAUI][m-clipboard] / [Tauri][t-clipboard].
- [x] **Preferences** - Save, read/remove and reload a small namespaced text setting across app restarts. [MAUI][m-preferences] / [Tauri store][t-store].
- [x] **Vite live development** - Edit HTML/CSS/JS inside the running app on selected desktop/mobile targets. [Current sample][sample-readme] / [Tauri development][t-develop].
- [x] **Packaged offline content** - Run the same feature UI and bridge without a development server. [MAUI assets][m-assets] / [Tauri frontend configuration][t-config].
- [x] **Native inspection and repeatable checks** - Inspect actual in-app DOM/results; support frontend mocks without confusing them with native verification. [MAUI developer tools][m-devtools] / [Tauri mocks][t-mocks].
- [ ] **Platform-aware results** - Show actual unsupported, permission-denied and configuration-required scenarios rather than fake success. [MAUI platform features][m-platform] / [Tauri plugin support][t-plugins].

## P1 - Must have

### Files, storage and networking

- [ ] **File picker** - Select one or several files using the OS picker. [MAUI][m-filepicker] / [Tauri dialogs][t-dialog].
- [ ] **File access** - Read/write/delete sample files in app-owned storage with explicit size and scope limits. [MAUI file helpers][m-files] / [Tauri filesystem][t-fs].
- [ ] **Save/export destination** - Export a result to a selected destination. **MAUI adapter/package** beyond its read-oriented FilePicker. [MAUI sharing][m-share] / [Tauri save dialog][t-dialog].
- [ ] **Paths and packaged resources** - Resolve app data/cache/resource locations without assuming mobile files have desktop paths. [MAUI][m-files] / [Tauri paths][t-path].
- [ ] **Structured local store** - Persist a small JSON document, distinct from primitive preferences. [.NET JSON][net-json] / [Tauri store][t-store].
- [ ] **Secure key/value storage** - Store and remove a non-sensitive test value; never use real secrets in the experiment. [MAUI][m-secure].
- [ ] **HTTP requests** - Send a native request and show status, headers, body and duration. [.NET HttpClient][net-http] / [Tauri HTTP][t-http].
- [ ] **Uploads and downloads** - Transfer a bounded sample file with progress and cancellation. [.NET HttpClient][net-http] / [Tauri upload][t-upload].
- [ ] **WebSocket** - Connect, send/receive messages and disconnect cleanly. [.NET ClientWebSocket][net-websocket] / [Tauri WebSocket][t-websocket].
- [ ] **Connectivity** - Report network access and subscribe to connectivity changes. [MAUI][m-connectivity]. Do not infer internet reachability from connection type alone.

### OS interactions

- [ ] **Native dialogs** - Display alerts, confirmations, prompts and action sheets. [MAUI][m-dialogs] / [Tauri][t-dialog].
- [ ] **Open external links** - Open an allowed URL in the system browser or another app. [MAUI Launcher][m-launcher] / [Tauri opener][t-opener].
- [ ] **In-app browser** - Open documentation in the platform browser presentation, separate from the embedded app WebView. [MAUI Browser][m-browser].
- [ ] **Share text and files** - Invoke the native share experience. [MAUI][m-share]. Tauri may need a mobile/platform adapter.
- [ ] **Window basics** - Read/set title and supported size/position; activate or close the owning window. [MAUI][m-window] / [Tauri][t-window].
- [ ] **Multiple windows** - Create a second experiment window and verify state and message isolation. [MAUI][m-window] / [Tauri][t-window].
- [ ] **Application lifecycle** - Observe creation, activation, deactivation, backgrounding and destruction. [MAUI][m-lifecycle] / [Tauri][t-app].

### Native and web UI together

- [ ] **Mixed native/web surface** - Open a real MAUI panel next to the web playground and exchange state. [MAUI embedding][m-embedding], [HybridWebView][m-hybrid]. Tauri's web UI is not a native-controls equivalent.
- [ ] **Navigation** - Exercise native and web navigation, back behaviour and modal presentation. [MAUI Shell][m-shell], [pages][m-pages] / [Tauri webviews][t-webview].
- [ ] **Text and actions** - Showcase Label, Entry, Editor, Button and ImageButton with native focus/input. [MAUI controls][m-controls].
- [ ] **Choices and values** - Showcase Picker, CheckBox, RadioButton, Switch, Slider and Stepper. [MAUI controls][m-controls].
- [ ] **Date/time and busy states** - Showcase DatePicker, TimePicker, ActivityIndicator and ProgressBar. [MAUI controls][m-controls].
- [ ] **Lists and search** - Showcase CollectionView selection/templates and SearchBar. [MAUI CollectionView][m-collection] / browser-side lists in Tauri.
- [ ] **Responsive layouts** - Exercise Grid, AbsoluteLayout, horizontal/vertical stacks and FlexLayout, safe areas and mobile/desktop arrangements. [MAUI layouts][m-layouts], [safe areas][m-safe].
- [ ] **Theme and appearance** - Follow system light/dark mode and allow an explicit override. [MAUI][m-theme] / [Tauri window theme][t-window].
- [ ] **Accessibility** - Verify names, roles, focus, keyboard navigation, screen-reader announcements and large text. [MAUI semantics and SemanticScreenReader][m-accessibility] / native WebView plus accessible HTML.
- [ ] **Native menus, toolbars and context menus** - Add actions and keyboard accelerators, then verify callbacks. [MAUI menu bar][m-menu], [context menus][m-contextmenu], [keyboard accelerators][m-keyboard] / [Tauri menus][t-menu].
- [ ] **Images, fonts, icons and splash** - Load packaged/dynamic images and verify the app's shared asset pipeline. [MAUI images][m-images], [fonts][m-fonts], [icon][m-icon], [splash][m-splash] / [Tauri images][t-image].
- [ ] **Resources, styles and bindings** - Demonstrate resource dictionaries, XAML/CSS styles, compiled bindings, templates and visual states. [MAUI resources][m-resources], [styles][m-styles], [CSS][m-css], [bindings][m-bindings].

## P2 - Need to have

### Device and media APIs

- [ ] **Battery** - Read charge/power information and observe changes. [MAUI][m-battery].
- [ ] **Display and DPI** - Read size/density/orientation and handle display changes. [MAUI DeviceDisplay][m-display] / [Tauri DPI][t-dpi], [monitors][t-window].
- [ ] **Keep screen awake** - Enable and restore keep-screen-on behaviour. [MAUI DeviceDisplay][m-display].
- [ ] **Location** - Request current location and observe permitted updates. [MAUI][m-location] / [Tauri][t-geolocation].
- [ ] **Geocoding** - Convert a sample place to coordinates and back. [MAUI][m-geocoding].
- [ ] **Launch system maps** - Open a location in the installed maps application. [MAUI Map][m-maplaunch].
- [ ] **Embedded native map** - Show pins and regions; keep provider/key requirements explicit. [MAUI Maps][m-maps].
- [ ] **Photo/video picker and capture** - Select or capture media through native APIs. [MAUI][m-media].
- [ ] **Screenshot** - Capture the app and export the result. [MAUI][m-screenshot].
- [ ] **Text-to-speech** - Speak sample text and select a supported voice. [MAUI][m-speech].
- [ ] **Haptic feedback** - Trigger supported native interaction feedback. [MAUI][m-haptics] / [Tauri][t-haptics].
- [ ] **Vibration** - Start and stop a bounded vibration. [MAUI][m-vibration].
- [ ] **Flashlight** - Enable/disable the camera light where available. [MAUI][m-flashlight].
- [ ] **Accelerometer and shake** - Stream acceleration readings and detect a shake. [MAUI sensors][m-sensors].
- [ ] **Gyroscope** - Stream rotation-rate readings. [MAUI sensors][m-sensors].
- [ ] **Compass** - Stream heading readings. [MAUI sensors][m-sensors].
- [ ] **Magnetometer** - Stream magnetic-field readings. [MAUI sensors][m-sensors].
- [ ] **Barometer** - Stream pressure readings. [MAUI sensors][m-sensors].
- [ ] **Orientation sensor** - Stream orientation readings and clean up listeners. [MAUI sensors][m-sensors].
- [ ] **Conversions** - Demonstrate unit and colour conversion helpers with deterministic results. [MAUI unit conversions][m-units], [colour helpers][m-colors].

### Desktop and application integration

- [ ] **Drag and drop** - Drop text/files and drag an app-owned result out where supported. [MAUI][m-drag] / [Tauri window events][t-window].
- [ ] **Tray/status icon** - Add a desktop tray icon, menu and click callback. **MAUI platform adapter.** [Official MAUI sample][m-weather] / [Tauri tray][t-tray].
- [ ] **Global shortcuts** - Register/release a desktop-wide keyboard shortcut. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-shortcut].
- [ ] **Deep links and file associations** - Route an incoming URI or associated file to the correct experiment. [MAUI Apple links][m-applinks], [Android links][m-androidapplinks] / [Tauri][t-deeplink].
- [ ] **Single instance** - Hand a second launch's arguments to the existing desktop app. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-single].
- [ ] **Notifications** - Request permission, show a local notification and handle activation. **Scheduling needs a MAUI adapter/package.** [MAUI guidance][m-notifications] / [Tauri][t-notification].
- [ ] **App actions** - Launch an experiment from an app-icon shortcut. [MAUI][m-actions].
- [ ] **Version tracking** - Report first launch and version/build changes. [MAUI][m-version].
- [ ] **Persisted window state** - Restore a valid desktop size/position. **MAUI composition over Window + storage.** [MAUI Window][m-window] / [Tauri][t-windowstate].
- [ ] **Advanced window behaviour** - Explore fullscreen, decorations/title bar, resizing, cursor, attention and supported window effects. [MAUI TitleBar][m-titlebar], [platform APIs][m-native] / [Tauri][t-window].
- [ ] **Window positioning** - Place a supported desktop window relative to a monitor or tray icon. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri positioner][t-positioner].
- [ ] **WebView controls** - Exercise initialization settings, navigation, multiple webviews and resource interception. [MAUI WebView][m-webview], [HybridWebView][m-hybrid] / [Tauri webview][t-webview], [WebviewWindow][t-webviewwindow].
- [ ] **Local SQL** - Query a tiny sample database and reset it. **Additional .NET SQLite package.** [MAUI tutorial][m-sql] / [Tauri SQL][t-sql].
- [ ] **Persisted file access scopes** - Retain explicitly granted file access across launches where supported. **Platform adapter; not ordinary Preferences.** [MAUI platform APIs][m-native] / [Tauri persisted scope][t-persistedscope].

### More native UI and communication

- [ ] **Graphics and drawing** - Draw an interactive native canvas with GraphicsView and compare web rendering. [MAUI][m-graphics].
- [ ] **Shapes, brushes and decoration** - Demonstrate Border, BoxView, shapes, gradients and shadows. [MAUI controls][m-controls], [shapes][m-shapes].
- [ ] **Gestures and animation** - Exercise tap, long press, pointer, pan, pinch, swipe and bounded animations. [MAUI gestures][m-gestures], [animations][m-animations].
- [ ] **Scrolling, refresh and swiping** - Showcase ScrollView, RefreshView and SwipeView. [MAUI controls][m-controls]. Independent HTML pane scrolling is not proof of these native controls.
- [ ] **Carousel and indicators** - Showcase CarouselView and IndicatorView. [MAUI controls][m-controls].
- [ ] **Page/layout variants** - Showcase ContentPage/ContentView, NavigationPage, TabbedPage, FlyoutPage and BindableLayout. [MAUI pages][m-pages], [layouts][m-layouts].
- [ ] **UI behaviours and triggers** - Attach reusable behaviour and react to native control/state changes. [MAUI behaviours][m-behaviors], [triggers][m-triggers].
- [ ] **Localization and RTL** - Switch language/direction and verify native/web text and layout. [MAUI][m-localization].
- [ ] **Contacts** - Pick a contact with explicit consent; do not collect an address book by default. [MAUI][m-contacts].
- [ ] **Email** - Launch a prefilled email composer without sending automatically. [MAUI][m-email].
- [ ] **SMS** - Launch a prefilled message composer. [MAUI][m-sms].
- [ ] **Phone dialer** - Open the dialer without placing a call automatically. [MAUI][m-phone].
- [ ] **Web camera/microphone** - Compare permitted WebView browser media access with native media APIs. [Tauri example][t-example] / [browser getUserMedia][web-media].

## P3 - Good to have

### Specialised native integrations

- [ ] **Browser authentication** - Exercise browser launch, callback and explicit cancellation. [MAUI WebAuthenticator][m-auth].
- [ ] **Passkeys** - Register/assert a sample credential against a configured relying party. [MAUI .NET 11][m-passkeys]. This is not the same operation as a generic biometric prompt.
- [ ] **Local biometrics** - Request a local Face ID/Touch ID/biometric confirmation. **MAUI platform adapter/package.** [Platform APIs][m-native] / [Tauri][t-biometric].
- [ ] **Barcode/QR scanning** - Scan a sample code and return its value. **MAUI scanner package/adapter.** [Platform APIs][m-native] / [Tauri][t-barcode].
- [ ] **NFC** - Read a supported sample tag with explicit permission. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-nfc].
- [ ] **Encrypted vault** - Exercise a disposable vault and key lifecycle. **Not equivalent to SecureStorage; extra implementation/package.** [Tauri Stronghold][t-stronghold].
- [ ] **Autostart** - Opt into and undo desktop launch-at-login. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-autostart].
- [ ] **Process lifecycle** - Demonstrate explicit app restart/exit on supported desktop targets. [.NET/platform adapter][net-process] / [Tauri process][t-process].
- [ ] **Shell and sidecar process** - Run one allowlisted desktop helper and stream its output; never accept arbitrary shell commands. [.NET Process][net-process] / [Tauri shell][t-shell].
- [ ] **Command-line arguments** - Parse a sample launch option and route it into the UI. [.NET command-line overview][net-cli] / [Tauri CLI plugin][t-cli].
- [ ] **Localhost asset server** - Compare an opt-in local HTTP origin with the virtual app origin; not required for Vite forwarding. **Additional server/adapter.** [Tauri localhost][t-localhost].
- [ ] **Native resource handles** - Create/use/dispose a bounded native resource and reject stale handles. [MAUI platform APIs][m-native] / [Tauri core Resource][t-core], [images][t-image].
- [ ] **Plugin/provider extension** - Add one optional command provider without changing the app UI or core router. [MAUI DI][m-di], [custom handlers][m-handlers] / [Tauri plugin authoring][t-plugin-authoring].
- [ ] **Platform-specific custom UI** - Demonstrate handler/effect customization and explicit platform differences. [MAUI handlers][m-handlers], [effects][m-effects], [platform-specifics][m-platformspecifics].
- [ ] **Blazor Hybrid companion** - Compare one equivalent Razor/native interop scenario without making Blazor a dependency of the JS playground. [MAUI Blazor Hybrid][m-blazor].
- [ ] **Legacy control compatibility** - Record older ListView/TableView/cell and Frame behaviour only when needed; do not build new samples around obsolete APIs. [MAUI controls][m-controls].
- [ ] **Age signals** - Optional reference-sample experiment for platform age-consent APIs, not a built-in Essentials API. [Official sample][m-age].
- [ ] **In-app purchases** - Optional store-sandbox experiment, not a built-in Essentials billing service. [Official sample][m-billing].

### Development and distribution coverage

- [ ] **Native Hot Reload** - Verify supported C#/XAML edits independently of Vite HMR. [MAUI XAML Hot Reload][m-hotreload] / [Tauri native watch/restart][t-develop]. These are different mechanisms.
- [x] **Build/stage frontend assets** - Produce and package fresh frontend output without stale files or per-target races. [MAUI assets][m-assets] / [Tauri build configuration][t-config].
- [ ] **Installable artifacts** - Build, install and run platform-appropriate application packages. [MAUI deployment][m-deployment] / [Tauri distribution][t-distribution].
- [ ] **Signing and installed execution** - Verify signatures, entitlements and offline native interop after installation. [MAUI deployment][m-deployment] / [Tauri distribution][t-distribution].
- [ ] **Updates** - Check/download/apply a signed update through the chosen desktop distribution channel. **No direct universal MAUI updater.** [Tauri updater][t-updater].
- [ ] **Trimming and NativeAOT** - Verify registered commands/DTOs in supported published configurations. [MAUI NativeAOT][m-aot] / [Tauri commands][t-commands]. Do not equate Rust compilation with .NET trimming.
- [ ] **Diagnostics and performance** - Capture command latency, startup, memory and reload behaviour with reproducible scenarios. [MAUI profiling][m-profiling] / [Tauri logging][t-log].
- [ ] **Browser mocks and native automation** - Run frontend contract checks separately from device/native tests through repeatable checked-in entrypoints. [MAUI UI testing][m-uitesting] / [Tauri mocks][t-mocks], [WebDriver][t-webdriver].

## Next implementation focus

Finish the unchecked P0 acceptance targets without replacing working explicit registration or inventing mock OS permission results. Then add small P1 experiments for files, HTTP, window operations and a real native control panel.

Keep one feature catalog and one command hub. The feature page and diagnostic experiment must call the same modules; do not create a second test-only implementation. Keep the playground small: no account system, backend administration, social features or themed story. A feature is a small experiment, not a complete application.

## API and platform gaps

- Tauri primarily supplies web UI plus native APIs. MAUI also supplies native controls, layouts, bindings and graphics; browser controls are not proof of those MAUI features.
- Tauri official plugins are inventoried, but not all are installed/enabled by default or supported on every platform.
- MAUI does not ship universal APIs for tray icons, global shortcuts, arbitrary shell execution, a save dialog, NFC, barcode scanning or a generic encrypted vault. Those rows identify adapters/packages rather than invented Essentials APIs.
- The MAUI target set here is Android, iOS, Mac Catalyst and Windows. Tauri Linux/native macOS support is a separate platform difference, not an unchecked API experiment. Tizen is excluded.
- Experiments needing real hardware, credentials, entitlements or an external service must document that requirement. Setup effort alone does not determine priority.
- Authentication, purchases, signed updates and installed execution cannot be completed using only a fake in-app response. Simulations must be explicitly labelled.
- This list groups low-level classes/control properties into user-visible feature families. It does not exhaustively catalogue third-party ecosystems.

## Inventory completeness

**Tauri core modules represented:** `core`, `app`, `event`, `window`, `webview`, `webviewWindow`, `path`, `dpi`, `image`, `menu`, `tray`, `mocks`. Root namespace exports and declarations are packaging/typing, not additional feature families.

**All 30 official Tauri plugins represented:** `autostart`, `barcode-scanner`, `biometric`, `cli`, `clipboard-manager`, `deep-link`, `dialog`, `fs`, `geolocation`, `global-shortcut`, `haptics`, `http`, `localhost`, `log`, `nfc`, `notification`, `opener`, `os`, `persisted-scope`, `positioner`, `process`, `shell`, `single-instance`, `sql`, `store`, `stronghold`, `updater`, `upload`, `websocket`, `window-state`.

**MAUI Essentials families represented:** AppActions, AppInfo, Browser, Launcher, MainThread, Map, Permissions, VersionTracking, Contacts, Email, Connectivity, Passkeys, PhoneDialer, Sms, WebAuthenticator, Battery, DeviceDisplay, DeviceInfo, Accelerometer, Barometer, Compass, Gyroscope, Magnetometer, OrientationSensor, Flashlight, Geocoding, Geolocation, HapticFeedback, Vibration, MediaPicker, Screenshot, TextToSpeech, UnitConverters, Clipboard, Share, FilePicker, FileSystem, Preferences, SecureStorage and SemanticScreenReader.

Native UI coverage additionally includes pages/navigation, layouts, modern controls, graphics, shapes, gestures, animations, resources/styles/templates/bindings, localization, accessibility, windows/menus, WebView/HybridWebView, handlers and legacy compatibility.

## Sources

Inventoried on **2026-10-12**. API documentation is live; source snapshots pin the original inventory.

- [Current sample, implementation and acceptance scope][sample-readme].
- [Standalone SDK imports, source documentation and release preparation][sdk-readme].
- [MAUI platform feature index][m-platform] and [native controls index][m-controls].
- [Tauri core source inventory](https://github.com/tauri-apps/tauri/tree/d62e5d9c0878b6fb327543cc9be50aa1696ee23c/packages/api/src).
- [Tauri plugin source inventory](https://github.com/tauri-apps/plugins-workspace/tree/c91a9e6af5e51d75d03ae289eb69534ef22fb923/plugins).
- [Official MAUI samples](https://github.com/dotnet/maui-samples), references rather than automatic net11 compatibility guarantees.

[sample-readme]: README.md
[sdk-readme]: JavaScript/README.md
[m-platform]: https://learn.microsoft.com/dotnet/maui/platform-integration/?view=net-maui-11.0
[m-controls]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/?view=net-maui-11.0
[m-hybrid]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0
[m-invoke]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0#invoke-c-from-javascript
[m-net11]: https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11?view=net-maui-11.0#hybridwebview-js-to-net-invocation
[m-appinfo]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/app-information?view=net-maui-11.0
[m-deviceinfo]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/information?view=net-maui-11.0
[m-clipboard]: https://learn.microsoft.com/dotnet/maui/platform-integration/data/clipboard?view=net-maui-11.0
[m-preferences]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/preferences?view=net-maui-11.0
[m-permissions]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions?view=net-maui-11.0
[m-mainthread]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/main-thread?view=net-maui-11.0
[m-assets]: https://learn.microsoft.com/dotnet/maui/fundamentals/single-project?view=net-maui-11.0#resource-files
[m-devtools]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0#enable-browser-developer-tools
[m-filepicker]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/file-picker?view=net-maui-11.0
[m-files]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/file-system-helpers?view=net-maui-11.0
[m-share]: https://learn.microsoft.com/dotnet/maui/platform-integration/data/share?view=net-maui-11.0
[m-secure]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/secure-storage?view=net-maui-11.0
[m-connectivity]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/networking?view=net-maui-11.0
[m-dialogs]: https://learn.microsoft.com/dotnet/maui/user-interface/pop-ups?view=net-maui-11.0
[m-launcher]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/launcher?view=net-maui-11.0
[m-browser]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/open-browser?view=net-maui-11.0
[m-window]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/window?view=net-maui-11.0
[m-lifecycle]: https://learn.microsoft.com/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-11.0
[m-embedding]: https://learn.microsoft.com/dotnet/maui/platform-integration/native-embedding?view=net-maui-11.0
[m-shell]: https://learn.microsoft.com/dotnet/maui/fundamentals/shell/?view=net-maui-11.0
[m-pages]: https://learn.microsoft.com/dotnet/maui/user-interface/pages/?view=net-maui-11.0
[m-collection]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/collectionview/?view=net-maui-11.0
[m-layouts]: https://learn.microsoft.com/dotnet/maui/user-interface/layouts/?view=net-maui-11.0
[m-safe]: https://learn.microsoft.com/dotnet/maui/user-interface/safe-area?view=net-maui-11.0
[m-theme]: https://learn.microsoft.com/dotnet/maui/user-interface/system-theme-changes?view=net-maui-11.0
[m-accessibility]: https://learn.microsoft.com/dotnet/maui/fundamentals/accessibility?view=net-maui-11.0
[m-menu]: https://learn.microsoft.com/dotnet/maui/user-interface/menu-bar?view=net-maui-11.0
[m-contextmenu]: https://learn.microsoft.com/dotnet/maui/user-interface/context-menu?view=net-maui-11.0
[m-keyboard]: https://learn.microsoft.com/dotnet/maui/user-interface/keyboard-accelerators?view=net-maui-11.0
[m-images]: https://learn.microsoft.com/dotnet/maui/user-interface/images/images?view=net-maui-11.0
[m-fonts]: https://learn.microsoft.com/dotnet/maui/user-interface/fonts?view=net-maui-11.0
[m-icon]: https://learn.microsoft.com/dotnet/maui/user-interface/images/app-icons?view=net-maui-11.0
[m-splash]: https://learn.microsoft.com/dotnet/maui/user-interface/images/splashscreen?view=net-maui-11.0
[m-resources]: https://learn.microsoft.com/dotnet/maui/fundamentals/resource-dictionaries?view=net-maui-11.0
[m-styles]: https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-11.0
[m-css]: https://learn.microsoft.com/dotnet/maui/user-interface/styles/css?view=net-maui-11.0
[m-bindings]: https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/?view=net-maui-11.0
[m-battery]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/battery?view=net-maui-11.0
[m-display]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/display?view=net-maui-11.0
[m-location]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/geolocation?view=net-maui-11.0
[m-geocoding]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/geocoding?view=net-maui-11.0
[m-maplaunch]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/maps?view=net-maui-11.0
[m-maps]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/map?view=net-maui-11.0
[m-media]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/picker?view=net-maui-11.0
[m-screenshot]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/screenshot?view=net-maui-11.0
[m-speech]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/text-to-speech?view=net-maui-11.0
[m-haptics]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/haptic-feedback?view=net-maui-11.0
[m-vibration]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/vibration?view=net-maui-11.0
[m-flashlight]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/flashlight?view=net-maui-11.0
[m-sensors]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/sensors?view=net-maui-11.0
[m-units]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/unit-converters?view=net-maui-11.0
[m-colors]: https://learn.microsoft.com/dotnet/maui/user-interface/graphics/colors?view=net-maui-11.0
[m-drag]: https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/drag-and-drop?view=net-maui-11.0
[m-native]: https://learn.microsoft.com/dotnet/maui/platform-integration/invoke-platform-code?view=net-maui-11.0
[m-weather]: https://github.com/dotnet/maui-samples/tree/main/10.0/Apps/WeatherTwentyOne
[m-applinks]: https://learn.microsoft.com/dotnet/maui/macios/universal-links?view=net-maui-11.0
[m-androidapplinks]: https://learn.microsoft.com/dotnet/maui/android/app-links?view=net-maui-11.0
[m-notifications]: https://learn.microsoft.com/dotnet/maui/platform-integration/local-notifications?view=net-maui-11.0
[m-actions]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/app-actions?view=net-maui-11.0
[m-version]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/version-tracking?view=net-maui-11.0
[m-titlebar]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/titlebar?view=net-maui-11.0
[m-webview]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/webview?view=net-maui-11.0
[m-sql]: https://learn.microsoft.com/dotnet/maui/data-cloud/database-sqlite?view=net-maui-11.0
[m-graphics]: https://learn.microsoft.com/dotnet/maui/user-interface/graphics/?view=net-maui-11.0
[m-shapes]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/shapes/?view=net-maui-11.0
[m-gestures]: https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/tap?view=net-maui-11.0
[m-animations]: https://learn.microsoft.com/dotnet/maui/user-interface/animation/basic?view=net-maui-11.0
[m-behaviors]: https://learn.microsoft.com/dotnet/maui/fundamentals/behaviors?view=net-maui-11.0
[m-triggers]: https://learn.microsoft.com/dotnet/maui/fundamentals/triggers?view=net-maui-11.0
[m-localization]: https://learn.microsoft.com/dotnet/maui/fundamentals/localization?view=net-maui-11.0
[m-contacts]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/contacts?view=net-maui-11.0
[m-email]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/email?view=net-maui-11.0
[m-sms]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/sms?view=net-maui-11.0
[m-phone]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/phone-dialer?view=net-maui-11.0
[m-auth]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/authentication?view=net-maui-11.0
[m-passkeys]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/passkeys?view=net-maui-11.0
[m-di]: https://learn.microsoft.com/dotnet/maui/fundamentals/dependency-injection?view=net-maui-11.0
[m-handlers]: https://learn.microsoft.com/dotnet/maui/user-interface/handlers/?view=net-maui-11.0
[m-effects]: https://learn.microsoft.com/dotnet/maui/migration/effects?view=net-maui-11.0
[m-platformspecifics]: https://learn.microsoft.com/dotnet/maui/platform-integration/?view=net-maui-11.0#access-platform-apis
[m-blazor]: https://learn.microsoft.com/aspnet/core/blazor/hybrid/?view=aspnetcore-11.0
[m-age]: https://github.com/dotnet/maui-samples/tree/main/10.0/AgeSignals
[m-billing]: https://github.com/dotnet/maui-samples/tree/main/10.0/BillingService
[m-hotreload]: https://learn.microsoft.com/dotnet/maui/xaml/hot-reload?view=net-maui-11.0
[m-deployment]: https://learn.microsoft.com/dotnet/maui/deployment/?view=net-maui-11.0
[m-aot]: https://learn.microsoft.com/dotnet/maui/deployment/nativeaot?view=net-maui-11.0
[m-profiling]: https://learn.microsoft.com/dotnet/maui/deployment/performance?view=net-maui-11.0
[m-uitesting]: https://learn.microsoft.com/dotnet/maui/deployment/ui-testing?view=net-maui-11.0
[net-json]: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview
[net-http]: https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient
[net-websocket]: https://learn.microsoft.com/dotnet/api/system.net.websockets.clientwebsocket
[net-process]: https://learn.microsoft.com/dotnet/api/system.diagnostics.process
[net-cli]: https://learn.microsoft.com/dotnet/standard/commandline/
[web-media]: https://developer.mozilla.org/docs/Web/API/MediaDevices/getUserMedia
[t-api]: https://v2.tauri.app/reference/javascript/api/
[t-core]: https://v2.tauri.app/reference/javascript/api/namespacecore/
[t-app]: https://v2.tauri.app/reference/javascript/api/namespaceapp/
[t-window]: https://v2.tauri.app/reference/javascript/api/namespacewindow/
[t-webview]: https://v2.tauri.app/reference/javascript/api/namespacewebview/
[t-webviewwindow]: https://v2.tauri.app/reference/javascript/api/namespacewebviewwindow/
[t-path]: https://v2.tauri.app/reference/javascript/api/namespacepath/
[t-dpi]: https://v2.tauri.app/reference/javascript/api/namespacedpi/
[t-image]: https://v2.tauri.app/reference/javascript/api/namespaceimage/
[t-menu]: https://v2.tauri.app/reference/javascript/api/namespacemenu/
[t-tray]: https://v2.tauri.app/reference/javascript/api/namespacetray/
[t-mocks]: https://v2.tauri.app/develop/tests/mocking/
[t-commands]: https://v2.tauri.app/develop/calling-rust/
[t-state]: https://v2.tauri.app/develop/state-management/
[t-events]: https://v2.tauri.app/develop/calling-frontend/#event-system
[t-channels]: https://v2.tauri.app/develop/calling-frontend/#channels
[t-capabilities]: https://v2.tauri.app/security/capabilities/
[t-config]: https://v2.tauri.app/reference/config/
[t-develop]: https://v2.tauri.app/develop/
[t-example]: https://github.com/tauri-apps/tauri/tree/d62e5d9c0878b6fb327543cc9be50aa1696ee23c/examples/api
[t-plugins]: https://v2.tauri.app/develop/plugins/
[t-clipboard]: https://v2.tauri.app/plugin/clipboard/
[t-dialog]: https://v2.tauri.app/plugin/dialog/
[t-fs]: https://v2.tauri.app/plugin/file-system/
[t-store]: https://v2.tauri.app/plugin/store/
[t-http]: https://v2.tauri.app/plugin/http-client/
[t-upload]: https://v2.tauri.app/plugin/upload/
[t-websocket]: https://v2.tauri.app/plugin/websocket/
[t-opener]: https://v2.tauri.app/plugin/opener/
[t-os]: https://v2.tauri.app/plugin/os-info/
[t-log]: https://v2.tauri.app/plugin/logging/
[t-geolocation]: https://v2.tauri.app/plugin/geolocation/
[t-haptics]: https://v2.tauri.app/plugin/haptics/
[t-shortcut]: https://v2.tauri.app/plugin/global-shortcut/
[t-deeplink]: https://v2.tauri.app/plugin/deep-linking/
[t-single]: https://v2.tauri.app/plugin/single-instance/
[t-notification]: https://v2.tauri.app/plugin/notification/
[t-windowstate]: https://v2.tauri.app/plugin/window-state/
[t-positioner]: https://v2.tauri.app/plugin/positioner/
[t-sql]: https://v2.tauri.app/plugin/sql/
[t-persistedscope]: https://v2.tauri.app/plugin/persisted-scope/
[t-biometric]: https://v2.tauri.app/plugin/biometric/
[t-barcode]: https://v2.tauri.app/plugin/barcode-scanner/
[t-nfc]: https://v2.tauri.app/plugin/nfc/
[t-stronghold]: https://v2.tauri.app/plugin/stronghold/
[t-autostart]: https://v2.tauri.app/plugin/autostart/
[t-process]: https://v2.tauri.app/plugin/process/
[t-shell]: https://v2.tauri.app/plugin/shell/
[t-cli]: https://v2.tauri.app/plugin/cli/
[t-localhost]: https://v2.tauri.app/plugin/localhost/
[t-plugin-authoring]: https://v2.tauri.app/develop/plugins/
[t-distribution]: https://v2.tauri.app/distribute/
[t-updater]: https://v2.tauri.app/plugin/updater/
[t-webdriver]: https://v2.tauri.app/develop/tests/webdriver/
