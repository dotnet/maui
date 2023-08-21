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

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal enum AndroidApplicationLifecycleState
	{
		Uninitialized,
		OnCreate,
		OnStart,
		OnResume,
		OnPause,
		OnStop,
		OnRestart,
		OnDestroy
	}
}