# LongPressGestureRecognizer Implementation Plan for .NET 11

**Issue**: https://github.com/dotnet/maui/issues/8675  
**Target Branch**: `net11.0`  
**Created**: 2026-01-08

---

## Executive Summary

This document outlines the implementation plan for adding `LongPressGestureRecognizer` to .NET MAUI for .NET 11. Based on extensive research of platform APIs, existing MAUI gesture implementations, and the CommunityToolkit.Maui TouchBehavior, this plan provides a comprehensive approach to implementing a robust, cross-platform long-press gesture recognizer.

---

## 1. Research Findings & Design Decisions

### 1.1 Platform APIs Analysis

| Platform | Native API | Default Duration | Key Features |
|----------|-----------|------------------|--------------|
| **iOS** | `UILongPressGestureRecognizer` | 0.5 seconds | State-based (began/changed/ended), allowable movement, configurable touches |
| **Android** | `View.OnLongClickListener` | System-defined (~500ms) | Simple boolean return, limited configurability |
| **Windows** | `UIElement.Holding` event | System-defined | HoldingState (Started/Completed/Canceled), PointerPressed for custom timing |
| **Mac Catalyst** | Same as iOS | 0.5 seconds | Same as iOS |
| **Tizen** | GestureDetector | Customizable | Similar to Android pattern |

### 1.2 API Design Review

**Original Proposal Analysis:**
The proposed API from issue #8675 was based on Xamarin.Forms proposal. After review:

✅ **Keep:**
- `MinimumPressDuration` - Essential cross-platform property
- `Command` / `CommandParameter` - Standard MAUI pattern
- `LongPressed` event - Primary event for gesture completion
- `NumberOfTouchesRequired` - iOS/Catalyst support

⚠️ **Modify:**
- `IsLongPressing` property → Add but clarify it's iOS/Catalyst only for live tracking
- `NumberOfTapsRequired` → Remove (confusing - this is for multi-tap-then-hold which is uncommon)
- `AllowableMovement` → Change from `uint` to `double` (pixels) for consistency

✨ **Add:**
- `State` property (GestureStatus enum) - Align with other gesture recognizers
- `LongPressing` event - For real-time state updates (iOS/Catalyst primarily)
- Platform-specific behavior documentation

### 1.3 CommunityToolkit Comparison

The CommunityToolkit.Maui TouchBehavior already implements long-press via:
- Custom touch tracking with timers
- Cross-platform approach
- Works via Behaviors, not GestureRecognizers

**Our approach differs:**
- ✅ True GestureRecognizer (not Behavior) - integrates with existing gesture system
- ✅ Leverages native platform APIs where available
- ✅ Consistent with existing MAUI gesture recognizers (Tap, Swipe, Pan, etc.)
- ✅ Better performance via native implementations
- ✅ **Designed to work alongside other gestures** (see section 1.4)

### 1.4 Gesture Interaction Design Principles

**CRITICAL: LongPressGestureRecognizer MUST work alongside other gesture recognizers without breaking them.**

#### Research Findings from Existing MAUI Gestures

Analysis of `GesturePlatformManager.iOS.cs` reveals MAUI's gesture interaction strategy:

1. **SwipeGestureRecognizer** (line 406): Uses `ShouldRecognizeSimultaneously = (g, o) => true`
   - Allows swipes to work with other gestures
   
2. **PanGestureRecognizer** (line 391): Uses app-level configuration
   - `GetPanGestureRecognizerShouldRecognizeSimultaneously()` allows per-app control
   
3. **TapGestureRecognizer** (lines 563-595): Complex logic in `ShouldRecognizeTapsTogether`
   - Multiple taps can fire if they have same number of taps/touches
   - Enables multiple tap handlers on same element

4. **Real-world test case**: Issue #11333 - SwipeView + TapGesture interaction bug
   - Demonstrates importance of proper gesture conflict handling
   - Android required special handling to allow both gestures

#### LongPress Gesture Interaction Strategy

**Core Principle**: LongPress should **coexist** with other gestures, not replace them.

**Interaction Matrix**:

| Gesture Type | Interaction Behavior | Implementation Strategy |
|--------------|----------------------|-------------------------|
| **TapGestureRecognizer** | Both can fire - Tap on quick tap, LongPress on hold | iOS: `ShouldRecognizeSimultaneously = true`<br>Android: `e.Handled = false`<br>Windows: Don't set `e.Handled` |
| **SwipeGestureRecognizer** | Swipe cancels LongPress if movement threshold exceeded | Monitor movement in `AllowableMovement` property |
| **PanGestureRecognizer** | Pan cancels LongPress if movement threshold exceeded | Same as Swipe - movement tracking |
| **ScrollView** | Scroll cancels LongPress; LongPress works if stationary | Platform scroll detection cancels timer |
| **CollectionView/ListView** | LongPress works without triggering selection | No event consumption on touch down |
| **PinchGestureRecognizer** | Pinch takes priority (multi-touch) | Second finger cancels LongPress |

**Key Implementation Details**:

```csharp
// iOS - Enable simultaneous recognition
uiRecognizer.ShouldRecognizeSimultaneously = (g, o) => true;

// Android - Don't consume touch events
e.Handled = false;  // Let other handlers process the event

// Windows - Don't mark routed events as handled
// (Implicit - don't set e.Handled in pointer event handlers)
```

**Movement Threshold**:
- LongPress monitors finger movement
- If movement exceeds `AllowableMovement` (default: 10 pixels), LongPress cancels
- This allows swipe/pan/scroll to take over naturally

**Testing Requirements**:
- MUST test LongPress + every other gesture type
- MUST verify ScrollView scenarios (both holding still and scrolling)
- MUST verify collection scenarios (CollectionView, ListView, CarouselView)
- See Section 4.4 for comprehensive test matrix

---

## 2. Implementation Architecture

### 2.1 Core Components

```
src/Controls/src/Core/
├── LongPressGestureRecognizer.cs              (NEW)
├── LongPressedEventArgs.cs                    (NEW)
└── Platform/
    ├── GestureManager/
    │   ├── GestureManager.cs                  (MODIFY - add LongPress support)
    │   ├── GesturePlatformManager.iOS.cs      (MODIFY - add native iOS recognizer)
    │   ├── GesturePlatformManager.Android.cs  (MODIFY - add LongClickListener)
    │   ├── GesturePlatformManager.Windows.cs  (MODIFY - add Holding/PointerPressed)
    │   └── GesturePlatformManager.Tizen.cs    (MODIFY - add gesture detector)
    └── Android/
        └── LongPressGestureHandler.cs         (NEW - if needed for complex logic)
```

### 2.2 API Surface

```csharp
namespace Microsoft.Maui.Controls
{
    public sealed class LongPressGestureRecognizer : GestureRecognizer
    {
        // Bindable Properties
        public static readonly BindableProperty CommandProperty;
        public static readonly BindableProperty CommandParameterProperty;
        public static readonly BindableProperty MinimumPressDurationProperty;
        public static readonly BindableProperty NumberOfTouchesRequiredProperty;
        public static readonly BindableProperty AllowableMovementProperty;
        public static readonly BindableProperty StateProperty;
        
        // Properties
        public ICommand? Command { get; set; }
        public object? CommandParameter { get; set; }
        public int MinimumPressDuration { get; set; } = 500; // milliseconds
        public int NumberOfTouchesRequired { get; set; } = 1; // iOS/Catalyst only
        public double AllowableMovement { get; set; } = 10; // pixels, iOS/Catalyst only
        public GestureStatus State { get; private set; } // Read-only
        
        // Events
        public event EventHandler<LongPressedEventArgs>? LongPressed;
        public event EventHandler<LongPressingEventArgs>? LongPressing; // State updates
        
        // Internal methods called by platform
        internal void SendLongPressed(View sender, Point? position);
        internal void SendLongPressing(View sender, GestureStatus state, Point? position);
    }
    
    public class LongPressedEventArgs : EventArgs
    {
        public LongPressedEventArgs(object? parameter, Point? position)
        {
            Parameter = parameter;
            Position = position;
        }
        
        public object? Parameter { get; }
        public Point? Position { get; }
    }
    
    public class LongPressingEventArgs : EventArgs
    {
        public LongPressingEventArgs(GestureStatus status, Point? position)
        {
            Status = status;
            Position = position;
        }
        
        public GestureStatus Status { get; }
        public Point? Position { get; }
    }
}
```

---

## 3. Platform Implementation Details

### 3.1 iOS / Mac Catalyst

**Approach**: Use `UILongPressGestureRecognizer` directly

```csharp
// In GesturePlatformManager.iOS.cs
void UpdateLongPressGestureRecognizer(LongPressGestureRecognizer recognizer)
{
    var uiRecognizer = new UILongPressGestureRecognizer(HandleLongPress)
    {
        MinimumPressDuration = recognizer.MinimumPressDuration / 1000.0, // Convert to seconds
        NumberOfTouchesRequired = (nuint)recognizer.NumberOfTouchesRequired,
        AllowableMovement = recognizer.AllowableMovement
    };
    
    // Store and add to view
    AddGestureRecognizer(recognizer, uiRecognizer);
}

void HandleLongPress(UILongPressGestureRecognizer recognizer)
{
    var longPressRecognizer = GetMauiGestureRecognizer<LongPressGestureRecognizer>(recognizer);
    var position = recognizer.LocationInView(recognizer.View);
    var point = new Point(position.X, position.Y);
    
    switch (recognizer.State)
    {
        case UIGestureRecognizerState.Began:
            longPressRecognizer.SendLongPressing(view, GestureStatus.Started, point);
            break;
        case UIGestureRecognizerState.Changed:
            longPressRecognizer.SendLongPressing(view, GestureStatus.Running, point);
            break;
        case UIGestureRecognizerState.Ended:
            longPressRecognizer.SendLongPressed(view, point);
            longPressRecognizer.SendLongPressing(view, GestureStatus.Completed, point);
            break;
        case UIGestureRecognizerState.Cancelled:
            longPressRecognizer.SendLongPressing(view, GestureStatus.Canceled, point);
            break;
    }
}
```

**Gesture Interaction Strategy**:

iOS uses `ShouldRecognizeSimultaneously` to control gesture conflicts. For LongPress:

```csharp
// Enable simultaneous recognition with most gestures
uiRecognizer.ShouldRecognizeSimultaneously = (g, o) => 
{
    // Allow LongPress to work with Tap, Swipe, etc.
    // Similar to SwipeGestureRecognizer pattern (line 406 in GesturePlatformManager.iOS.cs)
    return true;
};
```

**Expected Behavior with Other Gestures**:
- **TapGestureRecognizer**: Both can fire - Tap on quick tap, LongPress on hold
- **SwipeGestureRecognizer**: LongPress cancelled if swipe movement exceeds `AllowableMovement`
- **PanGestureRecognizer**: LongPress cancelled if pan starts (movement threshold)
- **In ScrollView**: LongPress works if stationary; scroll cancels if moved
- **In CollectionView/ListView**: LongPress on item works without triggering selection (if held still)

**Testing Strategy**: Use existing iOS gesture test patterns + multi-gesture integration tests

### 3.2 Android

**Approach**: Use `View.OnLongClickListener` + timer for minimum duration control

```csharp
// In GesturePlatformManager.Android.cs
void UpdateLongPressGestureRecognizer(LongPressGestureRecognizer recognizer)
{
    if (platformView is not Android.Views.View androidView)
        return;
        
    // Android doesn't support configurable duration natively
    // We'll need custom implementation with MotionEvent tracking
    var handler = new LongPressGestureHandler(recognizer, androidView);
    StoreLongPressHandler(recognizer, handler);
}

class LongPressGestureHandler
{
    readonly LongPressGestureRecognizer _recognizer;
    readonly Android.Views.View _view;
    System.Threading.Timer? _timer;
    float _startX, _startY;
    
    public LongPressGestureHandler(LongPressGestureRecognizer recognizer, Android.Views.View view)
    {
        _recognizer = recognizer;
        _view = view;
        _view.Touch += OnTouch;
    }
    
    void OnTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        switch (e.Event?.Action)
        {
            case Android.Views.MotionEventActions.Down:
                _startX = e.Event.GetX();
                _startY = e.Event.GetY();
                StartTimer();
                // IMPORTANT: Don't consume the event - let it pass through to other gesture handlers
                e.Handled = false;
                break;
            case Android.Views.MotionEventActions.Move:
                // Check allowable movement
                var deltaX = Math.Abs(e.Event.GetX() - _startX);
                var deltaY = Math.Abs(e.Event.GetY() - _startY);
                if (deltaX > _recognizer.AllowableMovement || deltaY > _recognizer.AllowableMovement)
                    CancelTimer();
                // Don't consume - allow scroll, swipe, pan to still work
                e.Handled = false;
                break;
            case Android.Views.MotionEventActions.Up:
            case Android.Views.MotionEventActions.Cancel:
                CancelTimer();
                e.Handled = false;
                break;
        }
    }
    
    void StartTimer()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            _view.Handler?.MauiContext?.GetDispatcherProvider()?.GetForCurrentThread()?.Dispatch(() =>
            {
                var point = new Point(_startX, _startY);
                _recognizer.SendLongPressed(_view, point);
            });
        }, null, _recognizer.MinimumPressDuration, Timeout.Infinite);
    }
    
    void CancelTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }
}
```

**Gesture Interaction Strategy for Android**:

Android's touch event system is more straightforward - by setting `e.Handled = false`, we allow touch events to propagate to other gesture handlers. The key behaviors:

- **With TapGestureRecognizer**: Both work - tap fires on quick release, long press fires after duration
- **With ScrollView/RecyclerView**: LongPress timer starts but cancels if user scrolls (movement threshold)
- **With SwipeGestureRecognizer**: Movement cancels LongPress timer, swipe proceeds normally
- **Not consuming events** allows the Android gesture system to handle conflicts naturally

**Testing Strategy**: Appium-based UI tests with touch hold actions + multi-gesture scenarios

### 3.3 Windows

**Approach**: Use `UIElement.Holding` event + custom PointerPressed for duration control

```csharp
// In GesturePlatformManager.Windows.cs
void UpdateLongPressGestureRecognizer(LongPressGestureRecognizer recognizer)
{
    if (platformView is not Microsoft.UI.Xaml.UIElement uiElement)
        return;
    
    // For custom duration, we need PointerPressed + timer
    var handler = new LongPressGestureHandler(recognizer, uiElement);
    StoreLongPressHandler(recognizer, handler);
}

class LongPressGestureHandler
{
    readonly LongPressGestureRecognizer _recognizer;
    readonly Microsoft.UI.Xaml.UIElement _element;
    DispatcherTimer? _timer;
    Windows.Foundation.Point _startPosition;
    
    public LongPressGestureHandler(LongPressGestureRecognizer recognizer, Microsoft.UI.Xaml.UIElement element)
    {
        _recognizer = recognizer;
        _element = element;
        _element.PointerPressed += OnPointerPressed;
        _element.PointerReleased += OnPointerReleased;
        _element.PointerCanceled += OnPointerReleased;
        _element.PointerMoved += OnPointerMoved;
    }
    
    // Implementation similar to Android with timer
}
```

**Gesture Interaction Strategy for Windows**:

Windows UWP/WinUI uses routed events. The key to multi-gesture support:

```csharp
void OnPointerPressed(object sender, PointerRoutedEventArgs e)
{
    _startPosition = e.GetCurrentPoint(_element).Position;
    StartTimer();
    // CRITICAL: Don't mark as handled - allow other gestures to process
    // e.Handled = false is implicit if not set
}

void OnPointerMoved(object sender, PointerRoutedEventArgs e)
{
    var currentPos = e.GetCurrentPoint(_element).Position;
    var deltaX = Math.Abs(currentPos.X - _startPosition.X);
    var deltaY = Math.Abs(currentPos.Y - _startPosition.Y);
    
    if (deltaX > _recognizer.AllowableMovement || deltaY > _recognizer.AllowableMovement)
        CancelTimer();
    
    // Don't handle - let scrolling/panning/swiping work
}
```

**Expected Behavior**:
- **With Tapped event**: Both fire appropriately based on timing
- **With ScrollViewer**: LongPress cancels if scroll movement occurs
- **With ManipulationStarting**: Movement threshold determines which wins
- **Routed events** propagate naturally when not marked as handled

**Testing Strategy**: WinAppDriver UI tests + multi-gesture integration scenarios

---

## 4. Testing Strategy

### 4.1 Unit Tests

**Location**: `src/Controls/tests/Core.UnitTests/Gestures/`

```
LongPressGestureRecognizerTests.cs (NEW)
├── Constructor_InitializesDefaultValues()
├── Command_CanBeSet()
├── CommandParameter_CanBeSet()
├── MinimumPressDuration_DefaultIs500Ms()
├── MinimumPressDuration_CanBeChanged()
├── NumberOfTouchesRequired_DefaultIs1()
├── AllowableMovement_DefaultIs10()
├── SendLongPressed_InvokesCommand()
├── SendLongPressed_RaisesEvent()
├── PropertyChanged_RaisesCorrectly()
└── State_UpdatesCorrectly()
```

### 4.2 Device Tests

**Location**: `src/Controls/tests/DeviceTests/`

```
LongPressGestureTests.cs (NEW)
├── LongPressGesture_FiresAfterDuration_iOS()
├── LongPressGesture_FiresAfterDuration_Android()
├── LongPressGesture_FiresAfterDuration_Windows()
├── LongPressGesture_CancelsOnMovement()
├── LongPressGesture_CustomDuration()
├── LongPressGesture_MultipleGestures()
└── LongPressGesture_WithOtherGestures()
```

### 4.3 UI Tests

**Location**: `src/Controls/tests/TestCases.HostApp/Issues/`

```
IssueLongPress.xaml (NEW - test page)
IssueLongPress.cs (NEW - NUnit tests)
├── LongPress_TriggersCommand()
├── LongPress_WithCustomDuration()
├── LongPress_OnLabel()
├── LongPress_OnImage()
├── LongPress_OnFrame()
└── LongPress_DoesNotInterfereWithTap()
```

### 4.4 Gesture Interaction Tests (CRITICAL)

**Purpose**: Verify LongPressGestureRecognizer works correctly alongside other gesture recognizers without conflicts.

**Test Matrix**:

| Scenario | Expected Behavior | Test Method |
|----------|-------------------|-------------|
| **LongPress + Tap on same element** | Quick tap fires Tap, hold fires LongPress | Both events should fire independently |
| **LongPress + Swipe on same element** | Swipe movement cancels LongPress | Swipe works, LongPress cancels on movement |
| **LongPress + Pan on same element** | Pan movement cancels LongPress | Pan works, LongPress cancels when threshold exceeded |
| **LongPress in ScrollView** | Hold still = LongPress fires; scroll = LongPress cancels | Test both scenarios |
| **LongPress in CollectionView item** | LongPress fires without selection | Item not selected when holding still |
| **LongPress in ListView cell** | LongPress fires without selection | Cell not selected when holding still |
| **Multiple LongPress recognizers** | All can fire independently | Each recognizer works on its own element |
| **LongPress + PinchGesture** | Pinch takes priority | LongPress cancels when second finger down |

**Implementation**:

```
LongPressGestureInteractionTests.cs (NEW)
├── LongPressWithTap_BothFireIndependently()
├── LongPressWithSwipe_SwipeCancelsLongPress()
├── LongPressWithPan_PanCancelsLongPress()
├── LongPressInScrollView_CancelsOnScroll()
├── LongPressInScrollView_FiresWhenStill()
├── LongPressInCollectionView_NoSelection()
├── LongPressInListView_NoSelection()
├── MultipleLongPress_AllWorkIndependently()
└── LongPressWithPinch_PinchTakesPriority()
```

**Test Page Example** (`TestCases.HostApp/Issues/IssueLongPressInteraction.xaml`):

```xaml
<ContentPage>
    <VerticalStackLayout>
        <!-- Test: LongPress + Tap -->
        <Frame AutomationId="TapAndLongPressFrame">
            <Frame.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnTapped" />
                <LongPressGestureRecognizer LongPressed="OnLongPressed" />
            </Frame.GestureRecognizers>
            <Label Text="Tap or Long Press Me" />
        </Frame>
        <Label AutomationId="TapLabel" Text="Tap Count: 0" />
        <Label AutomationId="LongPressLabel" Text="Long Press Count: 0" />
        
        <!-- Test: LongPress + Swipe -->
        <Frame AutomationId="SwipeAndLongPressFrame">
            <Frame.GestureRecognizers>
                <SwipeGestureRecognizer Direction="Left" Swiped="OnSwiped" />
                <LongPressGestureRecognizer LongPressed="OnLongPressed2" />
            </Frame.GestureRecognizers>
            <Label Text="Swipe or Long Press Me" />
        </Frame>
        <Label AutomationId="SwipeLabel" Text="Swipe Count: 0" />
        <Label AutomationId="LongPress2Label" Text="Long Press Count: 0" />
        
        <!-- Test: LongPress in ScrollView -->
        <ScrollView AutomationId="TestScrollView">
            <VerticalStackLayout>
                <Frame AutomationId="LongPressInScrollFrame">
                    <Frame.GestureRecognizers>
                        <LongPressGestureRecognizer LongPressed="OnLongPressed3" />
                    </Frame.GestureRecognizers>
                    <Label Text="Long Press in ScrollView" />
                </Frame>
                <BoxView HeightRequest="1000" Color="Gray" />
            </VerticalStackLayout>
        </ScrollView>
        <Label AutomationId="LongPress3Label" Text="Long Press Count: 0" />
    </VerticalStackLayout>
</ContentPage>
```

**UI Test Example** (`TestCases.Shared.Tests/Tests/Issues/IssueLongPressInteraction.cs`):

```csharp
[Test]
[Category(UITestCategories.Gestures)]
public void LongPressWithTap_BothFireIndependently()
{
    // Quick tap should only fire Tap
    App.WaitForElement("TapAndLongPressFrame");
    App.Tap("TapAndLongPressFrame");
    
    var tapLabel = App.FindElement("TapLabel").GetText();
    var longPressLabel = App.FindElement("LongPressLabel").GetText();
    Assert.That(tapLabel, Is.EqualTo("Tap Count: 1"));
    Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 0"));
    
    // Long press should fire LongPress (and might fire Tap on release)
    App.TouchAndHold("TapAndLongPressFrame", TimeSpan.FromSeconds(1));
    
    longPressLabel = App.FindElement("LongPressLabel").GetText();
    Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 1"));
}

[Test]
[Category(UITestCategories.Gestures)]
public void LongPressWithSwipe_SwipeCancelsLongPress()
{
    App.WaitForElement("SwipeAndLongPressFrame");
    
    // Swipe left - should fire swipe but NOT long press
    App.SwipeLeftToRight("SwipeAndLongPressFrame");
    
    var swipeLabel = App.FindElement("SwipeLabel").GetText();
    var longPressLabel = App.FindElement("LongPress2Label").GetText();
    
    Assert.That(swipeLabel, Is.EqualTo("Swipe Count: 1"));
    Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 0"));
}

[Test]
[Category(UITestCategories.Gestures)]
public void LongPressInScrollView_FiresWhenStill()
{
    App.WaitForElement("LongPressInScrollFrame");
    
    // Hold still - should fire LongPress
    App.TouchAndHold("LongPressInScrollFrame", TimeSpan.FromSeconds(1));
    
    var label = App.FindElement("LongPress3Label").GetText();
    Assert.That(label, Is.EqualTo("Long Press Count: 1"));
}
```

### 4.5 Integration Tests (Existing Pattern)

Test compatibility with existing gesture recognizers:
- LongPress + TapGesture on same element
- LongPress + SwipeGesture on same element
- LongPress in ScrollView
- LongPress in CollectionView items
- LongPress in ListView cells

---

## 5. Implementation Checklist

### Phase 1: Core Implementation (Week 1)
- [ ] Create `LongPressGestureRecognizer.cs` with public API
- [ ] Create `LongPressedEventArgs.cs` and `LongPressingEventArgs.cs`
- [ ] Add to `PublicAPI.Unshipped.txt` for all platforms
- [ ] Write unit tests for core class
- [ ] Update XML documentation files

### Phase 2: iOS/Mac Catalyst (Week 1-2)
- [ ] Implement iOS platform handler in `GesturePlatformManager.iOS.cs`
- [ ] Add native `UILongPressGestureRecognizer` integration
- [ ] Handle all gesture states properly
- [ ] Write iOS device tests
- [ ] Write iOS UI tests

### Phase 3: Android (Week 2)
- [ ] Implement Android platform handler in `GesturePlatformManager.Android.cs`
- [ ] Create custom `LongPressGestureHandler` with timer
- [ ] Handle touch events and movement tracking
- [ ] Write Android device tests
- [ ] Write Android UI tests

### Phase 4: Windows (Week 2-3)
- [ ] Implement Windows platform handler in `GesturePlatformManager.Windows.cs`
- [ ] Create custom handler with `PointerPressed` + timer
- [ ] Handle pointer events properly
- [ ] Write Windows device tests
- [ ] Write Windows UI tests (if feasible)

### Phase 5: Tizen (Week 3)
- [ ] Implement Tizen platform handler (if resources available)
- [ ] Follow Android pattern with gesture detector
- [ ] Basic testing

### Phase 6: Gesture Interaction Testing (Week 3) **CRITICAL**
- [ ] Create comprehensive gesture interaction test suite
- [ ] Test LongPress + Tap combinations on all platforms
- [ ] Test LongPress + Swipe combinations on all platforms
- [ ] Test LongPress + Pan combinations on all platforms
- [ ] Test LongPress in ScrollView scenarios (hold still vs scroll)
- [ ] Test LongPress in CollectionView/ListView (no selection when held)
- [ ] Test multiple simultaneous LongPress recognizers
- [ ] Verify `ShouldRecognizeSimultaneously` behavior on iOS/Catalyst
- [ ] Verify `e.Handled = false` behavior on Android
- [ ] Verify routed event propagation on Windows
- [ ] Document any platform-specific interaction quirks
- [ ] Performance testing with multiple gestures

### Phase 7: Integration & Polish (Week 3-4)
- [ ] Test interaction with existing gesture recognizers
- [ ] Performance testing and optimization
- [ ] Accessibility testing
- [ ] Documentation updates:
  - [ ] API documentation
  - [ ] Migration guide from CommunityToolkit
  - [ ] Sample gallery page
  - [ ] Platform-specific behavior documentation
  - [ ] **Gesture interaction guide** (best practices)
- [ ] Code review and refinement

### Phase 8: Testing & Validation (Week 4)
- [ ] Run full test suite on all platforms
- [ ] Manual testing with sample apps
- [ ] Performance profiling
- [ ] Memory leak testing
- [ ] Edge case testing:
  - [ ] Rapid gesture changes
  - [ ] Multiple simultaneous recognizers
  - [ ] Dynamic add/remove of recognizers
  - [ ] View hierarchy changes during gesture

---

## 6. Backward Compatibility & Migration

### 6.1 Breaking Changes
**None** - This is a new API in .NET 11

### 6.2 CommunityToolkit Migration Path

For users currently using `CommunityToolkit.Maui.TouchBehavior`:

**Before (CommunityToolkit):**
```xml
<Image>
    <Image.Behaviors>
        <toolkit:TouchBehavior 
            LongPressCommand="{Binding LongPressCommand}" 
            LongPressDuration="1000" />
    </Image.Behaviors>
</Image>
```

**After (MAUI built-in):**
```xml
<Image>
    <Image.GestureRecognizers>
        <LongPressGestureRecognizer 
            Command="{Binding LongPressCommand}"
            MinimumPressDuration="1000"
            LongPressed="OnLongPressed" />
    </Image.GestureRecognizers>
</Image>
```

**Migration Notes:**
- Property name change: `LongPressDuration` → `MinimumPressDuration`
- Move from `Behaviors` to `GestureRecognizers`
- Event-based approach now available (`LongPressed` event)
- Better performance with native implementations

---

## 7. Documentation Requirements

### 7.1 XML Documentation
- All public members must have `<summary>`, `<remarks>`, `<param>`, `<returns>`
- Include platform-specific behavior notes
- Add code examples

### 7.2 Conceptual Documentation
- Create new docs page: "Long Press Gestures"
- Add to gesture recognizers overview
- Platform differences section
- Best practices guide

### 7.3 Sample Code
- Add to `Controls.Sample` gallery
- Create comprehensive demo page showing:
  - Basic usage
  - Custom duration
  - Multiple touches (iOS)
  - Allowable movement
  - Integration with commands
  - Event handling

---

## 8. Performance Considerations

### 8.1 Memory
- Properly dispose timers (Android/Windows)
- Weak references where appropriate
- Avoid event handler leaks

### 8.2 Touch Responsiveness
- Minimize work on UI thread during gesture
- Use native APIs where possible (better than custom timing)
- Profile gesture latency across platforms

### 8.3 Battery Impact
- Ensure timers are cancelled properly
- No background work during idle

---

## 9. Known Limitations & Platform Differences

### 9.1 Android Limitations
- System-defined long-press duration cannot be overridden natively
- Custom implementation required for `MinimumPressDuration`
- No native support for "began/changed/ended" states
- `NumberOfTouchesRequired` not supported (always 1)

### 9.2 Windows Limitations
- `Holding` event has fixed system duration
- Custom implementation needed for configurable duration
- `NumberOfTouchesRequired` not applicable (mouse/pointer model)

### 9.3 iOS/Catalyst Advantages
- Native `UILongPressGestureRecognizer` support
- Full state tracking (began/changed/ended)
- `NumberOfTouchesRequired` supported
- `AllowableMovement` natively supported

### 9.4 Recommended Defaults
Based on platform research:
- `MinimumPressDuration`: 500ms (matches iOS default)
- `NumberOfTouchesRequired`: 1 (iOS/Catalyst only)
- `AllowableMovement`: 10 pixels (generous for finger inaccuracy)

---

## 10. Success Criteria

### 10.1 Functional Requirements
- ✅ Long press fires after specified duration on all platforms
- ✅ Long press cancels on excessive movement
- ✅ Command execution works correctly
- ✅ Event raising works correctly
- ✅ State tracking works (iOS/Catalyst)
- ✅ Compatible with other gesture recognizers

### 10.2 Quality Requirements
- ✅ 100% unit test coverage for core logic
- ✅ Device tests pass on iOS, Android, Windows
- ✅ UI tests pass on iOS, Android
- ✅ No memory leaks detected
- ✅ No performance regressions
- ✅ XML documentation complete
- ✅ Sample code works as expected

### 10.3 Acceptance Criteria
- ✅ All tests passing on CI
- ✅ Code review approved
- ✅ Documentation reviewed
- ✅ Sample app demonstrates all features
- ✅ No high-priority bugs
- ✅ Performance benchmarks meet targets

---

## 11. Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Platform API differences cause inconsistent behavior | High | Medium | Comprehensive cross-platform testing, document differences clearly |
| Timer-based approaches (Android/Windows) have latency | Medium | High | Profile and optimize, consider platform-specific tuning |
| **Conflicts with existing gesture recognizers** | **High** | **Medium** | **CRITICAL: Extensive integration testing (Section 4.4), proper `ShouldRecognizeSimultaneously` on iOS, `e.Handled = false` on Android, routed event best practices on Windows** |
| **LongPress breaks ScrollView/CollectionView** | **High** | **Medium** | **Movement threshold testing, ensure events not consumed, test real-world scrolling scenarios** |
| Breaking existing app behaviors | High | Low | This is a new API, no breaking changes |
| Performance impact on scrolling/lists | Medium | Medium | Optimize touch event handling, profile CollectionView scenarios |
| **Multi-gesture scenarios not tested adequately** | **High** | **Low** | **Comprehensive gesture interaction test matrix (Section 4.4), test all combinations** |

---

## 12. References

### Platform Documentation
- [Apple - UILongPressGestureRecognizer](https://developer.apple.com/documentation/uikit/uilongpressgesturerecognizer)
- [Apple - Handling Long Press Gestures](https://developer.apple.com/documentation/uikit/handling-long-press-gestures)
- [Android - View.OnLongClickListener](https://developer.android.com/reference/android/view/View.OnLongClickListener)
- [Microsoft - UIElement.Holding Event](https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.uielement.holding)

### Related Issues
- [Xamarin.Forms #3480](https://github.com/xamarin/Xamarin.Forms/issues/3480) - Original Xamarin proposal
- [CommunityToolkit.Maui #86](https://github.com/CommunityToolkit/Maui/issues/86) - TouchBehavior implementation
- [MAUI #11333](https://github.com/dotnet/maui/issues/11333) - SwipeView + TapGesture Android bug (gesture interaction example)

### MAUI Gesture System
- Existing gesture recognizers: `TapGestureRecognizer`, `PanGestureRecognizer`, `SwipeGestureRecognizer`, `PinchGestureRecognizer`
- `GestureManager` and `GesturePlatformManager` architecture
- `GestureStatus` enum for state tracking
- **Key reference**: `GesturePlatformManager.iOS.cs` lines 391, 406, 563-595 for gesture interaction patterns

---

## 13. Timeline

**Estimated Total Time**: 3-4 weeks

- **Week 1**: Core API + iOS/Catalyst implementation + unit tests
- **Week 2**: Android + Windows implementation + device tests
- **Week 3**: Tizen (if needed) + Integration testing + UI tests
- **Week 4**: Polish, documentation, code review, final testing

**Target Completion**: End of Q1 2026 for .NET 11 Preview inclusion

---

## 14. Open Questions

1. ~~Should we support `NumberOfTapsRequired` (tap-then-hold)?~~ **Decision: No - too niche**
2. ~~Should `AllowableMovement` be `uint` or `double`?~~ **Decision: double (pixels) - consistent with graphics**
3. Should we expose platform-specific properties via PlatformConfiguration? **To discuss**
4. Should there be a global default `MinimumPressDuration`? **To discuss**
5. How should LongPress interact with ScrollView scrolling? **Test and document**

---

## 15. Approvals

- [ ] Technical Review - Lead Engineer
- [ ] API Review - API Council
- [ ] Design Review - UX Team
- [ ] Security Review - If applicable
- [ ] Performance Review - Performance Team
- [ ] Final Approval - Product Owner

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-08  
**Author**: GitHub Copilot CLI  
**Reviewers**: TBD
