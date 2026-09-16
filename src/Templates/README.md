
# MAUI Templates

For easy building and testing you can use the `build.ps1` script. This script is only for manual use and not part of any pipeline.

> [!NOTE]
> On macOS you find encounter and error like: `error NU5119: Warning As Error: File '/file/path/.DS_Store' was not added to the package. Files and folders starting with '.' or ending with '.nupkg' are excluded by default. To include this file, use -NoDefaultExcludes from the commandline` when this happens, run a `git clean -xfd` on the repository to remove all `.DS_Store` files from the filesystem.

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

Existing apps need both the manifest and delegate in each Apple app head. MAUI already forwards scene events to the cross-platform `Window` lifecycle events; do not add duplicate forwarding to `SceneDelegate`. Custom platform lifecycle handlers must use the corresponding scene callbacks for activation and backgrounding. Launch options are no longer passed to `FinishedLaunching`; read activation data from the scene connection options instead. See [Apple's migration guidance](https://developer.apple.com/documentation/technotes/tn3187-migrating-to-the-uikit-scene-based-life-cycle).

AppActions shortcut delivery also needs scene-aware handling in Core/Essentials. Adding the template manifest and delegate does not migrate the legacy shortcut callbacks.

## Mac Catalyst deployment target

Mac Catalyst app templates explicitly target `SupportedOSPlatformVersion` 17.0 (macOS 14), the minimum accepted by the .NET Mac Catalyst 27.x SDK. This SDK requirement is separate from scene adoption and applies when using that Apple SDK band with .NET 10 as well. The iOS app minimum remains 15.0.

The new app-template default is 17.0 even with older SDKs. Apps that need Mac Catalyst 15.0/16.0 must use a compatible 26.x SDK and explicitly lower their deployment target. The class-library template retains its 15.0 minimum; it does not create an app bundle.

## Generated-template regression tests

`AppleTemplateManifestTests` in `Microsoft.Maui.IntegrationTests` verifies the generated Apple manifests, registered delegates, platform exclusions, and Mac Catalyst app deployment targets. Each case installs the selected package into a fresh, isolated template hive.

CI selects `artifacts/Microsoft.Maui.Templates.net10.$MAUI_PACKAGE_VERSION.nupkg`. For a local package, set `MAUI_TEMPLATE_TEST_PACKAGE` to its full path and run the integration tests with `--filter FullyQualifiedName~AppleTemplateManifestTests`. These tests do not modify the global template cache.