using System;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Util;
using Android.Views.InputMethods;
using AApplicationInfoFlags = Android.Content.PM.ApplicationInfoFlags;
using AActivity = Android.App.Activity;

#if __ANDROID_29__
using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;
using AFragmentManager = AndroidX.Fragment.App.FragmentManager;
#else
using AFragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Support.V4.App;
using Android.Support.V7.App;
#endif

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size FromPixels(this Context context, double width, double height)
		{
			return new Size(context.FromPixels(width), context.FromPixels(height));
		}

		public static void HideKeyboard(this Context self, global::Android.Views.View view)
		{
			var service = (InputMethodManager)self.GetSystemService(Context.InputMethodService);
			// service may be null in the context of the Android Designer
			if (service != null)
				service.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
		}

		public static void ShowKeyboard(this Context self, global::Android.Views.View view)
		{
			var service = (InputMethodManager)self.GetSystemService(Context.InputMethodService);
			// Can happen in the context of the Android Designer
			if (service != null)
				service.ShowSoftInput(view, ShowFlags.Implicit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToPixels(this Context self, double dp)
		{
			SetupMetrics(self);

			return (float)Math.Ceiling(dp * s_displayDensity);
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

		public static AActivity GetActivity(this Context context)
		{
			if (context == null)
				return null;

			if (context is AActivity activity)
				return activity;

			if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext.GetActivity();

			return null;
		}

		internal static Context GetThemedContext(this Context context)
		{
			if (context == null)
				return null;

			if (context.IsDesignerContext())
				return context;

			if (context is AppCompatActivity activity)
				return activity.SupportActionBar.ThemedContext;

			if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext.GetThemedContext();

			return null;
		}

		static bool? _isDesignerContext;
		internal static bool IsDesignerContext(this Context context)
		{
			if (_isDesignerContext.HasValue)
				return _isDesignerContext.Value;

			if (context == null)
				_isDesignerContext = false;
			else if ($"{context}".Contains("com.android.layoutlib.bridge.android.BridgeContext"))
				_isDesignerContext = true;
			else if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext.IsDesignerContext();
			else
				_isDesignerContext = false;

			return _isDesignerContext.Value;
		}

		public static AFragmentManager GetFragmentManager(this Context context)
		{
			if (context == null)
				return null;

			var activity = context.GetActivity();

			if (activity is FragmentActivity fa)
				return fa.SupportFragmentManager;

			return null;
		}
	}
}
