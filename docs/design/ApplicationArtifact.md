# ApplicationArtifact metadata in .NET MAUI

`@(ApplicationArtifact)` is the shared public item group for final application artifacts. Platform SDKs own creating these items and their artifact identity, path, format, and platform-specific metadata:

- .NET for Android creates APK and AAB items.
- .NET for iOS, Mac Catalyst, tvOS, and macOS creates `.app`, `.ipa`, `.pkg`, and `.xcarchive` items.
- `Microsoft.Maui.ApplicationArtifacts.Windows` creates items for modern SDK-style Windows App SDK applications.
- Other platforms should populate the same item group from their own build or publish pipeline.

MAUI does not rediscover platform package files and does not create a parallel MAUI-specific artifact item group. Each producer attaches common metadata from its final manifest or resolved build output:

- `ApplicationId`
- `ApplicationName`
- `ApplicationTitle`
- `ApplicationDisplayVersion`
- `ApplicationVersion`

Platform-specific metadata remains available alongside the common values, such as Android `PackageId`, Apple `BundleIdentifier`, and Windows `PackageVersion`. `ApplicationIdGuid` is Windows-specific and is present only when the effective package identity is a GUID. Resource-backed names remain resource references when the build cannot resolve a single locale.

Each platform owns `GetApplicationArtifacts`, retains `$(GetApplicationArtifactsDependsOn)` as the downstream extension point, and makes the collected items available during Publish.

## Windows App SDK

`Microsoft.Maui.ApplicationArtifacts.Windows` is a targets-only `buildTransitive` package with no MAUI runtime, Resizetizer, or MAUI build-task dependency. `Microsoft.Maui.Core` depends on it only for Windows target frameworks, and non-MAUI Windows App SDK applications can reference it directly. It activates for real, non-design-time Windows App SDK application builds with `Exe` or `WinExe` output. Set `EnableWindowsApplicationArtifacts` to `false` to disable it. Classic `.wapproj` packaging is not supported.

The package defines `GetApplicationArtifacts`, retains `$(GetApplicationArtifactsDependsOn)` as the downstream extension point, and makes publish-path items available to `AfterTargets="Publish"` consumers in the same invocation. It reads final Windows App SDK output properties and item lists rather than searching output directories.

Each Windows item has an `ArtifactRole`:

- `Primary`
- `PayloadDirectory`
- `Bundle`
- `StoreUpload`
- `DeploymentManifest`
- `Symbols`
- `Certificate`
- `InstallScript`
- `DependencyPackage`
- `LandingPage`
- `Support`

Windows items also provide `IsPrimary`, `PrimaryArtifact`, `PackageFormat`, `PackageType`, `PackageVersion`, `Architecture`, `RuntimeIdentifier`, `Signed`, `BundlePlatforms`, `EntryPoint`, and `DeploymentDirectory` when available. Packaged identity, display name, and version metadata comes from the final Appx manifest, including unchanged `ms-resource:` references.
