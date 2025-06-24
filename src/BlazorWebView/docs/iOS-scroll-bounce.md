# BlazorWebView iOS Scroll Bounce Configuration

This feature allows developers to disable the elastic/bounce scrolling effect in BlazorWebView on iOS to make Blazor Hybrid apps feel more like native apps.

## Problem

By default, iOS WebViews (including WKWebView used by BlazorWebView) have an elastic scrolling effect that creates a "bounce" when users scroll past the edge of the content. This bounce effect is characteristic of web content and can make Blazor Hybrid apps feel less native, especially when fixed/sticky UI elements bounce around during the overscroll.

## Solution

A new iOS-specific platform configuration property `IsScrollBounceEnabled` has been added to BlazorWebView that allows developers to disable this bounce effect.

## Usage

```csharp
using Microsoft.AspNetCore.Components.WebView.Maui.PlatformConfiguration.iOSSpecific;

// In your page constructor or elsewhere where you configure the BlazorWebView
blazorWebView.On<iOS>().SetIsScrollBounceEnabled(false);
```

## Example

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
        #if IOS
        // Disable scroll bounce on iOS to make the app feel more native
        myBlazorWebView.On<iOS>().SetIsScrollBounceEnabled(false);
        #endif
    }
}
```

## Default Behavior

- **Default**: `true` (bounce scrolling is enabled)
- **Platform**: iOS and Mac Catalyst only
- **Effect**: When set to `false`, disables `Bounces`, `AlwaysBounceVertical`, and `AlwaysBounceHorizontal` on the underlying UIScrollView

## Benefits

- Makes Blazor Hybrid apps feel more like native iOS apps
- Prevents fixed/sticky UI elements from bouncing during overscroll
- Provides better user experience for apps that want to hide their web-based nature

## Related

- Issue: [#6689](https://github.com/dotnet/maui/issues/6689)
- Similar to the existing `ShouldDelayContentTouches` platform configuration for ScrollView