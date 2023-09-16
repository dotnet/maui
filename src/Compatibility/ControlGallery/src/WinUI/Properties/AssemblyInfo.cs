using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.WinUI;

// Deliberately broken image source and handler so we can test handling of image loading errors
// [assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: Microsoft.Maui.Controls.ResolutionGroupName(Microsoft.Maui.Controls.ControlGallery.Issues.Effects.ResolutionGroupName)]