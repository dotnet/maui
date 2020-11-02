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

// For debug purposes. Probably won't be needed once we get to 16-9
// https://github.com/xamarin/xamarin-android/issues/5247#issuecomment-719481786
#if DEBUG
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
#endif
[assembly: ExportRenderer(typeof(Map), typeof(MapRenderer))]
[assembly: Preserve]