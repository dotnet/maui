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

		internal static bool HasSystemFeature(string systemFeature)
		{
			var packageManager = Application.Context.PackageManager;
			foreach (var feature in packageManager.GetSystemAvailableFeatures())
			{
				if (feature?.Name?.Equals(systemFeature, StringComparison.OrdinalIgnoreCase) ?? false)
					return true;
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

		internal static Java.Util.Locale GetLocale()
		{
			var resources = Application.Context.Resources;
			var config = resources.Configuration;

#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				return config.Locales.Get(0);
#endif

#pragma warning disable CS0618 // Type or member is obsolete
			return config.Locale;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		internal static void SetLocale(Java.Util.Locale locale)
		{
			Java.Util.Locale.Default = locale;
			var resources = Application.Context.Resources;
			var config = resources.Configuration;

#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				config.SetLocale(locale);
			else
#endif
#pragma warning disable CS0618 // Type or member is obsolete
				config.Locale = locale;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			resources.UpdateConfiguration(config, resources.DisplayMetrics);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
