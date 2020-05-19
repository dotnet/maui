using System.Reflection;
using System.Runtime.InteropServices;
using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls;

[assembly: AssemblyTitle("Xamarin.Forms.ControlGallery.Android")]
[assembly: ComVisible(false)]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

[assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "AIzaSyAdstcJQswxEjzX5YjLaMcu2aRVEBJw39Y")]
[assembly: Xamarin.Forms.ResolutionGroupName(Xamarin.Forms.Controls.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]