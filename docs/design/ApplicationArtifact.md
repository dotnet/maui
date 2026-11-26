# ApplicationArtifact metadata in .NET MAUI

`@(ApplicationArtifact)` is the shared public item group for final application artifacts. Platform SDKs own creating these items and their artifact identity, path, format, and platform-specific metadata:

- .NET for Android creates APK and AAB items.
- .NET for iOS, Mac Catalyst, tvOS, and macOS creates `.app`, `.ipa`, `.pkg`, and `.xcarchive` items.
- `Microsoft.Maui.Core` creates items for Windows App SDK applications until an equivalent ships in the Windows App SDK itself.
- Other platforms should populate the same item group from their own build or publish pipeline.

MAUI does not rediscover platform package files and does not create a parallel MAUI-specific artifact item group. Each producer attaches common metadata from its final manifest or resolved build output:

- `ApplicationId`
- `ApplicationName`
- `ApplicationTitle`
- `ApplicationDisplayVersion`
- `ApplicationVersion`

Platform-specific metadata remains available alongside the common values, such as Android `PackageId` and Apple `BundleIdentifier`. Every item has an absolute path. Resource-backed names remain resource references when the build cannot resolve a single locale.

## Consuming artifacts

Each platform defines `GetApplicationArtifacts`, which depends on `Build` and returns `@(ApplicationArtifact)`. Tools should call that target instead of relying on when a particular platform target happens to populate the items:

```bash
dotnet build -f net11.0-android -t:GetApplicationArtifacts -getItem:ApplicationArtifact
dotnet publish -f net11.0-ios -getItem:ApplicationArtifact
```

Targets that add or update metadata should extend `$(GetApplicationArtifactsDependsOn)` and update the platform-created items rather than creating new ones. Publish-path items are available to `AfterTargets="Publish"` consumers in the same invocation.

## Windows App SDK

The Windows producer ships in the `buildTransitive` targets of `Microsoft.Maui.Core` and is imported only for Windows target frameworks that use the Windows App SDK. It activates for real, non-design-time application builds with `Exe` or `WinExe` output. Set `EnableWindowsApplicationArtifacts` to `false` to disable it. Classic `.wapproj` packaging is not supported.

It reads final Windows App SDK output properties and item lists rather than searching output directories, and reports only final application outputs:

| Output | `PackageFormat` | `IsDirectory` |
|---|---|---|
| Unpackaged (`WindowsPackageType=None`) build or publish directory | `directory` | `true` |
| MSIX package (including sparse packages) | `msix` | `false` |
| Bundle | `msixbundle` | `false` |
| Store upload | `msixupload` | `false` |
| App Installer file | `appinstaller` | `false` |

Legacy `appx`, `appxbundle`, and `appxupload` formats are reported the same way. Intermediate files, dependency framework packages, symbols, certificates, install scripts, and loose upload contents are not application artifacts.

Windows items also provide `Signed`, `EntryPoint` (the application executable name), `RuntimeIdentifier`, and `PlatformName=Windows`. Packaged identity, display name, and version metadata comes from the final Appx manifest, including unchanged `ms-resource:` references. A four-part package version `a.b.c.d` maps to `ApplicationDisplayVersion=a.b.c` and `ApplicationVersion=d`.
