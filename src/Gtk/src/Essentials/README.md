# Microsoft.Maui.Essentials.Gtk

Linux/GTK 4 implementations of .NET MAUI Essentials (`net10.0`, GtkSharp4). Registered in DI, then bridged to the static
facades by MAUI's `EssentialsInitializer` (`BridgeEssentialsFromDI`).

```csharp
builder.UseMauiApp<App>().UseMauiGtkEssentials(o => { o.ApplicationId = "com.contoso.notes"; });
```

Call it on the GTK main thread (that thread is remembered for marshalling). Existing registrations are never replaced.
`GtkEssentialsOptions`: `ApplicationId`, `ApplicationName`, `ParentWindowProvider` (parent of dialogs/launchers, target of
window screenshots and announcements; default = active window of `Gtk.Application`).

| API | Implementation |
|---|---|
| AppInfo | Entry assembly (`ApplicationId` / `ApplicationTitle` / `ApplicationDisplayVersion` / `ApplicationVersion` `AssemblyMetadata`, which the SDK emits from the project properties, then title, version). Theme: portal `org.freedesktop.appearance color-scheme`, fallback `Gtk.Settings` |
| DeviceInfo | `DevicePlatform "Linux"`, `/etc/os-release` (`VERSION_ID`, kernel on rolling), DMI model/vendor, VM detection, Desktop idiom |
| DeviceDisplay | First `Gdk.Display` monitor (geometry x scale). `KeepScreenOn` = `Gtk.Application.Inhibit(Idle)` |
| FileSystem | `$XDG_DATA_HOME/<id>`, `$XDG_CACHE_HOME/<id>`; package files from `AppContext.BaseDirectory` |
| Preferences | JSON `$XDG_CONFIG_HOME/<id>/preferences[.<shared>].json`, atomic writes, cached (no cross-process sync) |
| SecureStorage | libsecret `secret_password_{store,lookup,clear}v_sync` bound at run time (`libsecret-1.so.0`); `FeatureNotSupportedException` if missing |
| Connectivity | `Gio.NetworkMonitor` for access; `/sys/class/net` for Wi-Fi/Ethernet/Cellular profiles |
| Battery | `/sys/class/power_supply` (no D-Bus); polled every 30 s only while subscribed |
| Clipboard | `Gdk.Clipboard` (async read, `Changed` event) |
| Launcher / Browser | `Gtk.UriLauncher` / `Gtk.FileLauncher` (OpenURI portal) |
| Share | `GtkShare`, a desktop approximation (no share sheet): text to the clipboard, `Uri` opened with `Gtk.UriLauncher`; files (single or multiple) opened with `Gtk.FileLauncher` (OpenURI portal), falling back to `OpenContainingFolder` when no application handles them; every file is attempted, failures reported together |
| Email / Map | `mailto:` (no attachments/HTML) / `geo:` URIs |
| FilePicker / MediaPicker | `Gtk.FileDialog`; filter strings: MIME types, `*.ext`, bare extensions. No camera capture |
| Screenshot | `WidgetPaintable` -> `Gsk.Renderer.RenderTexture` -> PNG (JPEG via gdk-pixbuf). Also keyed DI hooks `Microsoft.Maui.ViewCapture` / `WindowCapture` used by `CaptureAsync` |
| SemanticScreenReader | `Gtk.Accessible.Announce` on the window content (guarded: GTK >= 4.14; announcing on the toplevel crashes the Windows AccessKit backend) |
| VersionTracking | Stock implementation over Preferences (not re-registered) |
| TextToSpeech | Linux: speech-dispatcher (`libspeechd.so.2`, bound at run time, one `SPDConnection` per utterance, end/cancel callbacks complete the task, `spd_cancel` on the token). Windows: SAPI `SAPI.SpVoice` via COM automation, polled `WaitUntilDone`, purge on cancel, pitch via `<pitch absmiddle>` XML. `GetLocalesAsync` lists the engine voices (`Locale.Id` = voice id, `Country` null); `SpeechOptions.Locale` picks the voice by id, then language tag, then primary language, else the engine default. Rate/pitch/volume are scaled from the MAUI ranges; utterances queue. No engine: `FeatureNotSupportedException` |
| Geolocation | Linux: GeoClue2 over `GLib.DBusConnection` (system bus): `Manager.CreateClient` (one client per session so overlapping sessions never stop or reconfigure each other; falls back to the per-peer `GetClient` on GeoClue < 2.5), `DesktopId` = app id, `RequestedAccuracyLevel` from `GeolocationAccuracy` (Lowest = city, Low = neighborhood, Default/Medium = street, High/Best = exact), `TimeThreshold` = `MinimumTime` (whole seconds, rounded up), `Start`, `LocationUpdated` -> `Location` (accuracy, altitude (geoid), speed, heading, timestamp; GeoClue's "unknown" values become `null`). Windows: the Location API automation objects (`LocationDisp.LatLongReportFactory`, late bound COM, polled every `max(MinimumTime, 500 ms)`; needs a location sensor/service and no trimming/AOT). `GetLocationAsync` returns the first fix, `null` on `Timeout`, throws `OperationCanceledException` on cancel, `FeatureNotEnabledException` when services are off, `PermissionException` when refused. `StartListeningForegroundAsync` returns `false` when disabled/unstartable; `ListeningFailed` stops listening. Events are raised on the GTK thread. `IsEnabled` is probed off the caller's thread in `GetLocationAsync`, and each `LocationUpdated` is read on a worker (never blocking the GTK thread). `GetLastKnownLocationAsync` = latest fix this process received (Windows: also the service's current report); GeoClue has none cached. Authorization: the GeoClue agent (GNOME/KDE) prompts per `DesktopId`; clients are recycled, not deleted (binding cannot build the `o` argument of `DeleteClient`) |
| Sensors, Contacts, Flashlight, Vibration, Haptics, Sms, PhoneDialer | Registered stubs: `IsSupported == false`, calls throw `FeatureNotSupportedException` |

Notes

- GTK floor is 4.14; newer APIs are version-guarded.
- Threading: GTK-bound APIs marshal to the main thread via `GLib.Idle`; blocking getters (`HasText`, `MainDisplayInfo`) block the caller until it runs.
- Seam in Essentials: `FilePickerFileTypeBackend.Factory` (`FilePicker.netstandard.watchos.tvos.cs`). `UseMauiGtkEssentials` installs `GtkFilePickerFileTypes.Create`, so `FilePickerFileType.Images/Png/Jpeg/Video/Pdf` carry MIME types for platform `Linux`. Without it they throw `NotImplementedInReferenceAssemblyException` as upstream (test in `Essentials.UnitTests`). The members are `static readonly`: call `UseMauiGtkEssentials` before first use of `FilePickerFileType`.

Tests: `src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Essentials` (xUnit). Pure logic runs anywhere; `[GtkFact]` tests need a GTK 4 runtime/display
and are skipped otherwise (`MAUI_GTK_REQUIRE_NATIVE=1` turns the skip into a failure). Linux headless:
`GDK_BACKEND=x11 dbus-run-session -- xvfb-run -a dotnet test`.

Geolocation tests use fake sources (`ILocationSource`, `IGeoClueClient`, `ILocationReports`); two Linux tests run the real D-Bus code (overlapping sessions get separate clients; `GetClient` fallback) against a GeoClue2 look-alike (python3 + PyGObject, session bus) and returns early without them. No test needs a real GeoClue2 or location sensor.
