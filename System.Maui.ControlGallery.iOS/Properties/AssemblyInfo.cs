using System.Reflection;
using System.Runtime.InteropServices;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls;
using System.Maui.Internals;

[assembly: AssemblyTitle ("System.Maui.ControlGallery.iOS")]
[assembly: ComVisible (false)]
[assembly: Guid ("5098d081-687d-442c-9f92-77fa599779f9")]
[assembly: System.Maui.ResolutionGroupName (System.Maui.Controls.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: ExportRenderer(typeof(WkWebView), typeof(System.Maui.Platform.iOS.WkWebViewRenderer))]
[assembly: Preserve]

