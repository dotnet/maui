using System;
using Android.App;
using Android.Runtime;

namespace Microsoft.Maui.Core.DeviceTests
{
	[Application]
	public class MainApplication : MauiApplication<Startup>
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}