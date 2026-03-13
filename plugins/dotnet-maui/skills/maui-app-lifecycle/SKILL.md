---
name: maui-app-lifecycle
description: >
  .NET MAUI app lifecycle guidance covering the four app states (not running,
  running, deactivated, stopped), cross-platform Window lifecycle events,
  backgrounding and resume behaviour, platform-specific lifecycle mapping
  for Android and iOS/Mac Catalyst, and state-preservation patterns.
  USE FOR: "app lifecycle", "OnStart", "OnSleep", "OnResume", "backgrounding",
  "app state", "window lifecycle", "save state on background", "resume app",
  "deactivated event", "lifecycle events".
  DO NOT USE FOR: navigation events (use maui-shell-navigation),
  dependency injection setup (use maui-dependency-injection), or platform APIs (use maui-platform-invoke).
---

# .NET MAUI App Lifecycle

## App states

A MAUI app moves through four logical states:

| State | Meaning |
|---|---|
| **Not Running** | App process does not exist. |
| **Running** | App is in the foreground and receiving input. |
| **Deactivated** | App is visible but lost focus (e.g. a dialog or split-screen). |
| **Stopped** | App is fully backgrounded; UI is not visible. |

Typical flow: Not Running → Running → Deactivated → Stopped → Running (resumed) or Not Running (terminated).

## Cross-platform Window events

`Microsoft.Maui.Controls.Window` exposes six lifecycle events:

| Event | When it fires |
|---|---|
| `Created` | Window has been created (native window allocated). |
| `Activated` | Window has been activated and is receiving input. |
| `Deactivated` | Window lost focus but may still be visible. |
| `Stopped` | Window is no longer visible (backgrounded). |
| `Resumed` | Window returns to the foreground after being stopped. |
| `Destroying` | Window is being torn down (native window deallocated). |

> **Important:** `Resumed` never fires on the app's first launch. It only fires when returning from the `Stopped` state.

## Subscribing to Window events

### Option A – Override `CreateWindow` in `App`

```csharp
public partial class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        window.Created += (s, e) => Log("Window Created");
        window.Activated += (s, e) => Log("Window Activated");
        window.Deactivated += (s, e) => Log("Window Deactivated");
        window.Stopped += (s, e) => Log("Window Stopped");
        window.Resumed += (s, e) => Log("Window Resumed");
        window.Destroying += (s, e) => Log("Window Destroying");

        return window;
    }
}
```

### Option B – Custom Window subclass with overrides

```csharp
public class AppWindow : Window
{
    public AppWindow() : base() { }
    public AppWindow(Page page) : base(page) { }

    protected override void OnCreated() { /* init work */ }
    protected override void OnActivated() { /* refresh UI */ }
    protected override void OnDeactivated() { /* pause timers */ }
    protected override void OnStopped() { /* save state */ }
    protected override void OnResumed() { /* restore state */ }
    protected override void OnDestroying() { /* cleanup */ }
}
```

Return it from `CreateWindow`:

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    return new AppWindow(new AppShell());
}
```

## Platform lifecycle event mapping

### Android

| Window event | Android Activity callback |
|---|---|
| Created | `OnCreate` |
| Activated | `OnResume` |
| Deactivated | `OnPause` |
| Stopped | `OnStop` |
| Resumed | `OnRestart` → `OnStart` → `OnResume` |
| Destroying | `OnDestroy` |

### iOS / Mac Catalyst

| Window event | UIKit callback |
|---|---|
| Created | `WillFinishLaunching` / `SceneWillConnect` |
| Activated | `DidBecomeActive` |
| Deactivated | `WillResignActive` |
| Stopped | `DidEnterBackground` |
| Resumed | `WillEnterForeground` |
| Destroying | `WillTerminate` |

### Windows (WinUI)

| Window event | WinUI callback |
|---|---|
| Created | `OnLaunched` |
| Activated | `Activated` (foreground) |
| Deactivated | `Activated` (background) |
| Stopped | `VisibilityChanged` (false) |
| Resumed | `VisibilityChanged` (true) |
| Destroying | `Closed` |

## Platform-specific lifecycle events

Use `ConfigureLifecycleEvents` in `MauiProgram.cs` to hook directly into native callbacks:

```csharp
builder.ConfigureLifecycleEvents(events =>
{
#if ANDROID
    events.AddAndroid(android => android
        .OnCreate((activity, bundle) => Log("Android OnCreate"))
        .OnStart(activity => Log("Android OnStart"))
        .OnResume(activity => Log("Android OnResume"))
        .OnPause(activity => Log("Android OnPause"))
        .OnStop(activity => Log("Android OnStop"))
        .OnDestroy(activity => Log("Android OnDestroy")));
#elif IOS || MACCATALYST
    events.AddiOS(ios => ios
        .WillFinishLaunching((app, options) => { Log("iOS WillFinishLaunching"); return true; })
        .SceneWillConnect((scene, session, options) => Log("iOS SceneWillConnect"))
        .DidBecomeActive(app => Log("iOS DidBecomeActive"))
        .WillResignActive(app => Log("iOS WillResignActive"))
        .DidEnterBackground(app => Log("iOS DidEnterBackground"))
        .WillTerminate(app => Log("iOS WillTerminate")));
#elif WINDOWS
    events.AddWindows(windows => windows
        .OnLaunched((app, args) => Log("Windows OnLaunched"))
        .OnActivated((window, args) => Log("Windows Activated"))
        .OnClosed((window, args) => Log("Windows Closed")));
#endif
});
```

## State preservation pattern

Save and restore transient state during backgrounding:

```csharp
protected override void OnStopped()
{
    base.OnStopped();
    Preferences.Set("draft_text", _viewModel.DraftText);
    Preferences.Set("scroll_position", _viewModel.ScrollY);
}

protected override void OnResumed()
{
    base.OnResumed();
    _viewModel.DraftText = Preferences.Get("draft_text", string.Empty);
    _viewModel.ScrollY = Preferences.Get("scroll_position", 0.0);
}
```

For larger state, use `SecureStorage` or file-based serialization instead of `Preferences`.

## Key behavioural notes

- **Resumed ≠ first launch.** `Resumed` only fires when returning from `Stopped`. On first launch the sequence is `Created` → `Activated`.
- **Deactivated ≠ Stopped.** A dialog or split-screen triggers `Deactivated` without `Stopped`.
- **Android back button** may call `Destroying` without `Stopped` if the activity finishes.
- **iOS scene lifecycle** is used on iOS 13+; older delegate methods are still forwarded.
- **Multiple windows** (iPad, Mac Catalyst, desktop Windows): each `Window` instance fires its own events independently.
- Keep lifecycle handlers fast. Long-running work should use `Task.Run` or background services to avoid ANR/watchdog kills.
