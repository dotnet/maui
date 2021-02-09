using System;
using Foundation;

namespace Xamarin.Essentials
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			NSThread.Current.IsMainThread;

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
		}

		internal static T InvokeOnMainThread<T>(Func<T> factory)
		{
			T value = default;
			NSRunLoop.Main.InvokeOnMainThread(() => value = factory());
			return value;
		}
	}
}
