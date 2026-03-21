using System;
using System.IO;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;
using AActivity = Android.App.Activity;
using AApplicationInfoFlags = Android.Content.PM.ApplicationInfoFlags;
using AAttribute = Android.Resource.Attribute;
using AColor = Android.Graphics.Color;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public static class ContextExtensions
	{
		// Caching this display density here means that all pixel calculations are going to be based on the density
		// of the first Context these extensions are run against. That's probably fine, but if we run into a 
		// situation where subsequent activities can be launched with a different display density from the initial
		// activity, we'll need to remove this cached value or cache it in a Dictionary<Context, float>
		static float s_displayDensity = float.MinValue;

		static int? _actionBarHeight;
		static int? _statusBarHeight;
		static int? _navigationBarHeight;
		// TODO FromPixels/ToPixels is both not terribly descriptive and also possibly sort of inaccurate?
		// These need better names. It's really To/From Device-Independent, but that doesn't exactly roll off the tongue.

		internal static double FromPixels(this View view, double pixels)
		{
			EnsureMetrics(view);

			return FromPixelsUsingMetrics(pixels);
		}

		public static double FromPixels(this Context? self, double pixels)
		{
			EnsureMetrics(self);

			return FromPixelsUsingMetrics(pixels);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double FromPixelsUsingMetrics(double pixels)
		{
			return pixels / s_displayDensity;
		}

		internal static Size FromPixels(this View view, double width, double height)
		{
			return new Size(view.FromPixels(width), view.FromPixels(height));
		}

		public static Size FromPixels(this Context context, double width, double height)
		{
			return new Size(context.FromPixels(width), context.FromPixels(height));
		}

		public static Thickness FromPixels(this Context context, Thickness thickness) =>
			new Thickness(
				context.FromPixels(thickness.Left),
				context.FromPixels(thickness.Top),
				context.FromPixels(thickness.Right),
				context.FromPixels(thickness.Bottom));

		public static Rect FromPixels(this Context context, Rect rect) =>
			new Rect(
				context.FromPixels(rect.X),
				context.FromPixels(rect.Y),
				context.FromPixels(rect.Width),
				context.FromPixels(rect.Height));

		internal static Rect FromPixels(this Context context, global::Android.Graphics.Rect rect) =>
			new Rect(
				context.FromPixels(rect.Left),
				context.FromPixels(rect.Top),
				context.FromPixels(rect.Width()),
				context.FromPixels(rect.Height()));

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

		internal static float ToPixels(this View view, double dp)
		{
			EnsureMetrics(view);

			return ToPixelsUsingMetrics(dp);
		}

		public static float ToPixels(this Context? self, double dp)
		{
			EnsureMetrics(self);

			return ToPixelsUsingMetrics(dp);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float ToPixelsUsingMetrics(double dp)
		{
			// Layout arrangement in DP can cause rounding issues when converting to pixels.
			// While rounding up ensures the content is fully displayed, we must avoid rounding up unnecessarily, 
			// especially for values very close to the lower integer.
			// For example 112.00000000000001dp * 2.625 = 294.000000000000026px incorrectly rounded to 295px. 
			// To address this, we subtract a small Epsilon 0.0000000001px before ceiling.
			return (float)Math.Ceiling((dp * s_displayDensity) - GeometryUtil.Epsilon);
		}

		internal static (int left, int top, int right, int bottom) ToPixels(this View view, Graphics.Rect rectangle)
		{
			return
			(
				(int)view.ToPixels(rectangle.Left),
				(int)view.ToPixels(rectangle.Top),
				(int)view.ToPixels(rectangle.Right),
				(int)view.ToPixels(rectangle.Bottom)
			);
		}

		public static (int left, int top, int right, int bottom) ToPixels(this Context context, Graphics.Rect rectangle)
		{
			return
			(
				(int)context.ToPixels(rectangle.Left),
				(int)context.ToPixels(rectangle.Top),
				(int)context.ToPixels(rectangle.Right),
				(int)context.ToPixels(rectangle.Bottom)
			);
		}

		public static Thickness ToPixels(this Context context, Thickness thickness)
		{
			return new Thickness(
				context.ToPixels(thickness.Left),
				context.ToPixels(thickness.Top),
				context.ToPixels(thickness.Right),
				context.ToPixels(thickness.Bottom));
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

		public static bool TryResolveAttribute(this Context context, int id, out float? value) =>
			context.Theme.TryResolveAttribute(id, out value);

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
							if (OperatingSystem.IsAndroidVersionAtLeast(23))
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

		static void EnsureMetrics(Context? context)
		{
			if (s_displayDensity != float.MinValue)
				return;

			context ??= global::Android.App.Application.Context;

			using (DisplayMetrics? metrics = context.Resources?.DisplayMetrics)
				s_displayDensity = metrics != null ? metrics.Density : 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void EnsureMetrics(View view)
		{
			if (s_displayDensity != float.MinValue)
				return;

			EnsureMetrics(view.Context);
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
				return activity.SupportActionBar?.ThemedContext ?? context;

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

		public static IWindow? GetWindow(this Context context)
		{
			var platformWindow = context.GetActivity();
			if (platformWindow is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach (var window in windows)
			{
				if (window.Handler?.PlatformView is AActivity activity)
				{
					if (activity == platformWindow)
						return window;
				}
			}

			return null;
		}

		internal static Color GetAccentColor(this Context context)
		{
			Color? rc = null;
			using (var value = new TypedValue())
			{
				if (context.Theme != null)
				{
					if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorAccent, value, true)) // Android 5.0+
					{
						rc = Color.FromUint((uint)value.Data);
					}
				}
			}

			return rc ?? Color.FromArgb("#ff33b5e5");
		}

		public static int GetActionBarHeight(this Context context)
		{
			_actionBarHeight ??= (int)context.GetThemeAttributePixels(Resource.Attribute.actionBarSize);
			return _actionBarHeight.Value;
		}

		internal static int GetStatusBarHeight(this Context context)
		{
			if (_statusBarHeight != null)
				return _statusBarHeight.Value;

			var resources = context.Resources;

			if (resources == null)
				return 0;

			int resourceId = resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
			{
				_statusBarHeight = resources.GetDimensionPixelSize(resourceId);
			}

			return _statusBarHeight ?? 0;
		}

		internal static int GetNavigationBarHeight(this Context context)
		{
			if (_navigationBarHeight != null)
				return _navigationBarHeight.Value;

			var resources = context.Resources;

			if (resources == null)
				return 0;

			int resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
			if (resourceId > 0)
			{
				_navigationBarHeight = resources.GetDimensionPixelSize(resourceId);
			}

			return _navigationBarHeight ?? 0;
		}

		internal static int CreateMeasureSpec(this Context context, double constraint, double explicitSize, double minimumSize, double maximumSize)
		{
			var mode = MeasureSpecMode.AtMost;

			if (IsExplicitSet(explicitSize))
			{
				// We have a set value (i.e., a Width or Height)
				mode = MeasureSpecMode.Exactly;

				// Since the mode is "Exactly", we have to return the exact final value clamped to the minimum/maximum.
				constraint = Math.Max(explicitSize, ResolveMinimum(minimumSize));

				if (IsMaximumSet(maximumSize))
				{
					constraint = Math.Min(constraint, maximumSize);
				}
			}
			else if (IsMaximumSet(maximumSize) && maximumSize < constraint)
			{
				mode = MeasureSpecMode.AtMost;
				constraint = maximumSize;
			}
			else if (double.IsInfinity(constraint))
			{
				// We've got infinite space; we'll leave the size up to the platform control
				mode = MeasureSpecMode.Unspecified;
				constraint = 0;
			}

			// Convert to a platform size to create the spec for measuring
			var deviceConstraint = (int)context.ToPixels(constraint);

			return mode.MakeMeasureSpec(deviceConstraint);
		}

		public static float GetDisplayDensity(this Context? context)
		{
			EnsureMetrics(context);

			return s_displayDensity;
		}

		public static Rect ToCrossPlatformRectInReferenceFrame(this Context context, int left, int top, int right, int bottom)
		{
			var deviceIndependentLeft = context.FromPixels(left);
			var deviceIndependentTop = context.FromPixels(top);
			var deviceIndependentRight = context.FromPixels(right);
			var deviceIndependentBottom = context.FromPixels(bottom);

			return Rect.FromLTRB(0, 0,
				deviceIndependentRight - deviceIndependentLeft, deviceIndependentBottom - deviceIndependentTop);
		}

		internal static bool IsDestroyed(this Context? context)
		{
			if (context == null)
				return true;

			if (context.GetActivity() is FragmentActivity fa)
			{
				if (fa.IsDisposed())
					return true;

				var stateCheck = AndroidX.Lifecycle.Lifecycle.State.Destroyed;

				if (stateCheck != null &&
					fa.Lifecycle.CurrentState == stateCheck)
				{
					return true;
				}

				if (fa.IsDestroyed)
					return true;
			}

			return context.IsDisposed();
		}

		internal static bool IsPlatformContextDestroyed(this IElementHandler? handler)
		{
			var context = handler?.MauiContext?.Context;
			return context.IsDestroyed();
		}
	}
}