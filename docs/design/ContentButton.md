# ContentButton

`ContentButton` is a native button with one presentation view as its content.
It composes `IButton` and `IContentView` and derives from `ContentView`, so logical
children, inherited binding contexts, resources and control templates follow the
normal content-control implementation.

```xml
<ContentButton Command="{Binding OpenDetails}"
               CommandParameter="{Binding SelectedItem}"
               Padding="12,8"
               BorderColor="Purple"
               BorderWidth="1"
               CornerRadius="8"
               SemanticProperties.Description="Open item details">
    <HorizontalStackLayout Spacing="8">
        <Image Source="details.png" WidthRequest="24" HeightRequest="24" />
        <Label Text="Details" VerticalOptions="Center" />
    </HorizontalStackLayout>
</ContentButton>
```

## Behavior

The command executes before `Clicked`, just as for `Button`. `CanExecute` combines
with explicit, bound and ancestor `IsEnabled` values. Command tracking uses the
shared weak subscription and command-parameter binding dependency implementation.
`Pressed`, `Released`, read-only `IsPressed` and the `Pressed` visual state use
`ButtonElement`. A release clears the pressed state even after disabling.
Releasing outside the button's bounds cancels activation, including on Apple
platforms where UIKit normally allows a small release-outside tolerance.

Only presentation content is supported in this first implementation. Labels,
images and layouts inside the button are not separate input or accessibility
targets. Nested buttons, entries, gesture recognizers and other interactive
descendants are not supported. Give the owning button an explicit, localized
`SemanticProperties.Description`; content is not automatically concatenated into
an accessible name. Native focus and activation belong to the owning button.

`BorderColor`, `BorderWidth` and `CornerRadius` match `Button`. The default border
width and radius are `-1`, meaning native defaults. Negative border widths contribute
zero to the content inset. `Padding` is inherited from `ContentView` and defaults
to zero; use styles to set presentation padding rather than depending on a native
text button's text insets. Content padding and nonnegative border width are applied
once, with nonnegative inner constraints. Requested, minimum and maximum dimensions
use MAUI layout constraint resolution. Content margins, physical alignment and
inherited flow direction follow `ContentView`.

Android ripple color is a platform-specific property, not part of `IContentButton`:

```xml
<ContentButton xmlns:android="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;assembly=Microsoft.Maui.Controls"
               android:ContentButton.RippleColor="#40808080"
               SemanticProperties.Description="Open item details">
    <Label Text="Details" />
</ContentButton>
```

Clear the attached property or set it to `null` to return to the Android theme's
ripple color. An explicit transparent color disables the color rather than
requesting the theme default.

## Platforms

The native implementations target Android, iOS, Mac Catalyst and Windows.
Tizen is explicitly unsupported and throws `PlatformNotSupportedException` when
its handler is created; it does not fall back to a non-interactive `ContentView`.
GTK4 and AppKit backends are outside this implementation.

## Coverage and manual checks

Shared command/content tests are in `ContentButtonTests`, layout tests in
`ContentButtonLayoutTests`, and XAML inflation tests in `ContentButtonPage.Tests`
(runtime, XamlC and source generation). Native tests use the existing Button
device-test category. The TestCases host includes **ContentButton Feature Matrix**
and corresponding `ContentButtonFeatureTests` in the Button UI-test category.
The feature page exposes content replacement/removal, handler replacement,
background changes, disable-on-press and remove-on-press.
Apple device tests also assert native content frames before and after content
replacement, for both direct content and nested layouts. Native measurement tests
check content and inset dimensions, not just nonnegative sizes.
Template replacement covers both a direct presenter and one nested in a layout,
including returning to untemplated content. Windows stroke-reset coverage compares
against the native default captured before applying an explicit border.

The following checks require real platform runs; adding a test does not certify
that a platform has passed:

| Area | Check |
|---|---|
| Touch | Tap nested label, image and padding. Each activation executes one command followed by one `Clicked`. |
| Cancellation | Drag outside, drag within a scroll ancestor, disable/remove during a press, and unload the window. No activation; `IsPressed` returns to false. |
| Layout | Empty/replaced content, text/image resizing, asymmetric insets, tiny constraints, requested/minimum/maximum sizes, margins and RTL. |
| Android feedback | Theme default, light/dark configuration change, alpha/transparent/reset ripple, opaque descendants, gradient/radius/background changes and reattachment. |
| Apple | Native accessibility activation and touch cancellation, ancestor gestures, fixed/default radius changes and handler reconnection. |
| Windows | Tab focus, Space/Enter, capture loss, unload while pressed and UI Automation Invoke. |
| Accessibility | TalkBack, VoiceOver and Narrator announce one named button with its disabled state; descendants are not separate focus stops; the native activate action executes once. |
| Deployment | Run trimmed/NativeAOT host builds with the repository's existing platform build tooling. |

Screen-reader speech, focus traversal, native feedback appearance and real hardware
keyboard behavior are not proved by shared unit tests or synthetic native events.
