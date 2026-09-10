#nullable enable
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using Google.Android.Material.Shape;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AGraphics = Android.Graphics;
using AView = Android.Views.View;
using AWindow = Android.Views.Window;
using Android.Views;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AndroidSystemChrome
	{
		static readonly ConditionalWeakTable<AppBarLayout, OriginalAppBarBackground> s_originalAppBarBackgrounds = new();
		static readonly ConditionalWeakTable<AppBarLayout, GradientAppBarBackground> s_gradientAppBarBackgrounds = new();
		static readonly ConditionalWeakTable<AView, PendingBottomChromeUpdate> s_pendingBottomChromeUpdates = new();

		internal static void UpdateTopChrome(AView? chromeView, Brush? background)
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
			{
				return;
			}

			if (chromeView is null)
			{
				return;
			}

			var appBarLayout = chromeView.GetParentOfType<AppBarLayout>();
			UpdateAppBarBackground(appBarLayout, background);
			UpdateSystemBarAppearance(
				chromeView.Context,
				GetActivityWindowForView(chromeView),
				updateStatusBar: true,
				updateNavigationBar: false,
				statusBarBackgroundColor: GetChromeColor(background, ChromeEdge.Top),
				resolveActivityWindow: false);
		}

		internal static void UpdateBottomChrome(AView? chromeView, Brush? background)
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
			{
				return;
			}

			if (chromeView is null)
			{
				return;
			}

			var window = GetActivityWindowForView(chromeView);
			if (window is null && !chromeView.IsAttachedToWindow)
			{
				s_pendingBottomChromeUpdates
					.GetValue(chromeView, static view => new PendingBottomChromeUpdate(view))
					.Update(background);
				return;
			}

			if (s_pendingBottomChromeUpdates.TryGetValue(chromeView, out var pendingUpdate))
			{
				pendingUpdate.Cancel();
				s_pendingBottomChromeUpdates.Remove(chromeView);
			}

			UpdateSystemBarAppearance(
				chromeView.Context,
				window,
				updateStatusBar: false,
				updateNavigationBar: true,
				navigationBarBackgroundColor: GetChromeColor(background, ChromeEdge.Bottom),
				resolveActivityWindow: false);
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
			Color? navigationBarBackgroundColor = null,
			bool resolveActivityWindow = true)
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
			{
				return;
			}

			var activity = context?.GetActivity();
			if (window is null && resolveActivityWindow)
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

		static AWindow? GetActivityWindowForView(AView view)
		{
			var activityWindow = view.Context?.GetActivity()?.Window;
			if (activityWindow?.DecorView?.RootView is not { } activityRootView ||
				view.RootView is not { } viewRootView ||
				!viewRootView.Equals(activityRootView))
			{
				return null;
			}

			return activityWindow;
		}

		sealed class PendingBottomChromeUpdate
		{
			readonly WeakReference<AView> _view;
			Brush? _background;
			IDisposable? _loadedSubscription;
			bool _canceled;

			public PendingBottomChromeUpdate(AView view)
			{
				_view = new(view);
			}

			public void Update(Brush? background)
			{
				_background = background;

				if (_loadedSubscription is not null || !_view.TryGetTarget(out var view))
				{
					return;
				}

				_loadedSubscription = view.OnLoaded(Apply);
			}

			public void Cancel()
			{
				_canceled = true;
				_loadedSubscription?.Dispose();
				_loadedSubscription = null;
			}

			void Apply()
			{
				if (_canceled || !_view.TryGetTarget(out var view))
				{
					return;
				}

				s_pendingBottomChromeUpdates.Remove(view);
				Cancel();
				UpdateBottomChrome(view, _background);
			}
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

			RemoveGradientAppBarBackground(appBarLayout);
			ViewCompat.SetBackgroundTintMode(appBarLayout, null);
			ViewCompat.SetBackgroundTintList(appBarLayout, null);
			if (Brush.IsNullOrEmpty(background))
			{
				appBarLayout.Background = originalBackground.CreateDrawable();
				return;
			}

			if (background is SolidColorBrush { Color: not null } solidColorBrush)
			{
				if (RuntimeFeature.IsMaterial3Enabled && appBarLayout.Background is MaterialShapeDrawable materialShapeDrawable)
				{
					var platformColor = solidColorBrush.Color.ToPlatform();
					materialShapeDrawable.FillColor = ColorStateList.ValueOf(platformColor);
					appBarLayout.SetLiftOnScrollColor(
						ColorStateList.ValueOf(LightenColor(solidColorBrush.Color, 0.3f).ToPlatform()));
				}
				else
				{
					appBarLayout.Background = originalBackground.CreateDrawable() ?? new ColorDrawable(AGraphics.Color.Transparent);
					ViewCompat.SetBackgroundTintMode(appBarLayout, AGraphics.PorterDuff.Mode.Src);
					ViewCompat.SetBackgroundTintList(appBarLayout, ColorStateList.ValueOf(solidColorBrush.Color.ToPlatform()));
				}

				return;
			}
			else
			{
				if (background is LinearGradientBrush linearGradientBrush &&
				linearGradientBrush.GradientStops.Count > 0)
				{
					var stops = linearGradientBrush.GradientStops
						.OrderBy(x => x.Offset)
						.ToArray();
					var normalColors = stops
						.Select(x => x.Color.ToPlatform().ToArgb())
						.ToArray();
					var liftedColors = stops
						.Select(x => LightenColor(x.Color, 0.3f).ToPlatform().ToArgb())
						.ToArray();
					var offsets = stops
						.Select(x => (float)x.Offset)
						.ToArray();

					var gradientDrawable = new GradientDrawable();
					gradientDrawable.SetOrientation(
						GetGradientOrientation(
							linearGradientBrush.StartPoint,
							linearGradientBrush.EndPoint));
					SetGradientColors(gradientDrawable, normalColors, offsets);
					appBarLayout.Background = gradientDrawable;

					if (RuntimeFeature.IsMaterial3Enabled)
					{
						s_gradientAppBarBackgrounds.Add(
							appBarLayout,
							new GradientAppBarBackground(appBarLayout, gradientDrawable, normalColors, liftedColors, offsets));
					}

					return;
				}
				else if (background is RadialGradientBrush radialGradientBrush &&
				radialGradientBrush.GradientStops.Count > 0)
				{
				}
			}

			if (!RuntimeFeature.IsMaterial3Enabled)
			{
				appBarLayout.UpdateBackground(background);
			}
		}

		static void RemoveGradientAppBarBackground(AppBarLayout appBarLayout)
		{
			if (s_gradientAppBarBackgrounds.TryGetValue(appBarLayout, out var gradientBackground))
			{
				gradientBackground.Dispose();
				s_gradientAppBarBackgrounds.Remove(appBarLayout);
			}
		}

		static void SetGradientColors(GradientDrawable gradientDrawable, int[] colors, float[] offsets)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				gradientDrawable.SetColors(colors, offsets);
			}
			else
			{
				gradientDrawable.SetColors(colors);
			}
		}

		static GradientDrawable.Orientation? GetGradientOrientation(
			Point startPoint,
			Point endPoint)
		{
			var dx = endPoint.X - startPoint.X;
			var dy = endPoint.Y - startPoint.Y;

			if (Math.Abs(dx) >= Math.Abs(dy))
			{
				return dx >= 0
					? GradientDrawable.Orientation.LeftRight
					: GradientDrawable.Orientation.RightLeft;
			}

			return dy >= 0
				? GradientDrawable.Orientation.TopBottom
				: GradientDrawable.Orientation.BottomTop;
		}

		static Color LightenColor(Color color, float factor)
		{
			factor = Math.Clamp(factor, 0f, 1f);

			return new Color(
				color.Red + ((1f - color.Red) * factor),
				color.Green + ((1f - color.Green) * factor),
				color.Blue + ((1f - color.Blue) * factor),
				color.Alpha
			);
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

		static int BlendArgbColors(int startColor, int endColor, float factor)
		{
			factor = Math.Clamp(factor, 0f, 1f);

			return AGraphics.Color.Argb(
				BlendColorComponent(startColor >> 24, endColor >> 24, factor),
				BlendColorComponent(startColor >> 16, endColor >> 16, factor),
				BlendColorComponent(startColor >> 8, endColor >> 8, factor),
				BlendColorComponent(startColor, endColor, factor)).ToArgb();
		}

		static int BlendColorComponent(int startColor, int endColor, float factor)
		{
			return (int)(((startColor & 0xff) + (((endColor & 0xff) - (startColor & 0xff)) * factor)));
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

		sealed class GradientAppBarBackground : Java.Lang.Object, ViewTreeObserver.IOnScrollChangedListener
		{
			readonly AppBarLayout _appBarLayout;
			readonly GradientDrawable _gradientDrawable;
			readonly int[] _normalColors;
			readonly int[] _liftedColors;
			readonly float[] _offsets;

			ValueAnimator? _animator;
			bool _wasLifted;

			public GradientAppBarBackground(
				AppBarLayout appBarLayout,
				GradientDrawable gradientDrawable,
				int[] normalColors,
				int[] liftedColors,
				float[] offsets)
			{
				_appBarLayout = appBarLayout;
				_gradientDrawable = gradientDrawable;
				_normalColors = normalColors;
				_liftedColors = liftedColors;
				_offsets = offsets;

				_wasLifted = _appBarLayout.IsLifted;

				_appBarLayout.ViewTreeObserver?.AddOnScrollChangedListener(this);
			}

			bool _isLiftedTarget;

			public void OnScrollChanged()
			{
				bool currentLifted = _appBarLayout.IsLifted;

				if (_wasLifted != currentLifted)
				{
					_wasLifted = currentLifted;
					AnimateGradient(currentLifted);
				}
			}

			void AnimateGradient(bool toLifted)
			{
				if (_animator != null && _animator.IsRunning && _isLiftedTarget == toLifted)
				{
					return;
				}

				_isLiftedTarget = toLifted;
				_animator?.Cancel();

				float start = _animator != null ? (float)(_animator.AnimatedValue ?? 0f) : (toLifted ? 0f : 1f);
				float end = toLifted ? 1f : 0f;

				_animator = ValueAnimator.OfFloat(start, end);
				_animator?.SetDuration(250);

				_animator?.Update += (s, e) =>
				{
					float progress = (float)(e?.Animation.AnimatedValue ?? 0f);
					var currentColors = new int[_normalColors.Length];

					for (int i = 0; i < currentColors.Length; i++)
					{
						currentColors[i] = AndroidSystemChrome.BlendArgbColors(_normalColors[i], _liftedColors[i], progress);
					}

					AndroidSystemChrome.SetGradientColors(_gradientDrawable, currentColors, _offsets);
					_gradientDrawable.InvalidateSelf();
				};

				_animator?.Start();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_animator?.Cancel();
					_animator?.Dispose();
					_animator = null;

					if (_appBarLayout.ViewTreeObserver != null && _appBarLayout.ViewTreeObserver.IsAlive)
					{
						_appBarLayout.ViewTreeObserver.RemoveOnScrollChangedListener(this);
					}
				}

				base.Dispose(disposing);
			}
		}
	}
}
