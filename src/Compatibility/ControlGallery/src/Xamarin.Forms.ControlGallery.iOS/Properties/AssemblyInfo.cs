using System.Reflection;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Internals;

[assembly: AssemblyTitle ("Xamarin.Forms.ControlGallery.iOS")]
[assembly: ComVisible (false)]
[assembly: Guid ("5098d081-687d-442c-9f92-77fa599779f9")]
[assembly: Xamarin.Forms.ResolutionGroupName (Xamarin.Forms.Controls.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: ExportRenderer(typeof(WkWebView), typeof(Xamarin.Forms.Platform.iOS.WkWebViewRenderer))]
[assembly: Preserve]

