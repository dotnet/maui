Fixes #34788

Aligns the MAUI Blazor Hybrid templates with recent non-Identity changes to the ASP.NET Core Blazor Web App template.

## Changes

### 1. Remove inline JS event handler from NavMenu ([aspnetcore#63828](https://github.com/dotnet/aspnetcore/pull/63828))
Replace inline `onclick` handler with a collocated `NavMenu.razor.js` ES6 module in both `maui-blazor` and `maui-blazor-solution` templates.

### 2. Fix RequestId flashing on Error page using PersistentState ([aspnetcore#64634](https://github.com/dotnet/aspnetcore/pull/64634))
Add `[PersistentState]` attribute to `RequestId` property in `Error.razor` and use null-coalescing assignment (`??=`) to prevent value flashing during interactive transition.

### 3. Use BasePath component instead of `<base href>` ([aspnetcore#64590](https://github.com/dotnet/aspnetcore/pull/64590))
Replace static `<base href="/" />` with `<BasePath />` component in the solution web template's `App.razor`.

### 4. Add Docker support flag ([aspnetcore#63815](https://github.com/dotnet/aspnetcore/pull/63815))
Add `"supportsDocker": true` to `ide.host.json` to enable the 'Add Docker Support' option in Visual Studio for the solution template.

## Already aligned (no changes needed)
- **ResourcePreloader** — already present in solution `App.razor`
- **ReconnectModal** — already present in solution `App.razor`
- **NotFoundPage on Router** — already present in both templates
- **Accessibility (aria-labels, color-scheme)** — already applied
- **Asset fingerprinting (`@Assets[]`)** — already applied in solution
- **dev.localhost URLs** — already applied in `launchSettings.json`

## Not applicable
- **Circuit pause/resume** — SignalR-only JS APIs, N/A for BlazorWebView
- **Primary constructors** — no DI-injected constructors in MAUI templates
