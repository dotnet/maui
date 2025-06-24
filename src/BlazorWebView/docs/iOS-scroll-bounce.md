# BlazorWebView iOS Scroll Bounce Behavior

## Overview

BlazorWebView on iOS automatically disables elastic/bounce scrolling to make Blazor Hybrid applications feel more like native iOS apps.

## Behavior

By default, iOS WebViews (WKWebView) include an elastic scrolling effect that creates a "bounce" when users scroll past the edge of content. This bounce effect is characteristic of web content and can make Blazor Hybrid apps feel less native, especially when fixed/sticky UI elements bounce around during overscroll.

To address this, BlazorWebView automatically disables bounce scrolling on iOS and Mac Catalyst by setting the following properties on the underlying UIScrollView:

- `Bounces = false`
- `AlwaysBounceVertical = false` 
- `AlwaysBounceHorizontal = false`

## Implementation

This behavior is implemented automatically in the `CreatePlatformView()` method of `BlazorWebViewHandler` and requires no additional configuration from developers.

## Benefits

- Makes Blazor Hybrid apps feel more like native iOS apps
- Prevents fixed/sticky UI elements from bouncing during overscroll
- Provides a consistent, native-feeling user experience
- Zero configuration required

## Platform Support

This behavior applies to:
- iOS 11.0+
- Mac Catalyst 10.0+

The behavior has no effect on other platforms (Android, Windows, etc.) where bounce scrolling is not a characteristic behavior.

## Related

- Issue: [#6689](https://github.com/dotnet/maui/issues/6689)