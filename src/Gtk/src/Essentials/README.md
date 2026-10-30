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
| Email / Map | `mailto:` (no attachments/HTML) / `geo:` URIs |
| FilePicker / MediaPicker | `Gtk.FileDialog`; filter strings: MIME types, `*.ext`, bare extensions. No camera capture |
| Screenshot | `WidgetPaintable` -> `Gsk.Renderer.RenderTexture` -> PNG (JPEG via gdk-pixbuf). Also keyed DI hooks `Microsoft.Maui.ViewCapture` / `WindowCapture` used by `CaptureAsync` |
| SemanticScreenReader | `Gtk.Accessible.Announce` (guarded: GTK >= 4.14) |
| VersionTracking | Stock implementation over Preferences (not re-registered) |
| Sensors, Contacts, Flashlight, Vibration, Haptics, Sms, PhoneDialer, Share, TTS, Geolocation | Registered stubs: `IsSupported == false`, calls throw `FeatureNotSupportedException` (Geolocation via GeoClue2 is a later item) |

Notes

- GTK floor is 4.14; newer APIs are version-guarded.
- Threading: GTK-bound APIs marshal to the main thread via `GLib.Idle`; blocking getters (`HasText`, `MainDisplayInfo`) block the caller until it runs.
- Seam in Essentials: `FilePickerFileTypeBackend.Factory` (`FilePicker.netstandard.watchos.tvos.cs`). `UseMauiGtkEssentials` installs `GtkFilePickerFileTypes.Create`, so `FilePickerFileType.Images/Png/Jpeg/Video/Pdf` carry MIME types for platform `Linux`. Without it they throw `NotImplementedInReferenceAssemblyException` as upstream (test in `Essentials.UnitTests`). The members are `static readonly`: call `UseMauiGtkEssentials` before first use of `FilePickerFileType`.

Tests: `src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Essentials` (xUnit). Pure logic runs anywhere; `[GtkFact]` tests need a GTK 4 runtime/display
and are skipped otherwise (`MAUI_GTK_REQUIRE_NATIVE=1` turns the skip into a failure). Linux headless:
`GDK_BACKEND=x11 dbus-run-session -- xvfb-run -a dotnet test`.
