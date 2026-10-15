export type Experiment = 'identity' | 'clipboard' | 'preferences' | 'echo' | 'errors' | 'policy' | 'lifetime' | 'events' | 'work' | 'development' | 'welcome' | 'showcase' | 'windows' | 'multiWindow' | 'dialogs' | 'filePicker' | 'remoteWebView' | 'battery' | 'display' | 'menus' | 'theme' | 'connectivity' | 'textToSpeech' | 'launcher' | 'share' | 'geolocation'
export interface Feature { id: string; title: string; category: string; priority: string; experiment?: Experiment; description: string; docs: string; guide?: boolean }
const maui = 'https://learn.microsoft.com/dotnet/maui/'
const hybrid = `${maui}user-interface/controls/hybridwebview?view=net-maui-11.0`
const feature = (id: string, title: string, category: string, experiment: Experiment, description: string, docs = hybrid): Feature =>
  ({ id, title, category, priority: 'P0', experiment, description, docs })

export const features: Feature[] = [
  feature('identity', 'App and device identity', 'Essentials', 'identity', 'Read real app/device details, native PID and startup identity. Reload only the frontend and compare the startup identity.', `${maui}platform-integration/appmodel/app-information`),
  feature('clipboard', 'Text clipboard', 'Essentials', 'clipboard', 'Copy text to the OS clipboard, then explicitly paste it back. Try Unicode, emoji, quotes and multiple lines. No automatic clipboard reads.', `${maui}platform-integration/data/clipboard`),
  feature('preferences', 'Preferences', 'Essentials', 'preferences', 'Read and write actual native app settings. Clear deletes the entire default native store, including existing settings. Never put secrets here.', `${maui}platform-integration/storage/preferences`),
  feature('arguments', 'Arguments and return values', 'Bridge', 'echo', 'Roundtrip a typed DTO or 256 KiB Blob, download a controlled native PDF byte document, and asynchronously consume ordered native chunks with explicit backpressure. No OS sharing or writable filesystem is claimed.'),
  feature('registration', 'Custom command registration', 'Bridge', 'echo', 'Annotated actions generate direct calls and owned AOT JSON metadata. One explicit assembly registration adds JSON and binary actions without a forwarding switch.'),
  feature('modules', 'Typed JavaScript modules', 'Bridge', 'echo', 'The UI imports named clipboard, preferences, info and application helpers. Authored decoders validate native outputs. Wire strings belong to these modules, not UI handlers.'),
  feature('hub', 'One native target hub', 'Bridge', 'echo', 'One per-WebView HybridAppTargetObject exposes only Invoke(string). The .NET 11 generated invocation bridge routes into explicitly registered typed providers.'),
  feature('errors', 'Errors and operation log', 'Bridge', 'errors', 'Deliberately reject a native command. The operation log distinguishes native errors from transport failures, cancellation and successful results.'),
  feature('policy', 'Explicit exposure and permissions', 'Bridge', 'policy', 'Try a missing command, disabled command or invalid DTO. Application exposure policy is distinct from platform support and OS permission denial.'),
  feature('lifetime', 'Threading and service lifetime', 'Bridge', 'lifetime', 'Read the service counter, echo a DTO, then reload the frontend. The native service and startup identity survive; UI APIs run on the main thread.'),
  feature('cleanup', 'Reload and disconnect behaviour', 'Bridge', 'work', 'Start native work and reload during progress. Pagehide disconnects the session; an inactivity lease handles abrupt loss. Old listeners and operations do not survive into the new document.'),
  feature('events', 'Events and subscriptions', 'Bridge', 'events', 'Subscribe to native service changes, echo a DTO to emit a change, then unsubscribe. One listener per topic per document; repeated subscription is rejected.'),
  feature('work', 'Progress and cancellation', 'Bridge', 'work', 'Start bounded asynchronous native work, receive correlated progress events and cancel it. Cancelled is a terminal state, never a success-shaped value.'),
  feature('readiness', 'Bridge bootstrap and readiness', 'Bridge', 'identity', 'Load the framework bridge from the native origin and connect a document session. Browser-only mode lists the catalog but never emulates native APIs.'),
  feature('browser', 'Feature browser', 'Development', 'development', 'Search implemented experiments, collapse navigation groups, and follow API documentation. Only implemented experiments are shown.'),
  feature('vite', 'Vite live development', 'Development', 'development', 'Edit the HTML title, TypeScript message, CSS surface and dynamically imported details module. Only frontend code reloads; native startup identity must stay unchanged.'),
  feature('offline', 'Packaged offline content', 'Development', 'development', 'Build and stage Web/dist explicitly before native asset collection. The packaged app uses this same playground UI and native bridge without Vite.'),
  feature('inspection', 'Native inspection and repeatable checks', 'Development', 'identity', 'Optional authenticated Debug probes inspect actual DOM/results and native startup identity. Runtime commands work without the inspector. Test mocks are never enabled in the app.'),
  feature('platform', 'Platform-aware results', 'Essentials', 'policy', 'Unknown or disabled APIs are rejected explicitly. Native unsupported/permission failures are errors; planned experiments never pretend to return native values.'),
]

function planned(priority: string, category: string, titles: string[], docs: string): void {
  for (const title of titles) features.push({
    id: `planned-${features.length}`, title, category, priority, docs,
    description: 'Not implemented in this milestone. The catalog records a feature family, not a claim of native support or verification.',
  })
}
planned('P1', 'Storage & network', [
  'File picker', 'File access', 'Save/export destination', 'Paths and packaged resources', 'Structured local store',
  'Secure key/value storage', 'HTTP requests', 'Uploads and downloads', 'WebSocket', 'Connectivity',
], `${maui}platform-integration/`)
planned('P1', 'OS integration', [
  'Native dialogs', 'Open external links', 'In-app browser', 'Share text and files', 'Window basics', 'Multiple windows', 'Application lifecycle',
], `${maui}fundamentals/`)
planned('P1', 'Native UI', [
  'Mixed native/web surface', 'Navigation', 'Text and actions', 'Choices and values', 'Date/time and busy states',
  'Lists and search', 'Responsive layouts', 'Theme and appearance', 'Accessibility', 'Native menus, toolbars and context menus',
  'Images, fonts, icons and splash', 'Resources, styles and bindings',
], `${maui}user-interface/controls/`)
planned('P2', 'Device & media', [
  'Battery', 'Display and DPI', 'Keep screen awake', 'Location', 'Geocoding', 'Launch system maps', 'Embedded native map',
  'Photo/video picker and capture', 'Screenshot', 'Text-to-speech', 'Haptic feedback', 'Vibration', 'Flashlight',
  'Accelerometer and shake', 'Gyroscope', 'Compass', 'Magnetometer', 'Barometer', 'Orientation sensor', 'Conversions',
], `${maui}platform-integration/`)
planned('P2', 'OS integration', [
  'Drag and drop', 'Tray/status icon', 'Global shortcuts', 'Deep links and file associations', 'Single instance', 'Notifications',
  'App actions', 'Version tracking', 'Persisted window state', 'Advanced window behaviour', 'Window positioning',
  'WebView controls', 'Local SQL', 'Persisted file access scopes',
], `${maui}platform-integration/`)
planned('P2', 'Native UI', [
  'Graphics and drawing', 'Shapes, brushes and decoration', 'Gestures and animation', 'Scrolling, refresh and swiping',
  'Carousel and indicators', 'Page/layout variants', 'UI behaviours and triggers', 'Localization and RTL', 'Contacts',
  'Email', 'SMS', 'Phone dialer', 'Web camera/microphone',
], `${maui}user-interface/`)
planned('P3', 'Extensions', [
  'Browser authentication', 'Passkeys', 'Local biometrics', 'Barcode/QR scanning', 'NFC', 'Encrypted vault', 'Autostart',
  'Process lifecycle', 'Shell and sidecar process', 'Command-line arguments', 'Localhost asset server', 'Native resource handles',
  'Plugin/provider extension', 'Platform-specific custom UI', 'Blazor Hybrid companion', 'Legacy control compatibility',
  'Age signals', 'In-app purchases',
], `${maui}platform-integration/`)
planned('P3', 'Development', [
  'Native Hot Reload', 'Build/stage frontend assets', 'Installable artifacts', 'Signing and installed execution',
  'Updates', 'Trimming and NativeAOT', 'Diagnostics and performance', 'Browser mocks and native automation',
], `${maui}deployment/`)

function promote(title: string, id: string, category: string, experiment: Experiment, description: string, docs: string): void {
  const entry = features.find(feature => feature.title === title && !feature.experiment)
  if (!entry) throw new Error(`Missing backlog family: ${title}`)
  Object.assign(entry, { id, category, experiment, description, docs })
}
promote('Window basics', 'windows', 'App & windows', 'windows',
  'Read and control the current native window. Observe actual OS size changes live; move events and centering are Windows-only. Mobile placement is unsupported.', `${maui}user-interface/controls/window`)
promote('Multiple windows', 'multi-window', 'App & windows', 'multiWindow',
  'Open a known bundled page in a fresh window and provider scope. List or close children opened here. iPhone has no separate windows; internal routes are not OS deep links.', `${maui}user-interface/controls/window`)
promote('Native dialogs', 'dialogs', 'App & windows', 'dialogs',
  'Try native alerts, confirmations, text prompts and action sheets in separate sections on the calling page. User cancellation is normal.', `${maui}user-interface/pop-ups`)
promote('File picker', 'file-picker', 'Essentials', 'filePicker',
  'Choose a file with the system document picker. Preview complete UTF-8 text up to 64 KiB; binary, other encodings and oversized files are rejected. No arbitrary file paths or saved handles.', `${maui}platform-integration/storage/file-picker`)
promote('WebView controls', 'remote-webview', 'App & windows', 'remoteWebView',
  'Load a real unbridged desktop native browser guest anchored to a measured slot inside the web UI. No iframe, separate window or native split-row layout.', `${maui}user-interface/controls/webview`)
promote('Native menus, toolbars and context menus', 'menus', 'App & windows', 'menus',
  'Use a real desktop native menu bar for scoped windows, internal navigation, reload and native About. Mobile menu bars are explicitly unsupported.', `${maui}user-interface/menu-bar`)
promote('Theme and appearance', 'theme', 'App & windows', 'theme',
  'Choose native application light/dark/unspecified preference from TypeScript; unspecified follows the OS. Apply actual native theme-change events to Svelte CSS. This is not a per-window or remote-site theme.', `${maui}user-interface/system-theme-changes`)
promote('Battery', 'battery', 'Essentials', 'battery',
  'Read native power properties and explicitly observe battery or energy-saver changes. Leaving removes only this page’s callbacks; no browser emulation.', `${maui}platform-integration/device/battery`)
promote('Display and DPI', 'display', 'Essentials', 'display',
  'Read actual main-display metrics, explicitly observe native changes and edit KeepScreenOn. These are native properties, not CSS viewport measurements.', `${maui}platform-integration/device/display`)
promote('Connectivity', 'connectivity', 'Essentials', 'connectivity',
  'Read fresh native network conditions and subscribe to actual OS network changes. Unsubscribe detaches this listener; Internet is not a reachability guarantee.', `${maui}platform-integration/communication/networking`)
promote('Text-to-speech', 'text-to-speech', 'Essentials', 'textToSpeech',
  'Speak on an explicit click with native locales, pitch, volume and rate. Cancel only this speech through its actual native operation token.', `${maui}platform-integration/device-media/text-to-speech`)
promote('Open external links', 'launcher', 'Essentials', 'launcher',
  'Query or launch a native URI on an explicit click. Native schemes and boolean refusal pass through; never an in-app browser or embedded guest.', `${maui}platform-integration/appmodel/launcher`)
promote('Location', 'geolocation', 'Essentials', 'geolocation',
  'Request actual native location with MAUI request fields, explicit permission and cancellable timeout. Coordinates stay in this panel, not the operation log.', `${maui}platform-integration/device/geolocation`)
promote('Share text and files', 'share', 'Essentials', 'share',
  'Stage browser-produced PDF/text bytes with opaque document authority, then explicitly open the OS text/file/multi-file share UI. Native request completion is not file consumption.', `${maui}platform-integration/data/share`)

const canonicalRoutes: Partial<Record<Experiment, string>> = {
  identity: 'identity', echo: 'arguments', policy: 'policy', work: 'work', development: 'vite',
}
export const availableFeatures = features.filter(feature => feature.experiment &&
  (!canonicalRoutes[feature.experiment] || canonicalRoutes[feature.experiment] === feature.id))
export const guidePages: Feature[] = [
  { ...feature('welcome', 'Welcome to the playground', 'Welcome', 'welcome',
    'A real web interface, a real native host. Explore the surface here; launch the MAUI app to try its native capabilities.'), priority: 'guide', guide: true },
  { ...feature('showcase', 'Try it hands-on', 'Welcome', 'showcase',
    'Change your own window, ask the OS for input and read the device. These quick actions use the same public SDK as the detailed feature pages.'), priority: 'guide', guide: true },
]
export const navigationPages = [...guidePages, ...availableFeatures]
