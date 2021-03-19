using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;

namespace MauiSampleApp.SingleProject
{
	[Application]
	public class MainApplication : MauiApplication<MyApp>
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
		{
		}
	}
}