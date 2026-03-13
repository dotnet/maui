---
name: maui-gestures
description: >
  Guidance for implementing tap, swipe, pan, pinch, drag-and-drop, and pointer
  gesture recognizers in .NET MAUI applications. Covers XAML and C# usage,
  combining gestures, and platform differences.
  USE FOR: "tap gesture", "swipe gesture", "pan gesture", "pinch gesture",
  "drag and drop", "pointer gesture", "gesture recognizer", "TapGestureRecognizer",
  "SwipeGestureRecognizer", "PanGestureRecognizer", "combine gestures".
  DO NOT USE FOR: animations triggered by gestures (use maui-animations),
  CollectionView swipe actions (use maui-collectionview), or custom drawing interaction (use maui-graphics-drawing).
---

# .NET MAUI Gesture Recognizers

All gesture recognizers inherit from `GestureRecognizer` and are added via `View.GestureRecognizers`.

```xml
<Image>
  <Image.GestureRecognizers>
    <TapGestureRecognizer Tapped="OnTapped" />
  </Image.GestureRecognizers>
</Image>
```

```csharp
var tap = new TapGestureRecognizer();
tap.Tapped += OnTapped;
image.GestureRecognizers.Add(tap);
```

---

## Summary

| Recognizer | Key Properties | Key Events / Commands | Notes |
|---|---|---|---|
| `TapGestureRecognizer` | `NumberOfTapsRequired`, `Buttons` | `Tapped`, `Command` | Default 1 tap, primary button |
| `SwipeGestureRecognizer` | `Direction`, `Threshold` | `Swiped`, `Command` | Threshold default 100 DIU |
| `PanGestureRecognizer` | `TouchPoints` | `PanUpdated` | StatusType: Started/Running/Completed |
| `PinchGestureRecognizer` | — | `PinchUpdated` | Scale, ScaleOrigin, Status |
| `DragGestureRecognizer` | `CanDrag` | `DragStarting`, `DropCompleted` | Auto data for Text/Image controls |
| `DropGestureRecognizer` | `AllowDrop` | `DragOver`, `Drop` | Platform-specific `PlatformArgs` |
| `PointerGestureRecognizer` | — | `PointerEntered/Exited/Moved/Pressed/Released` | Matching commands; enables `PointerOver` visual state |

---

## TapGestureRecognizer

| Property | Type | Default | Description |
|---|---|---|---|
| `NumberOfTapsRequired` | `int` | `1` | Taps needed to fire |
| `Buttons` | `ButtonsMask` | `Primary` | `Primary`, `Secondary`, or both |
| `Command` | `ICommand` | — | Fires on tap |
| `CommandParameter` | `object` | — | Passed to `Command` |

```xml
<Label Text="Tap me">
  <Label.GestureRecognizers>
    <TapGestureRecognizer NumberOfTapsRequired="2" Buttons="Primary"
                          Command="{Binding DoubleTapCommand}" />
  </Label.GestureRecognizers>
</Label>
```

```csharp
var tap = new TapGestureRecognizer { NumberOfTapsRequired = 2, Buttons = ButtonsMask.Primary };
tap.Command = new Command(() => Debug.WriteLine("Double-tapped"));
label.GestureRecognizers.Add(tap);
```

> In .NET 10 `ClickGestureRecognizer` is deprecated. Use `TapGestureRecognizer`
> (touch/stylus) and `PointerGestureRecognizer` (mouse hover/press) instead.

---

## SwipeGestureRecognizer

| Property | Type | Default | Description |
|---|---|---|---|
| `Direction` | `SwipeDirection` | — | `Left`, `Right`, `Up`, `Down` |
| `Threshold` | `uint` | `100` | Minimum distance in DIU |
| `Command` | `ICommand` | — | Fires on swipe |

`SwipedEventArgs`: `Direction`, `Parameter`.

```xml
<BoxView Color="Teal">
  <BoxView.GestureRecognizers>
    <SwipeGestureRecognizer Direction="Left" Threshold="150" Swiped="OnSwiped" />
    <SwipeGestureRecognizer Direction="Right" Swiped="OnSwiped" />
  </BoxView.GestureRecognizers>
</BoxView>
```

```csharp
var swipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left, Threshold = 150 };
swipe.Swiped += (s, e) => Debug.WriteLine($"Swiped {e.Direction}");
boxView.GestureRecognizers.Add(swipe);
```

> Add one recognizer per direction; a single recognizer handles only one.

---

## PanGestureRecognizer

| Property | Type | Default | Description |
|---|---|---|---|
| `TouchPoints` | `int` | `1` | Number of fingers required |

`PanUpdatedEventArgs`: `StatusType` (`Started`, `Running`, `Completed`), `TotalX`, `TotalY`, `GestureId`.

```xml
<Image Source="photo.png">
  <Image.GestureRecognizers>
    <PanGestureRecognizer PanUpdated="OnPanUpdated" />
  </Image.GestureRecognizers>
</Image>
```

```csharp
var pan = new PanGestureRecognizer();
pan.PanUpdated += (s, e) => {
    if (e.StatusType == GestureStatus.Running)
    { image.TranslationX = e.TotalX; image.TranslationY = e.TotalY; }
};
image.GestureRecognizers.Add(pan);
```

> On iOS `TotalX`/`TotalY` are relative to the start; on Android they are
> relative to the previous event. Normalize in your handler if needed.

---

## PinchGestureRecognizer

`PinchGestureUpdatedEventArgs`: `Scale`, `ScaleOrigin` (`Point`), `Status` (`Started`, `Running`, `Completed`).

```xml
<Image Source="photo.png">
  <Image.GestureRecognizers>
    <PinchGestureRecognizer PinchUpdated="OnPinchUpdated" />
  </Image.GestureRecognizers>
</Image>
```

```csharp
var pinch = new PinchGestureRecognizer();
pinch.PinchUpdated += (s, e) =>
{
    if (e.Status == GestureStatus.Running)
        image.Scale = Math.Clamp(image.Scale + (e.Scale - 1), 0.5, 3);
};
image.GestureRecognizers.Add(pinch);
```

---

## DragGestureRecognizer

| Property | Type | Default | Description |
|---|---|---|---|
| `CanDrag` | `bool` | `true` | Enables/disables dragging |

| Event | Args Type | Key Properties |
|---|---|---|
| `DragStarting` | `DragStartingEventArgs` | `Data` (DataPackage), `Cancel` |
| `DropCompleted` | `DropCompletedEventArgs` | `DragDropResult` |

`Label` and `Image` auto-populate data packages. For custom data, set `DragStartingEventArgs.Data`.

```xml
<Label Text="Drag me" BackgroundColor="LightBlue">
  <Label.GestureRecognizers>
    <DragGestureRecognizer CanDrag="True" DragStarting="OnDragStarting" />
  </Label.GestureRecognizers>
</Label>
```

```csharp
var drag = new DragGestureRecognizer { CanDrag = true };
drag.DragStarting += (s, e) =>
{
    e.Data.Text = viewModel.ItemId;
    e.Data.Properties["payload"] = viewModel.SelectedItem;
};
view.GestureRecognizers.Add(drag);
```

---

## DropGestureRecognizer

| Property | Type | Default | Description |
|---|---|---|---|
| `AllowDrop` | `bool` | `false` | Must be `true` to receive drops |

| Event | Args Type | Key Properties |
|---|---|---|
| `DragOver` | `DragEventArgs` | `AcceptedOperation`, `PlatformArgs` |
| `Drop` | `DropEventArgs` | `Data` (DataPackageView), `PlatformArgs` |

```xml
<StackLayout BackgroundColor="LightGray">
  <StackLayout.GestureRecognizers>
    <DropGestureRecognizer AllowDrop="True" DragOver="OnDragOver" Drop="OnDrop" />
  </StackLayout.GestureRecognizers>
</StackLayout>
```

```csharp
var drop = new DropGestureRecognizer { AllowDrop = true };
drop.Drop += async (s, e) => { var text = await e.Data.GetTextAsync(); };
target.GestureRecognizers.Add(drop);
```

**PlatformArgs** per platform:

| Platform | DragOver | Drop |
|---|---|---|
| Android | `PlatformArgs.DragEvent` | `PlatformArgs.DragEvent` |
| iOS / Mac Catalyst | `UIDropInteraction` args | `UIDropInteraction` args |
| Windows | WinUI `DragEventArgs` | WinUI `DragEventArgs` |

---

## PointerGestureRecognizer

| Event | Command Property | Fires When |
|---|---|---|
| `PointerEntered` | `PointerEnteredCommand` | Pointer enters view bounds |
| `PointerExited` | `PointerExitedCommand` | Pointer leaves view bounds |
| `PointerMoved` | `PointerMovedCommand` | Pointer moves within view |
| `PointerPressed` | `PointerPressedCommand` | Button pressed in view |
| `PointerReleased` | `PointerReleasedCommand` | Button released in view |

`PointerEventArgs.GetPosition(relativeTo)` returns a `Point?`. Adding this recognizer enables the **PointerOver** `VisualState`.

```xml
<Border StrokeShape="RoundRectangle 8">
  <Border.GestureRecognizers>
    <PointerGestureRecognizer PointerEnteredCommand="{Binding HoverInCommand}"
                              PointerExitedCommand="{Binding HoverOutCommand}"
                              PointerMoved="OnPointerMoved" />
  </Border.GestureRecognizers>
  <VisualStateManager.VisualStateGroups>
    <VisualStateGroup Name="CommonStates">
      <VisualState Name="PointerOver">
        <VisualState.Setters>
          <Setter Property="BackgroundColor" Value="LightCyan" />
        </VisualState.Setters>
      </VisualState>
    </VisualStateGroup>
  </VisualStateManager.VisualStateGroups>
</Border>
```

```csharp
var pointer = new PointerGestureRecognizer();
pointer.PointerMoved += (s, e) => Debug.WriteLine($"Pointer at {e.GetPosition(null)}");
border.GestureRecognizers.Add(pointer);
```

> Best for mouse/stylus. On touch devices, pointer events fire but hover is not tracked.

---

## Combining Multiple Gestures

Add multiple recognizers to the same collection. The platform resolves conflicts.

```xml
<Image Source="card.png">
  <Image.GestureRecognizers>
    <TapGestureRecognizer Tapped="OnTapped" />
    <PanGestureRecognizer PanUpdated="OnPan" />
    <PinchGestureRecognizer PinchUpdated="OnPinch" />
    <PointerGestureRecognizer PointerEntered="OnHover" />
  </Image.GestureRecognizers>
</Image>
```

> Combining pan + swipe on the same view can conflict on Android. Test or use one at a time.

---

## Platform Differences

| Area | iOS / Mac Catalyst | Android | Windows |
|---|---|---|---|
| Pan TotalX/TotalY | Relative to start | Relative to previous event | Relative to start |
| Pointer hover | Mouse/trackpad only | Mouse only | Mouse/pen |
| Cross-app drag-drop | Supported (iPadOS/Mac) | Not supported | Supported |
| Secondary button tap | Supported | Long-press fallback | Supported |

---

## Quick Rules

1. One `SwipeGestureRecognizer` per direction.
2. Use `PointerGestureRecognizer` for hover effects, not `TapGestureRecognizer`.
3. Set `AllowDrop="True"` on drop targets — it defaults to `false`.
4. Normalize pan deltas across platforms if sub-pixel accuracy matters.
5. Prefer commands over events for MVVM; both work identically.
6. In .NET 10 use `TapGestureRecognizer` instead of deprecated `ClickGestureRecognizer`.