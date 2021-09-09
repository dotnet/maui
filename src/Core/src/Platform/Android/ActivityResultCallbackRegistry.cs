using System;
using System.Collections.Concurrent;
using Android.App;
using Android.Content;

namespace Microsoft.Maui
{
	public static class ActivityResultCallbackRegistry
	{
		static readonly ConcurrentDictionary<int, Action<Result, Intent>> s_activityResultCallbacks = new();

		static int s_nextActivityResultCallbackKey;

		public static void InvokeCallback(int requestCode, Result resultCode, Intent data)
		{
			if (s_activityResultCallbacks.TryGetValue(requestCode, out Action<Result, Intent>? callback))
				callback(resultCode, data);
		}

		internal static int RegisterActivityResultCallback(Action<Result, Intent> callback)
		{
			int requestCode = s_nextActivityResultCallbackKey;

			while (!s_activityResultCallbacks.TryAdd(requestCode, callback))
			{
				s_nextActivityResultCallbackKey += 1;
				requestCode = s_nextActivityResultCallbackKey;
			}

			s_nextActivityResultCallbackKey += 1;

			return requestCode;
		}

		internal static void UnregisterActivityResultCallback(int requestCode)
		{
			_ = s_activityResultCallbacks.TryRemove(requestCode, out Action<Result, Intent>? callback);
		}
	}
}
