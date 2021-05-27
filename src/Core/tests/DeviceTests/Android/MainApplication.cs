#if NET6_0_OR_GREATER
using System;
using Android.App;
using Android.Runtime;

namespace Microsoft.Maui.DeviceTests
{
	[Application]
	public class MainApplication : Application
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		// TODO: https://github.com/dotnet/runtime/issues/51274
		public override void OnCreate()
		{
			Java.Lang.JavaSystem.LoadLibrary("System.Security.Cryptography.Native.OpenSsl");

			base.OnCreate();
		}
	}
}
#endif