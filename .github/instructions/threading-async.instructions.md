---
applyTo:
  - "**/*.Android.cs"
  - "**/*.android.cs"
  - "**/*.iOS.cs"
  - "**/*.ios.cs"
  - "**/*.Windows.cs"
  - "**/*.windows.cs"
  - "**/Platform/**"
  - "**/Platforms/**"
---
# Threading and Async Patterns

## UI Thread Dispatch
- Platform view modifications MUST happen on the UI thread
- **Android**: `platformView.Post(() => { })` or `Looper.MainLooper` for thread checks
- **iOS/MacCatalyst**: `MainThread.BeginInvokeOnMainThread()` or `DispatchQueue.MainQueue`
- **Windows**: `DispatcherQueue.TryEnqueue()` — be aware of COM threading apartment model

## Async Handler Operations
- Use `.FireAndForget(handler)` for async work in mapper methods — never bare `async void`
- After `await`, verify the handler is still connected (`VirtualView != null`) before applying results
- Thread `CancellationToken` through long-running operations and check at appropriate yield points

## Race Condition Prevention
- Concurrent access to shared state must use `lock`, `Interlocked`, or immutable patterns
- Image loading and other async pipelines must cancel in-flight operations when the source changes
- Guard against stale callbacks — platform callbacks may fire after `DisconnectHandler`

## Platform-Specific Patterns
- **Android**: API-level checks use `Build.VERSION.SdkInt` comparison, not version string parsing
- **iOS**: Use `System.OperatingSystem.IsIOSVersionAtLeast()` for linker-friendly runtime checks
- **Windows**: WinUI version checks may be needed for features in specific Windows App SDK versions
- Use `System.OperatingSystem` APIs over `RuntimeInformation` — they are trimmer/AOT-friendly
