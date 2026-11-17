# UIKit scene lifecycle for Apple SDK 27

## Scope and prerequisites

Apps built against iOS/Mac Catalyst SDK 27 with Xcode 27 must adopt the UIKit
scene lifecycle before running on iOS/Mac Catalyst 27. This is an Apple SDK
requirement, not exclusively a MAUI 11 change or a new-device layout feature.
An existing binary built with an older SDK does not suddenly need rebuilding
because the device OS updates. A missing scene lifecycle can cause a launch
crash with `UIScene life cycle is required for apps built with this SDK.`;
inspect logs rather than attributing every post-splash crash to scenes.

Use the [Microsoft Learn migration guide](https://learn.microsoft.com/dotnet/maui/ios/uikit-scene-lifecycle)
and the selected workload's release notes. The
[Xcode 27 migration article](https://devblogs.microsoft.com/dotnet/migrate-dotnet-maui-apps-uikit-scene-lifecycle/)
explains the manual workflow and optional agent tooling. This reference reflects
that public guidance as reviewed on 2026-10-28, not a guarantee about future packs.

Inventory actual Apple targets, SDK/workload and MAUI versions, Xcode selection,
CI pins, imported version/minimum declarations, custom delegate base classes,
`ConfigureLifecycleEvents` registrations and every configuration-specific
`Info.plist`. Skip platforms not targeted. Package/workload updates do not
rewrite existing platform files; new-template defaults do not prove an existing
app adopted scenes.

For a .NET 10 baseline, use MAUI 10.0.110 or a later compatible servicing release
for scene-side App Actions and relevant WebAuthenticator forwarding. Preserve a
newer compatible pin and edit the actual central version owner. For .NET 11,
verify `MauiUISceneDelegate` and required forwarding APIs in the app-resolved
package and select a compatible .NET 11 Apple workload; do not pin MAUI 10
packages into a .NET 11 app or extrapolate RC1 behavior to later releases.
Match Xcode to the selected workload's supported version; do not disable its
version checks or assume 27.1 support from 27.0 support.

SDK 27 requires deployment minima of iOS 15.0 and Mac Catalyst 17.0 (macOS 14).
Preserve higher minima. If existing support extends below those floors, obtain
an explicit support-policy decision before raising them; retaining a compatible
older SDK may be necessary. Raising `SupportedOSPlatformVersion` alone does not
adopt scenes. Keep platform/configuration conditions and plist declarations
consistent without changing unrelated targets.

## Add or reconcile scene configuration

For a previously unconfigured, single-window app, add this public delegate in
`Platforms/iOS/SceneDelegate.cs`, using the application's namespace. Add the
equivalent under `Platforms/MacCatalyst` only when that target exists:

```csharp
using Foundation;
using Microsoft.Maui;

namespace MyApp;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
```

Keep the existing `AppDelegate : MauiUIApplicationDelegate` and entry point.
Merge the following into the root dictionary of each applicable shipping
`Info.plist`, not as a second manifest key:

```xml
<key>UIApplicationSceneManifest</key>
<dict>
  <key>UIApplicationSupportsMultipleScenes</key>
  <false/>
  <key>UISceneConfigurations</key>
  <dict>
    <key>UIWindowSceneSessionRoleApplication</key>
    <array>
      <dict>
        <key>UISceneConfigurationName</key>
        <string>__MAUI_DEFAULT_SCENE_CONFIGURATION__</string>
        <key>UISceneDelegateClassName</key>
        <string>SceneDelegate</string>
      </dict>
    </array>
  </dict>
</dict>
```

This is a new-head example, not a replacement for existing custom configuration.
Match `UISceneDelegateClassName` to the Objective-C `[Register]` name, not merely
the CLR class name. Preserve custom registrations, multiwindow intent, session
roles and existing scene behavior. The default window-creation path expects
`__MAUI_DEFAULT_SCENE_CONFIGURATION__`; review other names with custom
`GetConfiguration`/`WillConnect` window creation. Preserve splash/launch-storyboard
settings; do not add `UISceneStoryboardFile` or duplicate MAUI window creation.
Dynamic configuration still needs at least a `UIApplicationSceneManifest`
dictionary for the standard MAUI scene path.

## Preserve custom behavior

Inventory both AppDelegate overrides and lifecycle registrations, including
inherited custom implementations. Trace every original side effect to its new
delivery path; a manifest and an empty delegate do not complete a custom migration.

| Application callback | Scene registration |
| --- | --- |
| `OnActivated` | `SceneOnActivated` |
| `OnResignActivation` | `SceneOnResignActivation` |
| `DidEnterBackground` | `SceneDidEnterBackground` |
| `WillEnterForeground` | `SceneWillEnterForeground` |
| `OpenUrl` | `SceneOpenUrl` |
| `ContinueUserActivity` | `SceneContinueUserActivity` |

Arguments differ: move the actual behavior, not just the callback name. Preserve
existing scene behavior while adding former application-level work. Preserve
handled results and call base implementations unconditionally in scene overrides
so framework window setup, lifecycle and authentication forwarding still run.
Remove obsolete registrations once moved; prevent duplicate delivery.

`FinishedLaunching` still runs, but `AppDelegate.Window` remains null in the
standard scene path, including after its base call. Move window-dependent
initialization to the owning scene when its window is ready.

Warm URL/activity handlers do not cover cold launch. Inspect
`SceneWillConnect` connection options: retain **all** `UrlContexts` and
`UserActivities`, guard absent collections, and defer navigation until the
owning scene's window exists. Do not retain only the first payload or assume
base forwarding replays application-specific cold links. Audit
`NotificationResponse` separately for notification-tap navigation; ordinary push
registration/remote-notification handling remains application-level.
WebAuthenticator forwarding is not a general deep-link router.

Move custom `AppDelegate.PerformActionForShortcutItem` logic to the lifecycle
registration. Every custom registration, including logging-only observers,
must complete its own callback exactly once: true when handled, false otherwise,
after an asynchronous decision when applicable. Retain all original observers.
Do not add duplicate `Platform.PerformActionForShortcutItem` forwarding; the
compatible default builder already forwards Essentials.

## Verify the delivered Apple app

Check source plist structure, then restore/build Debug and Release for each
affected framework with its supported workload/Xcode pairing. Inspect the
**built app bundle's** merged `Info.plist` for the intended manifest and delegate
binding; an incremental build can retain stale output. On macOS, use
`plutil -lint -- <plist>` for structure and `plutil -p <bundle>/Info.plist` for
the actual bundle. Confirm the corresponding delegate is included in that target.

Workloads containing the scene-manifest check report `MT7199` when the final
manifest lacks a dictionary. Availability depends on the workload. An empty
dictionary passes that heuristic but does not prove correct window/delegate
wiring. Do not recommend `ValidateSceneManifest=false` as a migration fix:
it disables only the build check, not the OS requirement. Earlier drafts used
`MT7198`; that now identifies a different signing error.

With approved device/simulator access, exercise Debug and Release on iOS 27
and separately on Mac Catalyst 27 when targeted, plus the oldest supported OS.
Check cold/warm launch, background/foreground, cold/warm deep and universal
links, authentication return, notification taps, handled/unhandled App Actions,
and custom multiwindow behavior when present. Inspect launch logs for the
scene-lifecycle fault. Compilation or a launch on iOS 26 alone is not proof.
Report missing toolchains or unexercised behaviors rather than claiming success.
The historical Android RC1 evaluations do not validate this Apple migration.

> **Optional agent assistance:** The experimental
> [maui-xcode27-migration skill](https://github.com/dotnet/maui-labs/tree/main/plugins/dotnet-maui/skills/maui-xcode27-migration)
> offers a deeper audit and custom-lifecycle workflow. Follow the
> [MAUI CLI documentation](https://learn.microsoft.com/dotnet/maui/developer-tools/cli/)
> for current installation instructions and obtain permission before installing
> tooling. Installation is separate from applying or validating a migration.
> This NuGet package supplies text guidance only; it does not install that skill
> or include its executable audit helper.
