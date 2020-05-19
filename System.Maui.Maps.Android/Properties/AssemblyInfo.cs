using System.Reflection;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using System.Maui;
using System.Maui.Internals;
using System.Maui.Maps;
using System.Maui.Maps.Android;

[assembly: ComVisible(false)]

[assembly: UsesPermission(Manifest.Permission.Internet)]
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
[assembly: ExportRenderer(typeof (Map), typeof (MapRenderer))]
[assembly: Preserve]