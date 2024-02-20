#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Microsoft.Maui.ApplicationModel
{
	static class PlatformUtils
	{
		internal const int requestCodeFilePicker = 11001;
		internal const int requestCodeMediaPicker = 11002;
		internal const int requestCodeMediaCapture = 11003;
		internal const int requestCodePickContact = 11004;

		internal const int requestCodeStart = 12000;

		static int requestCode = requestCodeStart;

		internal static int NextRequestCode()
		{
			if (++requestCode >= 12999)
				requestCode = requestCodeStart;

			return requestCode;
		}

		internal static Intent? RegisterBroadcastReceiver(BroadcastReceiver? receiver, IntentFilter filter)
		{
#if ANDROID34_0_OR_GREATER
			if (OperatingSystem.IsAndroidVersionAtLeast(34))
			{
				return Application.Context.RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
			}
#endif
			return Application.Context.RegisterReceiver(receiver, filter);
		}

		internal static bool HasSystemFeature(string systemFeature)
		{
			var packageManager = Application.Context.PackageManager;
			if (packageManager is not null)
			{
				foreach (var feature in packageManager.GetSystemAvailableFeatures())
				{
					if (feature?.Name?.Equals(systemFeature, StringComparison.OrdinalIgnoreCase) ?? false)
						return true;
				}
			}
			return false;
		}

		internal static bool IsIntentSupported(Intent intent)
		{
			if (Application.Context is not Context ctx || ctx.PackageManager is not PackageManager pm)
				return false;

			return intent.ResolveActivity(pm) is not null;
		}

		internal static bool IsIntentSupported(Intent intent, string expectedPackageName)
		{
			if (Application.Context is not Context ctx || ctx.PackageManager is not PackageManager pm)
				return false;

			return intent.ResolveActivity(pm) is ComponentName c && c.PackageName == expectedPackageName;
		}
	}
}
