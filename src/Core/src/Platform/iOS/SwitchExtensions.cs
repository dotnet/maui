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
				uiSwitch.UpdateOnTintColor(trackColor);

				// With Liquid Glass, OnTintColor colors the track, and the track subview covers the thumb while the switch animates
				if (!IsLiquidGlass)
				{
					UpdateBackgroundColor(uIView, trackColor);
				}
			}
			else
			{
				// iOS 13+ uses the UIColor.SecondarySystemFill to support Light and Dark mode
				// else, use the RGBA equivalent of UIColor.SecondarySystemFill in Light mode
				var fallbackColor = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : DefaultBackgroundColor;
				UpdateBackgroundColor(uIView, trackColor ?? fallbackColor);
			}
		}

		// The colors are only set when they change: with Liquid Glass, setting them while the switch animates, as the IsOn update
		// of a tap does, shows a solid track over the thumb (https://github.com/dotnet/maui/issues/34560)
		internal static void UpdateOnTintColor(this UISwitch uiSwitch, UIColor? onTintColor)
		{
			if (!Equals(uiSwitch.OnTintColor, onTintColor))
			{
				uiSwitch.OnTintColor = onTintColor;
			}
		}

		static void UpdateBackgroundColor(UIView view, UIColor? backgroundColor)
		{
			if (!Equals(view.BackgroundColor, backgroundColor))
			{
				view.BackgroundColor = backgroundColor;
			}
		}

		static bool IsLiquidGlass => OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			Graphics.Color thumbColor = view.ThumbColor;
			if (thumbColor != null)
			{
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
			}
			else
			{
				uiSwitch.ThumbTintColor = null;
			}
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
