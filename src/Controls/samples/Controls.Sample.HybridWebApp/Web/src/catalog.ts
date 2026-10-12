export type Experiment = 'identity' | 'clipboard' | 'preferences' | 'echo' | 'errors' | 'policy' | 'lifetime' | 'events' | 'work' | 'development'
export interface Feature { id: string; title: string; category: string; priority: string; experiment?: Experiment; description: string; docs: string }
const maui = 'https://learn.microsoft.com/dotnet/maui/'
const hybrid = `${maui}user-interface/controls/hybridwebview?view=net-maui-11.0`
const feature = (id: string, title: string, category: string, experiment: Experiment, description: string, docs = hybrid): Feature =>
  ({ id, title, category, priority: 'P0', experiment, description, docs })

export const features: Feature[] = [
  feature('identity', 'App and device identity', 'Essentials', 'identity', 'Read real app/device details, native PID and startup identity. Reload only the frontend and compare the startup identity.', `${maui}platform-integration/appmodel/app-information`),
  feature('clipboard', 'Text clipboard', 'Essentials', 'clipboard', 'Copy text to the OS clipboard, then explicitly paste it back. Try Unicode, emoji, quotes and multiple lines. No automatic clipboard reads.', `${maui}platform-integration/data/clipboard`),
  feature('preferences', 'Preferences', 'Essentials', 'preferences', 'Save, read and remove one namespaced setting. Restart the native app to verify persistence. Never put secrets here.', `${maui}platform-integration/storage/preferences`),
  feature('arguments', 'Arguments and return values', 'Bridge', 'echo', 'An asynchronous native service round-trips a typed DTO containing text or null, a number, a boolean and an array. Each successful echo increments its native instance counter.'),
  feature('registration', 'Custom command registration', 'Bridge', 'echo', 'SampleCommands registers typed handlers and JSON metadata explicitly. Add a provider without editing a forwarding switch. There is no attribute or TypeScript generator in this prototype.'),
  feature('modules', 'Typed JavaScript modules', 'Bridge', 'echo', 'The UI imports named clipboard, preferences, info and application helpers. Authored decoders validate native outputs. Wire strings belong to these modules, not UI handlers.'),
  feature('hub', 'One native target hub', 'Bridge', 'echo', 'One per-WebView HybridAppTargetObject exposes only Invoke(string). The .NET 11 generated invocation bridge routes into explicitly registered typed providers.'),
  feature('errors', 'Errors and operation log', 'Bridge', 'errors', 'Deliberately reject a native command. The operation log distinguishes native errors from transport failures, cancellation and successful results.'),
  feature('policy', 'Explicit exposure and permissions', 'Bridge', 'policy', 'Try a missing command, disabled command or invalid DTO. Application exposure policy is distinct from platform support and OS permission denial.'),
  feature('lifetime', 'Threading and service lifetime', 'Bridge', 'lifetime', 'Read the service counter, echo a DTO, then reload the frontend. The native service and startup identity survive; UI APIs run on the main thread.'),
  feature('cleanup', 'Reload and disconnect behaviour', 'Bridge', 'work', 'Start native work and reload during progress. Pagehide disconnects the session; an inactivity lease handles abrupt loss. Old listeners and operations do not survive into the new document.'),
  feature('events', 'Events and subscriptions', 'Bridge', 'events', 'Subscribe to native service changes, echo a DTO to emit a change, then unsubscribe. One listener per topic per document; repeated subscription is rejected.'),
  feature('work', 'Progress and cancellation', 'Bridge', 'work', 'Start bounded asynchronous native work, receive correlated progress events and cancel it. Cancelled is a terminal state, never a success-shaped value.'),
  feature('readiness', 'Bridge bootstrap and readiness', 'Bridge', 'identity', 'Load the framework bridge from the native origin and connect a document session. Browser-only mode lists the catalog but never emulates native APIs.'),
  feature('browser', 'Feature browser', 'Playground', 'development', 'Search the catalog, filter by category and priority, and follow API documentation. Later families are explicitly marked Not implemented.'),
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

export const availableFeatures = features.filter(feature => feature.experiment)
