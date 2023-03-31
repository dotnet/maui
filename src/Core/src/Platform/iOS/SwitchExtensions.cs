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
			if (view == null)
				return;

			var uIView = GetTrackSubview(uiSwitch);

			if (uIView is null)
				return;

			if (!view.IsOn)
			{
				// iOS 13+ uses the UIColor.SecondarySystemFill to support Light and Dark mode
				// else, use the RGBA equivalent of UIColor.SecondarySystemFill in Light mode
				uIView.BackgroundColor = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : DefaultBackgroundColor;
			}

			else if (view.TrackColor is not null)
			{
				uiSwitch.OnTintColor = view.TrackColor.ToPlatform();
				uIView.BackgroundColor = uiSwitch.OnTintColor;
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
