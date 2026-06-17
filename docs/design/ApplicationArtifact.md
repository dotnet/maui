# ApplicationArtifact metadata in .NET MAUI

`@(ApplicationArtifact)` is the shared public item group for final application artifacts. Platform SDKs own creating these items and their artifact identity, path, format, and platform-specific metadata:

- .NET for Android creates APK and AAB items.
- .NET for iOS, Mac Catalyst, tvOS, and macOS creates `.app`, `.ipa`, `.pkg`, and `.xcarchive` items.
- Other platforms should populate the same item group from their own build or publish pipeline.

MAUI does not rediscover platform package files and does not create a parallel MAUI-specific artifact item group. Instead, MAUI participates through `$(GetApplicationArtifactsDependsOn)` and updates existing `@(ApplicationArtifact)` items with MAUI project metadata after the platform SDK `GetApplicationArtifacts` target has run `Build` and platform-produced items exist.

The MAUI metadata enrichment target adds these metadata values when the matching project properties are set:

- `ApplicationId`
- `ApplicationIdGuid`
- `ApplicationName`, mapped from `ApplicationTitle`
- `ApplicationTitle`
- `ApplicationDisplayVersion`
- `ApplicationVersion`

`GetApplicationArtifacts` and `Publish` remain platform-owned result paths. Platform SDK `GetApplicationArtifacts` depends on `Build`, then executes targets appended to `$(GetApplicationArtifactsDependsOn)` before returning `@(ApplicationArtifact)` items. `Publish` uses the same post-`Build` extension path before returning items. Replacing `$(GetApplicationArtifactsDependsOn)` must not bypass platform build or platform artifact population; platform SDKs keep `Build` outside that extensibility property.
