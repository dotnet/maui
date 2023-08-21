// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Android.OS;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static volatile Handler handler;

		static bool PlatformIsMainThread
		{
			get
			{
				if (OperatingSystem.IsAndroidVersionAtLeast(23))
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
