# ScrollView Implementation Notes

This document explains the implementation of ScrollView on each platform.

# The Problem with ScrollView

ScrollView is a challenging control to implement in a cross-platform way because the three primary target platforms (Windows, iOS, and Android) all have different rules for their native scrolling content controls. To make things even more challenging, we are also trying to make the .NET MAUI ScrollView work as closely as possible to the Xamarin.Forms ScrollView (for ease of migration). 

# Interface

Cross-platform implementations of a ScrollView need to implement the `Microsoft.Maui.IScrollView` interface. In the Maui.Controls library, this implementation is `Microsoft.Maui.Controls.ScrollView`. 

`IScrollView` derives from `IContentView` - a ScrollView in MAUI contains a single piece of content. The ScrollView will attempt to expand to be large enough to contain that content unless otherwise constrained; if constrained, the ScrollView will show a subsection of the content in its viewport, and allow scrolling to show the content in directions specified by the `Orientation` property (and depending on the settings for invidual scroll bar visibility). 


## Scroll Bar Visibility

`IScrollView` has two scroll bar visibility properties: `HorizontalScrollBarVisibility` and `VerticalScrollBarVisibility`. Both support 3 possible values: `Default`, `Always`, and `Never`. 

If the value is set to `Never`, the scroll bar in that direction will not be visible. The content will still be scrollable if it exceeds the size of the viewport. 

If the value is set to `Always`, the scroll bar in that direction will be visible even if there isn't sufficient content to require scrolling. 

If the value is set to `Default`, the scroll bar visibility will follow the rules of the target platform. Usually this means that the scroll bar will become visible if there is sufficient content to required scrolling in that direction, and the scroll bar will not be visible otherwise. Other behaviors may also apply (such as scroll bar fading) as dictated by the platform. 

## Orientation

The `Orientation` property of `IScrollView` can be one of four values (in the `ScrollOrientation` enum):

 - `Vertical` - the content scrolls vertically if it's taller than the viewport
 - `Horizontal` - the content scrolls horizontally if it's wider than the viewport
 - `Both` - the content scrolls horizontally if it's wider than the viewport, and vertically if it's taller than the viewport
 - `Neither` - scrolling is disabled in both directions
 
The `Orientation` value affects how the ScrollView's `Content` is measured and laid out. If the value is `Vertical`, the measurement height is unconstrained (i.e., `Double.Infinity`). If the value is `Horizontal`, the measurement width is unconstrained. `Both` results in measurement being unconstrained in all directions, and `Neither` constrains the measurement to the width and height of the viewport.

## ContentSize

This is a read-only value determined by the actual size of the ScrollView's `Content`, which may (and usually does) exceed the size of the ScrollView itself.

## Offsets

`IScrollView` has two `double` values, `HorizontalOffset` and `VerticalOffset` which specify the offsets of the viewport relative to the content. This can also be thought of as the scroll position of the ScrollView in each direction. 

## Scroll methods

`IScrollView` defines two methods related to scrolling. `RequestScrollTo()` is used by the virtual view to request that the native view scroll to the specified horizontal and vertical offsets. It includes a `bool` parameter to specify whether the scrolling operation should be animated or instant. 

The other method, `ScrollFinished()`, is called by the native platform to indicate that a scrolling operation has finished. This is used to signal that scrolling is finished for various `async` operations. 

# Platform Implementations

The behavior of the `Padding` property on ScrollView (inherited from Forms) requires that the padding is applied _inside_ the scrollable portion of the ScrollView. Also, the content of a ScrollView may have its own `Margin`. The inset of content in a ScrollView is effectively the sum of the ScrollView's `Padding` and the content's `Margin`. However, the various platforms all treat these properties differently within their native ScrollView equivalents. Much of the complexity of the ScrollViewHandler for each platform is addressing these differences.

## Windows

We'll start with Windows, because it's the most confusing. First off, it's important to note that as of this writing, the backing control for the .NET MAUI ScrollView is [`Microsoft.UI.Xaml.Controls.ScrollViewer`](https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.scrollviewer?view=winrt-22621). It is _not_ [`Microsoft.UI.Xaml.Controls.ScrollView`](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.scrollview?view=windows-app-sdk-1.4), as this control was not available when .NET MAUI was first ported from Forms. _This may change in the future._ But for now, the native control is a ScrollView_er_. 

The ScrollViewer control behavior differs from the .NET MAUI behavior (and Forms) in a couple of ways:

	- The native `Padding` property creates space _around_ the scrollable area, rather than _inside_ of it. 
	- The ScrollViewer forces the content to start at location (0, 0) in the ScrollViewer, which defeats our cross-platform layout's `Margin` property. 
	
To compound our problems, ScrollViewer is `sealed`. So we cannot override the measure/arrange behavior.
	
So, to make the Windows implementation of ScrollView work the way we want, we insert an extra layer - a `ContentPanel`. The native `Content` property of the ScrollViewer is set our extra `ContentPanel`, which hosts the content of the virtual ScrollView. This intermediate `ContentPanel` provides our virtual `Padding` and `Margin` property behaviors, and is responsible for invoking the `CrossPlatformMeasure()` and `CrossPlatformArrange()` methods. 

## Android

Our Android implementation of ScrollView is backed by MauiScrollView, which is a subclass of NestedScrollView. Again, we have some issues because the fundamentals of ScrollView on Android differ from our .NET MAUI target behaviors:

	- Android treats `Padding` as space around the scrollable area, rather than inside of it. 
	- Android's native measurements will not account for our virtual `Margin` when measuring ScrollView content. 
	
So again, we insert an intermediate `ContentViewGroup` to handle these problems. The `ContentViewGroup` is laid out at (0, 0) in the MauiScrollView; it handles the `Padding` and `Margin` behaviors for us, and initiates `CrossPlatformMeasure()` and `CrossPlatformArrange()` for its `Content`. 

Another note: the content of an Android ScrollView does not stretch to fill the viewport by default. That is, if you have a ScrollView which fills the screen and the content of the ScrollView is smaller than the screen, by default that content will not expand to take up the entire viewport (the behavior we expect for .NET MAUI). On Android, we can achieve the behavior we expect by setting the `FillViewport` property to `true` for the native ScrollView. This is all handled automatically by the Android ScrollViewHandler; I note it here because this causes an extra measure pass when the content is smaller than the ScrollView's viewport and the virtual ScrollView has layout alignment set to `Fill`. This is all explained in the comments for the ScrollViewHandler's `GetDesiredSize()` override, but I'm calling it out here as well in case anyone is investigating the number of measure calls being made. 

## iOS

The default iOS ScrollView behavior is actually pretty close to what we want for .NET MAUI, but we still use an intermediate ContentView because it gives us a way to invoke the `CrossPlatformMeasure()` and `CrossPlatformArrange()` methods of the ScrollView `Content`. 
