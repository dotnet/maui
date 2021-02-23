using System;
using Android.App;
using Android.Runtime;
using Xamarin.Platform;

namespace Maui.Controls.Sample.Droid
{
	[Application]
	public class MainApplication : MauiApplication<MyApp>
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
		{
		}
	}
}