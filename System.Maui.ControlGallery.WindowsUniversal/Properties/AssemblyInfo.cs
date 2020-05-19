using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls;
using System.Maui.Platform.UWP;

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: System.Maui.ResolutionGroupName (System.Maui.Controls.Issues.Effects.ResolutionGroupName)]