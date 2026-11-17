# Mobile runtime project properties

## Applicability and evidence

[dotnet/docs#56477](https://github.com/dotnet/docs/issues/56477) tracks the .NET 11
breaking change "Mono project properties no longer configure mobile apps."
It is a documentation issue, not a published compatibility article or evidence
that every preview/RC implements the same behavior. Check its current status,
linked final documentation and the selected Android/Apple SDK before editing.
The historical Controls `11.0.0-rc.1.26451.6` evaluation predates this guidance;
its successful builds do not validate these runtime changes.

When a mobile target moves from Mono to CoreCLR, Mono-specific settings no longer
provide their previous behavior. Depending on the setting and SDK, they can have
no effect or produce a warning/error. Do not treat a warning-free build as proof
that an old AOT, interpreter, GC or LLVM tuning option still works.

## Inventory the existing configuration

Search project files and imported `Directory.Build.props`/`Directory.Build.targets`,
including platform, target-framework and Debug/Release conditions. Inspect
evaluated properties for each claimed framework/configuration: removing a local
declaration does not remove an inherited value.

The issue identifies these Android Mono settings:

- `RunAOTCompilation`, `AotAssemblies`, `EnableLLVM`
- `AndroidEnableProfiledAot`, `AndroidAotMode`, `AndroidAotAdditionalArguments`,
  `AndroidExtraAotOptions`, `AndroidAotEnableLazyLoad`
- `AndroidUseInterpreter`, `UseInterpreter`
- The `@(AndroidAotProfile)` item

The Apple inventory covers iOS and Mac Catalyst:

- `UseInterpreter`, `MtouchInterpreter`
- `EnableSGenConc`, `MtouchEnableSGenConc`
- `MtouchUseLlvm`, `MtouchFloat32`, `MonoUseCompressedInterfaceBitmap`
- `RunAotCompilation`, `AotAssemblies`, `AotCompileMaxDegreeOfParallelism`

MSBuild property names are case-insensitive; the different
`RunAOTCompilation`/`RunAotCompilation` spelling is not a different setting.

## Preserve older targets without carrying Mono tuning forward

Remove obsolete settings from CoreCLR targets. If the application still targets
.NET 10 and needs its existing Mono configuration, keep those settings limited
to the .NET 10 mobile inner build rather than deleting them indiscriminately.
For example, **in the project file after its target frameworks are declared**:

```xml
<PropertyGroup Condition="$([MSBuild]::GetTargetFrameworkVersion('$(TargetFramework)')) == '10.0' and ($([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android' or $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios' or $([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst')">
  <UseInterpreter>true</UseInterpreter>
</PropertyGroup>
```

This illustrates scoping an **existing** setting, not a recommendation to enable
the interpreter. Retain the app's original configuration/platform conditions as
well. Apply equivalent scoping to existing items such as `AndroidAotProfile`.
Do not rely on `TargetFramework` already being populated in an early imported
props file; verify the evaluated inner builds, including explicit platform-version
TFMs. Do not invent one-to-one CoreCLR replacements for Mono tuning properties.

## Marshal methods and ReadyToRun

`AndroidEnableMarshalMethods` is not Mono-specific. However, the tracked
CoreCLR ReadyToRun combination is unsupported and causes `XA1049`. Do not
enable it in a CoreCLR ReadyToRun build. Remove or condition an existing
declaration instead of suppressing the error or switching runtimes just to get
a green build. Do not infer a blanket ban for every Android runtime/configuration.

## Validate behavior, not just property cleanup

Build Debug and Release and exercise each claimed platform after the change.
Retest startup and navigation, reflection/dynamic-code-dependent paths, and any
workflows that previously required interpreter or AOT exceptions. Measure startup,
memory and relevant performance workloads against the source baseline on comparable
devices/configurations; removing Mono tuning does not guarantee equivalent
performance. Report any unverified platform or runtime behavior explicitly.
