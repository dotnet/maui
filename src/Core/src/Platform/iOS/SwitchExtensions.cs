using System;
using CoreFoundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static UIColor DefaultBackgroundColor = UIColor.FromRGBA(120, 120, 128, 40);
		const int MaxColorReapplyAttempts = 2;

		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view is null)
			{
				return;
			}

			var styleChanged = uiSwitch.UpdatePreferredStyle(view);

			uiSwitch.ApplyTrackColor(view);

			if (styleChanged)
			{
				uiSwitch.ReapplyColorsAfterStyleUpdate(view);
			}
		}

		static void ApplyTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			var uIView = GetTrackSubview(uiSwitch);

			if (uIView is null)
			{
				return;
			}

			var trackColor = view.TrackColor?.ToPlatform();

			if (view.IsOn)
			{
				if (trackColor is not null)
				{
					uiSwitch.OnTintColor = trackColor;
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					uiSwitch.OnTintColor = null;
					uIView.BackgroundColor = null;
				}
			}
			else
			{
				if (trackColor is not null)
				{
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					// iOS 13+ uses the UIColor.SecondarySystemFill to support Light and Dark mode
					// else, use the RGBA equivalent of UIColor.SecondarySystemFill in Light mode
					var fallbackColor = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : DefaultBackgroundColor;
					uIView.BackgroundColor = fallbackColor;
				}
			}
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			var styleChanged = uiSwitch.UpdatePreferredStyle(view);
			uiSwitch.ApplyThumbColor(view);

			if (styleChanged)
			{
				uiSwitch.ReapplyColorsAfterStyleUpdate(view);
			}
		}

		static void ApplyThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.ThumbTintColor = view.ThumbColor?.ToPlatform();
		}

		static bool UpdatePreferredStyle(this UISwitch uiSwitch, ISwitch view)
		{
#if IOS
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return false;
			}

			var preferredStyle = view.TrackColor is not null || view.ThumbColor is not null
				? UISwitchStyle.Sliding
				: UISwitchStyle.Automatic;

			if (uiSwitch.PreferredStyle != preferredStyle)
			{
				uiSwitch.PreferredStyle = preferredStyle;
				uiSwitch.SetNeedsLayout();
				uiSwitch.LayoutIfNeeded();
				return true;
			}
#endif
			return false;
		}

		static void ReapplyColorsAfterStyleUpdate(this UISwitch uiSwitch, ISwitch view)
		{
#if IOS
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			uiSwitch.ScheduleColorReapply(view, 0);
#endif
		}

		static void ScheduleColorReapply(this UISwitch uiSwitch, ISwitch view, int retryCount)
		{
			var weakSwitch = new WeakReference<UISwitch>(uiSwitch);
			var weakView = new WeakReference<ISwitch>(view);

			DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, TimeSpan.FromMilliseconds(10)), () =>
			{
				if (!weakSwitch.TryGetTarget(out var currentSwitch) || currentSwitch.Handle == IntPtr.Zero)
				{
					return;
				}

				if (!weakView.TryGetTarget(out var currentView))
				{
					return;
				}

				currentSwitch.SetNeedsLayout();
				currentSwitch.LayoutIfNeeded();

				if (!currentSwitch.IsReadyForColorReapply() && retryCount < MaxColorReapplyAttempts)
				{
					currentSwitch.ScheduleColorReapply(currentView, retryCount + 1);
					return;
				}

				currentSwitch.ApplyTrackColor(currentView);
				currentSwitch.ApplyThumbColor(currentView);
			});
		}

		static bool IsReadyForColorReapply(this UISwitch uiSwitch)
		{
			var trackSubview = uiSwitch.GetTrackSubview();

			return uiSwitch.Window is not null
				&& trackSubview is not null
				&& uiSwitch.Bounds.Width > 0
				&& uiSwitch.Bounds.Height > 0
				&& trackSubview.Bounds.Width > 0
				&& trackSubview.Bounds.Height > 0;
		}

		internal static UIView? GetTrackSubview(this UISwitch uISwitch)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
			else
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
		}

		internal static UIColor? GetTrackColor(this UISwitch uISwitch)
		{
			return uISwitch.GetTrackSubview()?.BackgroundColor;
		}
	}
}
