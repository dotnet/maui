using System;
using System.IO;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views.InputMethods;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AActivity = Android.App.Activity;
using AApplicationInfoFlags = Android.Content.PM.ApplicationInfoFlags;
using AAttribute = Android.Resource.Attribute;
using AColor = Android.Graphics.Color;
using AFragmentManager = AndroidX.Fragment.App.FragmentManager;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui
{
	public static class ContextExtensions
	{
		// Caching this display density here means that all pixel calculations are going to be based on the density
		// of the first Context these extensions are run against. That's probably fine, but if we run into a 
		// situation where subsequent activities can be launched with a different display density from the intial
		// activity, we'll need to remove this cached value or cache it in a Dictionary<Context, float>
		static float s_displayDensity = float.MinValue;

		// TODO FromPixels/ToPixels is both not terribly descriptive and also possibly sort of inaccurate?
		// These need better names. It's really To/From Device-Independent, but that doesn't exactly roll off the tongue.

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double FromPixels(this Context self, double pixels)
		{
			EnsureMetrics(self);

			return pixels / s_displayDensity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size FromPixels(this Context context, double width, double height)
		{
			return new Size(context.FromPixels(width), context.FromPixels(height));
		}

		public static void HideKeyboard(this Context self, global::Android.Views.View view)
		{
			// Service may be null in the context of the Android Designer
			if (self.GetSystemService(Context.InputMethodService) is InputMethodManager service)
				service.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
		}

		public static void ShowKeyboard(this Context self, global::Android.Views.View view)
		{
			// Can happen in the context of the Android Designer
			if (self.GetSystemService(Context.InputMethodService) is InputMethodManager service)
				service.ShowSoftInput(view, ShowFlags.Implicit);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToPixels(this Context self, double dp)
		{
			EnsureMetrics(self);

			return (float)Math.Ceiling(dp * s_displayDensity);
		}

		public static bool HasRtlSupport(this Context self)
		{
			if (self == null)
				return false;

			return (self.ApplicationInfo?.Flags & AApplicationInfoFlags.SupportsRtl) == AApplicationInfoFlags.SupportsRtl;
		}

		public static int? TargetSdkVersion(this Context self)
		{
			return (int?)self?.ApplicationInfo?.TargetSdkVersion;
		}

		public static double GetThemeAttributeDp(this Context self, int resource)
		{
			using (var value = new TypedValue())
			{
				if (self == null || self.Theme == null)
					return -1;

				if (!self.Theme.ResolveAttribute(resource, value, true))
					return -1;

				var pixels = (double)TypedValue.ComplexToDimension(value.Data, self.Resources?.DisplayMetrics);

				return self.FromPixels(pixels);
			}
		}

		public static double GetThemeAttributePixels(this Context self, int resource)
		{
			using (var value = new TypedValue())
			{
				if (self == null || self.Theme == null)
					return -1;

				if (!self.Theme.ResolveAttribute(resource, value, true))
					return -1;

				return (double)TypedValue.ComplexToDimension(value.Data, self.Resources?.DisplayMetrics);
			}
		}

		internal static int GetDisabledThemeAttrColor(this Context context, int attr)
		{
			if (context.Theme == null)
				return 0;

			using (var value = new TypedValue())
			{
				// Now retrieve the disabledAlpha value from the theme
				context.Theme.ResolveAttribute(AAttribute.DisabledAlpha, value, true);
				float disabledAlpha = value.Float;
				return GetThemeAttrColor(context, attr, disabledAlpha);
			}
		}

		public static bool TryResolveAttribute(this Context context, int id)
		{
			return context.Theme.TryResolveAttribute(id);
		}

		internal static int GetThemeAttrColor(this Context context, int attr)
		{
			using (TypedValue mTypedValue = new TypedValue())
			{
				if (context.Theme?.ResolveAttribute(attr, mTypedValue, true) == true)
				{
					if (mTypedValue.Type >= DataType.FirstInt && mTypedValue.Type <= DataType.LastInt)
					{
						return mTypedValue.Data;
					}
					else if (mTypedValue.Type == DataType.String)
					{
						if (context.Resources != null)
						{
							if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
								return context.Resources.GetColor(mTypedValue.ResourceId, context.Theme);
							else
#pragma warning disable CS0618 // Type or member is obsolete
								return context.Resources.GetColor(mTypedValue.ResourceId);
#pragma warning restore CS0618 // Type or member is obsolete
						}
					}
				}
			}

			return 0;
		}

		internal static int GetThemeAttrColor(this Context context, int attr, float alpha)
		{
			int color = GetThemeAttrColor(context, attr);
			int originalAlpha = AColor.GetAlphaComponent(color);
			// Return the color, multiplying the original alpha by the disabled value
			return (color & 0x00ffffff) | ((int)Math.Round(originalAlpha * alpha) << 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void EnsureMetrics(Context context)
		{
			if (s_displayDensity != float.MinValue)
				return;

			using (DisplayMetrics? metrics = context?.Resources?.DisplayMetrics)
				s_displayDensity = metrics != null ? metrics.Density : 0;
		}

		public static AActivity? GetActivity(this Context context)
		{
			if (context == null)
				return null;

			if (context is AActivity activity)
				return activity;

			if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext?.GetActivity();

			return null;
		}

		internal static Context? GetThemedContext(this Context context)
		{
			if (context == null)
				return null;

			if (context is AppCompatActivity activity)
				return activity.SupportActionBar.ThemedContext;

			if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext?.GetThemedContext();

			return null;
		}

		public static FragmentManager? GetFragmentManager(this Context context)
		{
			if (context == null)
				return null;

			var activity = context.GetActivity();

			if (activity is FragmentActivity fa)
				return fa.SupportFragmentManager;

			return null;
		}

		public static int GetDrawableId(this Context context, string name)
		{
			if (context.Resources == null || context.PackageName == null)
				return 0;

			return context.Resources.GetDrawableId(context.PackageName, name);
		}

		public static int GetDrawableId(this Resources resources, string packageName, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return 0;

			var title = Path.GetFileNameWithoutExtension(name);

			title = title.ToLowerInvariant();

			return resources.GetIdentifier(title, "drawable", packageName);
		}
	}
}