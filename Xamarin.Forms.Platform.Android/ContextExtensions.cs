using System;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Util;
using Android.Views.InputMethods;
using AApplicationInfoFlags = Android.Content.PM.ApplicationInfoFlags;

namespace Xamarin.Forms.Platform.Android
{
	public static class ContextExtensions
	{
		// Caching this display density here means that all pixel calculations are going to be based on the density
		// of the first Context these extensions are run against. That's probably fine, but if we run into a 
		// situation where subsequent activities can be launched with a different display density from the intial
		// activity, we'll need to remove this cached value or cache it in a Dictionary<Context, float>
		static float s_displayDensity = float.MinValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double FromPixels(this Context self, double pixels)
		{
			SetupMetrics(self);

			return pixels / s_displayDensity;
		}

		public static void HideKeyboard(this Context self, global::Android.Views.View view)
		{
			var service = (InputMethodManager)self.GetSystemService(Context.InputMethodService);
			// Can happen in the context of the Android Designer
			if (service != null)
				service.HideSoftInputFromWindow(view.WindowToken, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToPixels(this Context self, double dp)
		{
			SetupMetrics(self);

			return (float)Math.Round(dp * s_displayDensity);
		}

		public static bool HasRtlSupport(this Context self) =>
			(self.ApplicationInfo.Flags & AApplicationInfoFlags.SupportsRtl) == AApplicationInfoFlags.SupportsRtl;

		public static int TargetSdkVersion(this Context self) =>
			(int)self.ApplicationInfo.TargetSdkVersion;

		internal static double GetThemeAttributeDp(this Context self, int resource)
		{
			using (var value = new TypedValue())
			{
				if (!self.Theme.ResolveAttribute(resource, value, true))
					return -1;

				var pixels = (double)TypedValue.ComplexToDimension(value.Data, self.Resources.DisplayMetrics);

				return self.FromPixels(pixels);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void SetupMetrics(Context context)
		{
			if (s_displayDensity != float.MinValue)
				return;

			using (DisplayMetrics metrics = context.Resources.DisplayMetrics)
				s_displayDensity = metrics.Density;
		}
	}
}
