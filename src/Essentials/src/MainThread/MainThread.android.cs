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
				if (Platform.HasApiLevel(BuildVersionCodes.M))
					return Looper.MainLooper.IsCurrentThread;

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
