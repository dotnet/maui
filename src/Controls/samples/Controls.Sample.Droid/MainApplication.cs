using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Droid
{
	[Application]
	public class MainApplication : MauiApplication<Startup>
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

#if NET6_0_OR_GREATER
		// TODO: https://github.com/dotnet/runtime/issues/51274
		public override void OnCreate()
		{
			Java.Lang.JavaSystem.LoadLibrary("System.Security.Cryptography.Native.OpenSsl");

			base.OnCreate();
		}
#endif
	}
}