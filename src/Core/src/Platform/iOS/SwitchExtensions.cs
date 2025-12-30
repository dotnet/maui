using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static UIColor DefaultBackgroundColor = UIColor.FromRGBA(120, 120, 128, 40);

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

			Graphics.Color thumbColor = view.ThumbColor;
			if (thumbColor != null)
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
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
