using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]
[assembly: Xamarin.Forms.ResolutionGroupName (Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Effects.ResolutionGroupName)]