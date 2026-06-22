#nullable enable
using System;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AGraphics = Android.Graphics;
using AView = Android.Views.View;
using AWindow = Android.Views.Window;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AndroidSystemChrome
	{
		static readonly ConditionalWeakTable<AppBarLayout, OriginalAppBarBackground> s_originalAppBarBackgrounds = new();

		internal static void UpdateTopChrome(AView? chromeView, Brush? background)
		{
			if (chromeView is null)
			{
				return;
			}

			var appBarLayout = chromeView.GetParentOfType<AppBarLayout>();
			UpdateAppBarBackground(appBarLayout, background);
			UpdateSystemBarAppearance(
				chromeView.Context,
				window: null,
				updateStatusBar: true,
				updateNavigationBar: false,
				statusBarBackgroundColor: GetChromeColor(background, ChromeEdge.Top));
		}

		internal static void UpdateBottomChrome(AView? chromeView, Brush? background)
		{
			if (chromeView is null)
			{
				return;
			}

			UpdateSystemBarAppearance(
				chromeView.Context,
				window: null,
				updateStatusBar: false,
				updateNavigationBar: true,
				navigationBarBackgroundColor: GetChromeColor(background, ChromeEdge.Bottom));
		}

		internal static void UpdateWindowChrome(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Paint? background = null)
		{
			UpdateSystemBarAppearance(
				context,
				window,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor: GetChromeColor(background, ChromeEdge.Top),
				navigationBarBackgroundColor: GetChromeColor(background, ChromeEdge.Bottom));
		}

		internal static void UpdateWindowChrome(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Brush? statusBarBackground,
			Paint? navigationBarBackground = null)
		{
			UpdateSystemBarAppearance(
				context,
				window,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor: GetChromeColor(statusBarBackground, ChromeEdge.Top),
				navigationBarBackgroundColor: GetChromeColor(navigationBarBackground, ChromeEdge.Bottom));
		}

		static void UpdateSystemBarAppearance(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor = null,
			Color? navigationBarBackgroundColor = null)
		{
			var activity = context?.GetActivity();
			if (window is null)
			{
				window = activity?.Window;
			}

			window.UpdateSystemBarAppearance(
				activity,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor,
				navigationBarBackgroundColor);
		}

		static void UpdateAppBarBackground(AppBarLayout? appBarLayout, Brush? background)
		{
			if (appBarLayout is null)
			{
				return;
			}

			var originalBackground = s_originalAppBarBackgrounds.GetValue(
				appBarLayout,
				static appBar => new OriginalAppBarBackground(appBar.Background));

			if (Brush.IsNullOrEmpty(background))
			{
				ViewCompat.SetBackgroundTintMode(appBarLayout, null);
				ViewCompat.SetBackgroundTintList(appBarLayout, null);
				appBarLayout.Background = originalBackground.CreateDrawable();
				return;
			}

			if (background is SolidColorBrush { Color: not null } solidColorBrush)
			{
				appBarLayout.Background = originalBackground.CreateDrawable() ?? new ColorDrawable(AGraphics.Color.Transparent);
				ViewCompat.SetBackgroundTintMode(appBarLayout, AGraphics.PorterDuff.Mode.Src);
				ViewCompat.SetBackgroundTintList(appBarLayout, ColorStateList.ValueOf(solidColorBrush.Color.ToPlatform()));
				return;
			}

			ViewCompat.SetBackgroundTintMode(appBarLayout, null);
			ViewCompat.SetBackgroundTintList(appBarLayout, null);
			appBarLayout.UpdateBackground(background);
		}

		static Color? GetChromeColor(Brush? background, ChromeEdge edge)
		{
			return background switch
			{
				SolidColorBrush { Color: { Alpha: > 0 } color } => color,
				LinearGradientBrush linearGradientBrush => GetGradientColorAt(
					linearGradientBrush.GradientStops,
					GetLinearGradientOffset(linearGradientBrush.StartPoint, linearGradientBrush.EndPoint, edge)),
				RadialGradientBrush radialGradientBrush => GetGradientColorAt(
					radialGradientBrush.GradientStops,
					GetRadialGradientOffset(radialGradientBrush.Center, radialGradientBrush.Radius, edge)),
				_ => null
			};
		}

		static Color? GetChromeColor(Paint? background, ChromeEdge edge)
		{
			return background switch
			{
				SolidPaint { Color: { Alpha: > 0 } color } => color,
				LinearGradientPaint linearGradientPaint => GetGradientColorAt(
					linearGradientPaint.GradientStops,
					GetLinearGradientOffset(linearGradientPaint.StartPoint, linearGradientPaint.EndPoint, edge)),
				RadialGradientPaint radialGradientPaint => GetGradientColorAt(
					radialGradientPaint.GradientStops,
					GetRadialGradientOffset(radialGradientPaint.Center, radialGradientPaint.Radius, edge)),
				_ => null
			};
		}

		static Color? GetGradientColorAt(GradientStopCollection? gradientStops, float offset)
		{
			if (gradientStops is null || gradientStops.Count == 0)
			{
				return null;
			}

			GradientStop? before = null;
			GradientStop? after = null;

			foreach (var gradientStop in gradientStops)
			{
				if (gradientStop is null || gradientStop.Color is null)
				{
					continue;
				}

				if (gradientStop.Offset <= offset && (before is null || gradientStop.Offset >= before.Offset))
				{
					before = gradientStop;
				}

				if (gradientStop.Offset >= offset && (after is null || gradientStop.Offset <= after.Offset))
				{
					after = gradientStop;
				}
			}

			return GetGradientColorAt(before?.Offset, before?.Color, after?.Offset, after?.Color, offset);
		}

		static Color? GetGradientColorAt(PaintGradientStop[]? gradientStops, float offset)
		{
			if (gradientStops is null || gradientStops.Length == 0)
			{
				return null;
			}

			PaintGradientStop? before = null;
			PaintGradientStop? after = null;

			foreach (var gradientStop in gradientStops)
			{
				if (gradientStop is null || gradientStop.Color is null)
				{
					continue;
				}

				if (gradientStop.Offset <= offset && (before is null || gradientStop.Offset >= before.Offset))
				{
					before = gradientStop;
				}

				if (gradientStop.Offset >= offset && (after is null || gradientStop.Offset <= after.Offset))
				{
					after = gradientStop;
				}
			}

			return GetGradientColorAt(before?.Offset, before?.Color, after?.Offset, after?.Color, offset);
		}

		static Color? GetGradientColorAt(float? beforeOffset, Color? beforeColor, float? afterOffset, Color? afterColor, float offset)
		{
			Color? color = null;

			if (beforeOffset.HasValue && beforeColor is not null && afterOffset.HasValue && afterColor is not null)
			{
				color = beforeOffset == afterOffset
					? beforeColor
					: BlendColors(beforeColor, afterColor, (offset - beforeOffset.Value) / (afterOffset.Value - beforeOffset.Value));
			}
			else if (beforeColor is not null)
			{
				color = beforeColor;
			}
			else if (afterColor is not null)
			{
				color = afterColor;
			}

			return color is { Alpha: > 0 } ? color : null;
		}

		static Color BlendColors(Color startColor, Color endColor, float factor)
		{
			factor = Math.Clamp(factor, 0f, 1f);

			return new Color(
				startColor.Red + ((endColor.Red - startColor.Red) * factor),
				startColor.Green + ((endColor.Green - startColor.Green) * factor),
				startColor.Blue + ((endColor.Blue - startColor.Blue) * factor),
				startColor.Alpha + ((endColor.Alpha - startColor.Alpha) * factor));
		}

		static float GetLinearGradientOffset(Point startPoint, Point endPoint, ChromeEdge edge)
		{
			var samplePoint = GetEdgeSamplePoint(edge);
			var x = endPoint.X - startPoint.X;
			var y = endPoint.Y - startPoint.Y;
			var lengthSquared = (x * x) + (y * y);

			if (lengthSquared == 0)
			{
				return 0;
			}

			return (float)Math.Clamp(
				(((samplePoint.X - startPoint.X) * x) + ((samplePoint.Y - startPoint.Y) * y)) / lengthSquared,
				0,
				1);
		}

		static float GetRadialGradientOffset(Point center, double radius, ChromeEdge edge)
		{
			if (radius <= 0)
			{
				return 0;
			}

			var samplePoint = GetEdgeSamplePoint(edge);
			var x = samplePoint.X - center.X;
			var y = samplePoint.Y - center.Y;

			return (float)Math.Clamp(Math.Sqrt((x * x) + (y * y)) / radius, 0, 1);
		}

		static Point GetEdgeSamplePoint(ChromeEdge edge)
		{
			return edge == ChromeEdge.Top
				? new Point(0.5, 0)
				: new Point(0.5, 1);
		}

		enum ChromeEdge
		{
			Top,
			Bottom
		}

		sealed class OriginalAppBarBackground
		{
			readonly Drawable? _backgroundTemplate;

			public OriginalAppBarBackground(Drawable? background)
			{
				_backgroundTemplate = CreateDrawable(background);
			}

			public Drawable? CreateDrawable()
			{
				return CreateDrawable(_backgroundTemplate);
			}

			static Drawable? CreateDrawable(Drawable? background)
			{
				if (!background.IsAlive())
				{
					return null;
				}

				var constantState = background.GetConstantState();
				if (!constantState.IsAlive())
				{
					return null;
				}

				return constantState.NewDrawable()?.Mutate();
			}
		}
	}
}
