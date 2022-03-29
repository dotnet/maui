using System;
using Android.OS;

namespace Microsoft.Maui.Essentials
{
	public static partial class MainThread
	{
		static volatile Handler handler;

		static bool PlatformIsMainThread
		{
			get
			{
				if (OperatingSystem.IsAndroidVersionAtLeast((int)BuildVersionCodes.M))
#pragma warning disable CA1416 // Validate platform compatibility
					return Looper.MainLooper.IsCurrentThread;
#pragma warning restore CA1416 // Validate platform compatibility

				return Looper.MyLooper() == Looper.MainLooper;
			}
		}

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			if (handler?.Looper != Looper.MainLooper)
				handler = new Handler(Looper.MainLooper);

			handler.Post(action);
		}
	}
}
