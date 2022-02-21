using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
#nullable disable
using Android.App;
using Android.Content;

namespace Microsoft.Maui.Platform
{
	public static class ActivityResultCallbackRegistry
	{
		static readonly ConcurrentDictionary<int, Action<Result, Intent>> ActivityResultCallbacks =
			new ConcurrentDictionary<int, Action<Result, Intent>>();

		static int NextActivityResultCallbackKey;

		public static void InvokeCallback(int requestCode, Result resultCode, Intent data)
		{
			Action<Result, Intent> callback;

			if (ActivityResultCallbacks.TryGetValue(requestCode, out callback))
			{
				callback(resultCode, data);
			}
		}

		internal static int RegisterActivityResultCallback(Action<Result, Intent> callback)
		{
			int requestCode = NextActivityResultCallbackKey;

			while (!ActivityResultCallbacks.TryAdd(requestCode, callback))
			{
				NextActivityResultCallbackKey += 1;
				requestCode = NextActivityResultCallbackKey;
			}

			NextActivityResultCallbackKey += 1;

			return requestCode;
		}

		internal static void UnregisterActivityResultCallback(int requestCode)
		{
			Action<Result, Intent> callback;
			ActivityResultCallbacks.TryRemove(requestCode, out callback);
		}
	}
}