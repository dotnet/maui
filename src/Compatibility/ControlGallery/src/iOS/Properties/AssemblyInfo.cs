using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: AssemblyTitle ("Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS")]
[assembly: ComVisible (false)]
[assembly: Guid ("5098d081-687d-442c-9f92-77fa599779f9")]
[assembly: ResolutionGroupName (Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: ExportRenderer(typeof(WkWebView), typeof(WkWebViewRenderer))]
[assembly: Preserve]

