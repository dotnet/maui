//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Reflection;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Maps.Android;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;

[assembly: ComVisible(false)]

[assembly: UsesPermission(Manifest.Permission.Internet)]

// For debug purposes. Probably won't be needed once we get to 16-9
// https://github.com/xamarin/xamarin-android/issues/5247#issuecomment-719481786
#if DEBUG
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
#endif
[assembly: ExportRenderer(typeof(Map), typeof(MapRenderer))]
[assembly: Preserve]