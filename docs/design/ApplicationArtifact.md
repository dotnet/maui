# ApplicationArtifact metadata in .NET MAUI

`@(ApplicationArtifact)` is the shared public item group for final application artifacts. Platform SDKs own the artifact identity, path, format, and platform-specific metadata:

- .NET for Android creates APK and AAB items.
- .NET for iOS, Mac Catalyst, tvOS, and macOS creates `.app`, `.ipa`, `.pkg`, and `.xcarchive` items.
- Other platforms should populate the same item group from their own build or publish artifact pipeline.

MAUI does not rediscover platform package files and does not create a parallel MAUI-specific artifact item group. Instead, MAUI participates through `$(GetApplicationArtifactsDependsOn)` and updates existing `@(ApplicationArtifact)` items with MAUI project metadata after the platform SDK has populated them.

The MAUI metadata enrichment target adds these metadata values when the matching project properties are set:

- `ApplicationId`
- `ApplicationIdGuid`
- `ApplicationName`, mapped from `ApplicationTitle`
- `ApplicationTitle`
- `ApplicationDisplayVersion`
- `ApplicationVersion`

`GetApplicationArtifacts` and `Publish` remain platform-owned result paths. They run platform artifact population first, then execute targets appended to `$(GetApplicationArtifactsDependsOn)` before returning `@(ApplicationArtifact)` items. Replacing `$(GetApplicationArtifactsDependsOn)` must not bypass platform artifact population; platform SDKs keep required build/artifact-population targets outside that extensibility property.
