using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Platform
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
