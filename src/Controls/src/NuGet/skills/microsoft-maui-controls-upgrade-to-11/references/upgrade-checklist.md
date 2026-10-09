# Version and validation checklist

## Before editing

- Record the app's SDK, target frameworks, direct package versions and workload
  versions. Check both project-local and centrally managed declarations.
- Establish a working source build and runtime baseline on each claimed platform.
  Keep source evidence separate from a target smoke build.
- Check the selected release's Android SDK/JDK, Apple Xcode and Windows toolchain
  requirements. Obtain approval before provisioning missing tools or changing
  machine-wide settings.
- Identify explicit `SupportedOSPlatformVersion` values and manifest/deployment
  minimums. Check the selected platform SDK's requirements; do not blindly carry
  an older minimum forward or lower one to silence a build error.

For example, inspect:

```shell
dotnet --info
dotnet workload list
dotnet msbuild MyApp.csproj -getProperty:TargetFramework,TargetFrameworks,MauiVersion,SupportedOSPlatformVersion
```

For multi-targeted apps, evaluate/build the individual framework being inspected.
Use the actual project's name and supported configuration, not these placeholders.

## Version-matched API evidence

Use the fresh `obj/project.assets.json` to determine what restore actually
selected. Preserve the app's package-management style and keep existing explicit
Controls/Core package declarations consistent with the requested release.
Do not infer that an installed workload guarantees that every directly referenced
NuGet package has the same version.

A package `.nuspec` can contain:

```xml
<repository type="git" url="https://github.com/dotnet/maui" commit="COMMIT" />
```

Inspect the corresponding public source at that commit, not necessarily `main`.
If the package omits a source revision, use its reference assemblies/XML
documentation and release-specific evidence instead of inventing a commit.

For a preview/RC, distinguish an intended final-release default from the behavior
of the actual selected package. An application may also explicitly register a
different handler or set feature switches. Inspect those overrides before
attributing native behavior to MAUI.

## Build and runtime

Restore after changing the SDK/framework/package declarations, then build the
selected frameworks in Debug and Release. Evaluate `TargetDir` in the same
framework/configuration if locating the generated application package:

```shell
dotnet restore MyApp.csproj
dotnet build MyApp.csproj -c Debug --no-restore
dotnet restore MyApp.csproj -p:Configuration=Release
dotnet build MyApp.csproj -c Release --no-restore
dotnet msbuild MyApp.csproj -p:Configuration=Debug -p:TargetFramework=FRAMEWORK -getProperty:TargetDir
```

The output directory can include an explicit platform version. Do not rename a
valid TFM just to match a hardcoded artifact path.

Exercise navigation, query values, binding and commands on a real runtime.
Test a second visit to a page and both toolbar/system back paths. Inspect native
implementation when a target-default migration is required. Report infrastructure
obstructions separately and never use a full target build to claim the source
baseline was runnable.

## Public documentation

- [.NET MAUI 11 changes](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11)
- [Installation and prerequisites](https://learn.microsoft.com/dotnet/maui/get-started/installation)
- [Supported platforms](https://learn.microsoft.com/dotnet/maui/supported-platforms)
- [Shell navigation](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/navigation)
- [Handler customization](https://learn.microsoft.com/dotnet/maui/user-interface/handlers/customize)

Check the documentation view/release and the exact package together. These links
can change as previews become final releases.
