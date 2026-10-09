# Hybrid web app

This sample is being implemented to demonstrate Vite live edits in a .NET 11
Mac Catalyst app using `HybridWebView`.

The native app retains its virtual app origin while loading development assets
from a local Vite server. Vite owns frontend updates; editing HTML, CSS, or
JavaScript should not rebuild or restart the native app.

## Planned layout

```text
Controls.Sample.HybridWebApp/
  run-demo.sh
  Native/
    Maui.Controls.Sample.HybridWebApp.csproj
  Web/
    package.json
    vite.config.ts
    src/
```

The native and frontend project roots are separate so Node dependencies are not
implicitly discovered or packaged by the .NET project.

## Initial scope

- Mac Catalyst and .NET 11.
- A temporary root launcher using the existing frontend command and `dotnet run`.
- Safe development-resource forwarding and explicit error handling.
- Sample-local Debug helpers for inspecting the actual WebView DOM and snapshots,
  without a DevFlow dependency.

Commands, generated JS modules, new interop APIs, native C# Hot Reload, and SDK
integration are not part of this first demo.

## Status

This initial draft contains the sample description only. The app, frontend,
launcher, setup instructions, and validation evidence will be added in
incremental commits.
