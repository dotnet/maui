using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Internals;

[assembly: ResolutionGroupName(Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: ExportRenderer(typeof(WkWebView), typeof(WkWebViewRenderer))]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: Preserve]

