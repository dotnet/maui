
# MAUI Templates

## MAUI app content

The `maui` template has a small counter app and an optional offline project/task sample:

```shell
dotnet new maui -n CounterApp
dotnet new maui -n CounterAppCSharp --ui csharp
dotnet new maui -n ProjectTasks --sample-content
```

Sample content uses XAML. Selecting C# UI with sample content creates the C# counter app and reports that sample content was not applied. Selecting Avalonia with sample content keeps the XAML sample and reports that Avalonia was not applied.

The counter app uses one .NET 11 bot PNG. The image's `BaseSize="190,185"` and `Resize="true"` metadata generate platform-density resources during the build. The page keeps the full composition, uses `AspectFit`, and limits its width. Do not replace this with runtime resizing or add the original image as a second raw asset. Recheck generated dimensions, resource selection, and display quality when changing the display size or supported scale range.

The sample retains Shell navigation, typed page models, generated observable properties and commands, and parameterized SQLite storage. New language syntax must preserve persistence and notification behavior. In particular, retain the fixed-schema `JsonDocument` seed reader; do not restore the earlier generated seed-materialization path.

The sample alone enables preview features for a XAML C# expression on the Dashboard Add action. Keep this opt-in out of the plain counter outputs. A conventional binding and inverse Boolean converter can replace the expression when preview features are not wanted.

Sample asset metadata includes an explicit Android monochrome layer. The existing foreground SVG is already a single-color alpha mask, so it is reused rather than duplicated.
The splash screen reuses its vector in light and dark appearances, with background and tint values from the sample palette. These sample settings do not change the counter app's icon or splash branding.

## Validation

Use the repository integration-test workflow and an isolated template hive for automated generation checks. Validate generated projects, not template files compiled outside the template engine. Include CLI/IDE sample selection, C# UI, combined-option warnings, and exclusions for sample-only files and assets.

Keep the normal Debug development path separate from release publishing. Check breakpoints, locals, async stepping, C# method-body Hot Reload, and XAML updates to resources and realized item templates. A successful C# update does not rerun an already completed page constructor.

Full trimming and Native AOT need actual publish and runtime checks of both app experiences. In the sample, check chart slices and labels, behaviors, SQLite native initialization, seed loading, and database reopen. A successful Debug build or a dependency's AOT compatibility flag is not sufficient. Use platform-specific supported publish settings; do not globally enable Native AOT and break managed Hot Reload.

For performance comparisons, use the same SDK/workload, hardware, runtime, configuration, and package mode. Record each resolved dependency graph and include intentional package updates in the complete candidate. Measure first frame separately from usable seeded content, and report clean, no-op, incremental build, restore, and publish times separately. Keep repeated raw samples and binlogs; do not infer performance from image file size.

## Manual build script

For easy building and testing you can use the `build.ps1` script. This script is only for manual use and not part of any pipeline.

> [!WARNING]
> This script removes manual template installations and clears the user's template-engine folder. Do not run it in a shared development environment without approval. Use isolated template generation for automated checks.

> [!NOTE]
> On macOS, an untracked `.DS_Store` file can cause NU5119 during packaging. Inspect and remove only the identified unwanted file; do not clean all untracked repository content.

## Functionality

The script:
* Deletes the `.tempTemplateOutput` folder which is used for the temporary files used by this script
* Builds the `src\Templates\src\Microsoft.Maui.Templates.csproj` project
* Packs the `src\Templates\src\Microsoft.Maui.Templates.csproj` project into a .nupkg file and outputs it to the `.tempTemplateOutput` directory, this directly is excluded from git
* Uninstalls any previous manual installations of .NET MAUI templates
* Empties the `~\templateengine` folder
* Finds and installs the resulting .nupkg artifact in the `.tempTemplateOutput` directory
* Creates a new .NET MAUI project based on the latest changes in the template
* Opens the new .NET MAUI project in Visual Studio (or on Mac in Visual Studio Code)

## Parameters

The script defines a coupe of parameters you can use. All have default values, so you only have to set them whenever you want to deviate from the default behavior.

The parameters are as follows:

* `projectType`: Specifies the type of .NET project to create (default is `maui`).
* `templateVersion`: Specifies the version number to use for the template pack build (default is `13.3.7`, needs to be a valid major, minor, patch version number, for example 1.2.3).
* `templatesProjectPath`: Specifies the path to the template project to build (default is `src\Microsoft.Maui.Templates.csproj`).
* `startVsAfterBuild`: Specifies whether to start Visual Studio (Code) after creating the new project with the latest template changes (default is `true`).

### Example usage with parameters

Find sample usages of the different parameters below, of course these can be mixed and matched as needed.

* Instead of a default .NET MAUI app, use the Blazor Hybrid template: `.\build.ps1 -projectType maui-blazor`
* Set a custom version number for the template: `.\build.ps1 -templateVersion 1.2.3`
* Build another template project: `.\build.ps1 -templatesProjectPath src\Microsoft.Maui.Templates-new.csproj`
* Don't start VS after creating the new project using the latest template changes: `.\build.ps1 -startVsAfterBuild $false`

## iOS and Mac Catalyst scene lifecycle

All app templates use UIKit's scene-based lifecycle on iOS and Mac Catalyst, as [required for apps built with the 27 SDKs](https://developer.apple.com/documentation/uikit/transitioning-to-the-uikit-scene-based-life-cycle). Each Apple app head includes a `UIApplicationSceneManifest` in `Info.plist` and a registered `SceneDelegate` derived from `MauiUISceneDelegate`.

Keep the configuration name `__MAUI_DEFAULT_SCENE_CONFIGURATION__` unchanged: it must match MAUI's framework configuration. The delegate's `[Register]` name must match `UISceneDelegateClassName` (`SceneDelegate` in these templates). MAUI creates the scene's window programmatically, so do not add `UISceneStoryboardFile` or change the existing `MauiSplashScreen` / `UILaunchStoryboardName` configuration.

`UIApplicationSupportsMultipleScenes` is set to `false` to preserve single-window behavior. To enable multiple windows, change this existing value to `true` rather than adding another manifest or `SceneDelegate` class. On Mac Catalyst, enabling multiple scenes also enables automatic window tabbing by default.

Existing apps need both the manifest and delegate in each Apple app head. MAUI already forwards scene events to the cross-platform `Window` lifecycle events; do not add duplicate forwarding to `SceneDelegate`. Custom platform lifecycle handlers must use the corresponding scene callbacks for activation and backgrounding. UIKit still calls `FinishedLaunching`, and MAUI forwards any application launch options UIKit supplies. Under the scene lifecycle, scene activation data such as shortcut items, URLs, and user activities comes through `UISceneConnectionOptions` in `WillConnect` and the corresponding scene callbacks, rather than the legacy application launch-options dictionary. See [Apple's migration guidance](https://developer.apple.com/documentation/technotes/tn3187-migrating-to-the-uikit-scene-based-life-cycle).

Core/Essentials forwards AppActions through the existing registrations for both application and scene lifecycles. Warm scene shortcuts use the native scene callback; a shortcut supplied when connecting a scene is delivered after that window's activation callbacks, at most once per connection. Do not duplicate this forwarding in template delegates or add another `Platform.PerformActionForShortcutItem` registration when using the default MAUI builder.

When migrating existing shortcut handlers, update every `PerformActionForShortcutItem` registration to invoke its completion callback exactly once: `true` when handled, or `false` when unhandled. Logging-only observers must also acknowledge `false`. A handler may acknowledge asynchronously after returning; returning alone is not an acknowledgement. If no handler reports `true`, an observer that never acknowledges prevents the native warm-action completion from finishing. There is no automatic timeout, because it would discard legitimate delayed responses. Cold scene connection options do not supply a native completion callback.

Essentials forwards warm scene URL contexts and user activities with a `WebPageUrl` through WebAuthenticator's existing callback handling. This preserves externally delivered callbacks for custom authenticators implementing `IPlatformWebAuthenticatorCallback`. The built-in modern `ASWebAuthenticationSession` flow still receives its result through the authentication session's own completion handler.

This authentication bridge is not general application deep-link navigation and does not replay URLs or user activities from a cold scene connection. Applications handling their own links must configure the corresponding scene handlers, including `SceneWillConnect` for connection options and `SceneContinueUserActivity` for continued universal-link activities. Those handlers are relevant even when multiple windows are disabled.

## Mac Catalyst deployment target

Mac Catalyst app templates explicitly target `SupportedOSPlatformVersion` 17.0 (macOS 14), the minimum accepted by the .NET Mac Catalyst 27.x SDK. This SDK requirement is separate from scene adoption and applies when using that Apple SDK band with .NET 10 as well. The iOS app minimum remains 15.0.

The new app-template default is 17.0 even with older SDKs. Apps that need Mac Catalyst 15.0/16.0 must use a compatible 26.x SDK and explicitly lower their deployment target. The class-library template retains its 15.0 minimum; it does not create an app bundle.

## Generated-template regression tests

`AppleTemplateManifestTests` in `Microsoft.Maui.IntegrationTests` verifies the generated Apple manifests, registered delegates, platform exclusions, and Mac Catalyst app deployment targets. Each case installs the selected package into a fresh, isolated template hive.

CI selects `artifacts/Microsoft.Maui.Templates.net10.$MAUI_PACKAGE_VERSION.nupkg`. For a local package, set `MAUI_TEMPLATE_TEST_PACKAGE` to its full path and run the integration tests with `--filter FullyQualifiedName~AppleTemplateManifestTests`. These tests do not modify the global template cache.