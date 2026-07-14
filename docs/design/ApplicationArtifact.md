# ApplicationArtifact metadata in .NET MAUI

`@(ApplicationArtifact)` is the shared public item group for final application artifacts. Platform SDKs own creating these items and their artifact identity, path, format, and platform-specific metadata:

- .NET for Android creates APK and AAB items.
- .NET for iOS, Mac Catalyst, tvOS, and macOS creates `.app`, `.ipa`, `.pkg`, and `.xcarchive` items.
- Other platforms should populate the same item group from their own build or publish pipeline.

MAUI does not rediscover platform package files and does not create a parallel MAUI-specific artifact item group. Instead, it defines default MAUI metadata for the shared `ApplicationArtifact` item type. Platform SDKs create the items, and the defaults supplement their platform-specific metadata.

MAUI supplies these metadata values when the matching project properties are set:

- `ApplicationId`
- `ApplicationIdGuid`
- `ApplicationName`, mapped from `ApplicationTitle`
- `ApplicationTitle`
- `ApplicationDisplayVersion`
- `ApplicationVersion`

The defaults are declared with an MSBuild `ItemDefinitionGroup`, so explicit metadata from a platform SDK takes precedence. The defaults also apply when a platform recreates an item, as Android does when changing artifact identities to published paths.

`GetApplicationArtifacts` and `Publish` remain platform-owned result paths. MAUI only supplies shared metadata and does not change which artifacts those targets produce or return.
