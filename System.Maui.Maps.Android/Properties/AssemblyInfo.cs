using System.Reflection;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ComVisible(false)]

[assembly: UsesPermission(Manifest.Permission.Internet)]
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
[assembly: ExportRenderer(typeof (Map), typeof (MapRenderer))]
[assembly: Preserve]