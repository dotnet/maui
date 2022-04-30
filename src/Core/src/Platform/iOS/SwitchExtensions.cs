﻿using System;
using System.Linq;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			if (view.TrackColor != null)
				uiSwitch.OnTintColor = view.TrackColor.ToPlatform();

			UIView uIView = uiSwitch.GetTrackSubview();

			if (view.TrackColor != null)
				uIView.BackgroundColor = uiSwitch.OnTintColor;
			else if (uiSwitch.On)
				uIView.BackgroundColor = null;
			else
				uIView.BackgroundColor = new Graphics.Color(120, 120, 128, 40).ToPlatform();
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			Graphics.Color thumbColor = view.ThumbColor;
			if (thumbColor != null)
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
			else
				uiSwitch.ThumbTintColor = thumbColor?.ToPlatform();
		}

		internal static UIView GetTrackSubview(this UISwitch uISwitch)
		{
			UIView uIView;
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				uIView = uISwitch.Subviews[0].Subviews[0];
			else
				uIView = uISwitch.Subviews[0].Subviews[0].Subviews[0];

			return uIView;
		}

		internal static UIColor? GetOffTrackColor(this UISwitch uISwitch)
		{
			return uISwitch.GetTrackSubview().BackgroundColor;
		}
	}
}
