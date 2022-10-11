using System;
using System.Linq;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static UIColor? OffTrackColor;
		static bool HasSwitched;

		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			UpdateOffTrackColor(uiSwitch);

			var uIView = GetTrackSubview(uiSwitch);

			if (!view.IsOn)
				uIView.BackgroundColor = OffTrackColor;

			else if (view.TrackColor is not null) {
				uiSwitch.OnTintColor = view.TrackColor.ToPlatform ();
				uIView.BackgroundColor = uiSwitch.OnTintColor;
			}
		}

		static void UpdateOffTrackColor (UISwitch uiSwitch)
		{
			if (!HasSwitched)
			{
				OffTrackColor = uiSwitch.GetOffTrackColor();
				HasSwitched = true;
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

		internal static UIView GetTrackSubview(this UISwitch uISwitch)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				return uISwitch.Subviews[0].Subviews[0];
			else
				return uISwitch.Subviews[0].Subviews[0].Subviews[0];
		}

		internal static UIColor? GetOffTrackColor(this UISwitch uISwitch)
		{
			return uISwitch.GetTrackSubview().BackgroundColor;
		}
	}
}
