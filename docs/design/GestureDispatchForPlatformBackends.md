# Gesture dispatch for external platform backends

.NET MAUI ships gesture *recognizers* (`TapGestureRecognizer`, `PointerGestureRecognizer`,
`LongPressGestureRecognizer`, …) in `Microsoft.Maui.Controls`, and gesture *detection* in the
per-platform gesture managers under `src/Controls/src/Core/Platform`.

Out-of-tree platform backends — for example [`Maui.Tizen`](https://github.com/Redth/Maui.Tizen) —
implement their own detection layer and need to raise the recognizer's events. To make that
possible, each recognizer exposes an infrastructure dispatch surface: `public` methods annotated
with `[EditorBrowsable(EditorBrowsableState.Never)]` so they are reachable from any assembly but
stay out of IntelliSense for app authors.

| Recognizer | Dispatch API |
| ---------- | ------------ |
| `TapGestureRecognizer` | `SendTapped(View sender, Func<IElement?, Point?>? getPosition = null)` |
| `PointerGestureRecognizer` | `SendPointerEntered` / `SendPointerExited` / `SendPointerMoved` / `SendPointerPressed` / `SendPointerReleased` |
| `LongPressGestureRecognizer` | `SendLongPressing(View sender, GestureStatus status, Func<IElement?, Point?>? getPosition = null)` and `SendLongPressed(View sender, Func<IElement?, Point?>? getPosition = null)` |
| `DragGestureRecognizer` | `SendDragStarting` / `SendDropCompleted` |
| `DropGestureRecognizer` | `SendDragLeave` / `SendDrop` |

## Rules that apply to every dispatch method

- **`sender` must not be `null`.** Each method throws `ArgumentNullException`; pass the `View` the
  gesture was detected on.
- **Call on the UI thread.** Dispatch runs user event handlers and commands synchronously, and it
  writes bindable properties. Marshal to the dispatcher before calling.
- **`getPosition` is optional.** Pass `null` when the platform cannot supply a location. Otherwise
  supply a callback that returns the gesture location *relative to the element passed in*, and the
  location relative to `sender` when the argument is `null` or is `sender` itself. The callback is
  invoked lazily by the event args, so keep it cheap and avoid capturing strong references to
  platform views that may already be disposed.
- **Only dispatch to recognizers attached to the view.** Enumerate
  `view.GestureRecognizers.OfType<TRecognizer>()` and honor recognizer configuration such as
  `NumberOfTouchesRequired`.

## `LongPressGestureRecognizer` contract

`LongPressGestureRecognizer` has two channels:

- `SendLongPressing` sets `LongPressGestureRecognizer.State` and raises `LongPressing` with a
  `GestureStatus`. It is the state machine.
- `SendLongPressed` executes `Command` (when `Command.CanExecute(CommandParameter)` is `true`) and
  raises `LongPressed`. It is the "the long press succeeded" notification and carries
  `CommandParameter` on the event args.

Backends should follow the same ordering the in-box platforms use:

| Platform signal | Calls |
| --------------- | ----- |
| Press recognized | `SendLongPressing(view, GestureStatus.Started, getPosition)` |
| Press held / moved within `AllowableMovement` | `SendLongPressing(view, GestureStatus.Running, getPosition)` |
| Press released after the minimum duration | `SendLongPressed(view, getPosition)` **then** `SendLongPressing(view, GestureStatus.Completed, getPosition)` |
| Press cancelled or failed | `SendLongPressing(view, GestureStatus.Canceled, getPosition)` |

Notes:

- `SendLongPressed` is raised **before** `Completed` so handlers observing `LongPressed` see the
  gesture result before the terminal state change.
- A cancelled gesture must **not** raise `LongPressed` and must **not** execute `Command`.
- `Started`/`Running` are optional for platforms that only surface a single "long press happened"
  callback. Android and Windows go straight to `SendLongPressed` + `Completed`.
- `MinimumPressDuration`, `NumberOfTouchesRequired`, and `AllowableMovement` are *inputs* for the
  backend's detector. MAUI does not enforce them; the backend decides which of them the platform can
  honor and documents the gaps.

### Example backend

```csharp
using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public sealed class MyPlatformLongPressHandler
{
    readonly View _view;

    public MyPlatformLongPressHandler(View view) => _view = view;

    Func<IElement?, Point?> GetPosition(Point origin) =>
        relativeTo => relativeTo is null || ReferenceEquals(relativeTo, _view)
            ? origin
            : TranslateToElement(origin, relativeTo);

    void OnPlatformLongPress(PlatformLongPressArgs e)
    {
        var position = GetPosition(new Point(e.X, e.Y));

        foreach (var recognizer in _view.GestureRecognizers
                     .OfType<LongPressGestureRecognizer>()
                     .Where(r => r.NumberOfTouchesRequired == e.TouchCount))
        {
            switch (e.State)
            {
                case PlatformGestureState.Began:
                    recognizer.SendLongPressing(_view, GestureStatus.Started, position);
                    break;
                case PlatformGestureState.Changed:
                    recognizer.SendLongPressing(_view, GestureStatus.Running, position);
                    break;
                case PlatformGestureState.Ended:
                    recognizer.SendLongPressed(_view, position);
                    recognizer.SendLongPressing(_view, GestureStatus.Completed, position);
                    break;
                case PlatformGestureState.Cancelled:
                case PlatformGestureState.Failed:
                    recognizer.SendLongPressing(_view, GestureStatus.Canceled, position);
                    break;
            }
        }
    }
}
```

## Keeping the contract honest

`src/Controls/tests/ExternalGestureBackend` is a test-support assembly that is deliberately **not**
listed in `Microsoft.Maui.Controls`'s `InternalsVisibleTo` set. It contains a fake backend that
drives `LongPressGestureRecognizer` purely through the public dispatch API, so demoting any of these
methods back to `internal` breaks the build. `LongPressGestureRecognizerExternalBackendTests` in
`Controls.Core.UnitTests` exercises that fake backend across the full
`Started` → `Running` → `Completed` / `Canceled` lifecycle.

Add the same coverage whenever a new recognizer gains a dispatch API.
