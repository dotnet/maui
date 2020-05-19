using System.Reflection;
using System.Runtime.InteropServices;
using Android.App;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls;

[assembly: AssemblyTitle("System.Maui.ControlGallery.Android")]
[assembly: ComVisible(false)]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

[assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "AIzaSyAdstcJQswxEjzX5YjLaMcu2aRVEBJw39Y")]
[assembly: System.Maui.ResolutionGroupName(System.Maui.Controls.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]