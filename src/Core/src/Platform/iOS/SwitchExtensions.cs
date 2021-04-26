using System.Linq;
using UIKit;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view, UIColor? defaultOnTrackColor, UIColor? defaultOffTrackColor)
		{
			if (view == null)
				return;

			if (view.TrackColor == null)
				uiSwitch.OnTintColor = defaultOnTrackColor;
			else
				uiSwitch.OnTintColor = view.TrackColor.ToNative();

			UIView uIView;
			if (NativeVersion.IsAtLeast(13))
				uIView = uiSwitch.Subviews[0].Subviews[0];
			else
				uIView = uiSwitch.Subviews[0].Subviews[0].Subviews[0];

			if (view.TrackColor == null)
				uIView.BackgroundColor = defaultOffTrackColor;
			else
				uIView.BackgroundColor = uiSwitch.OnTintColor;
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view, UIColor? defaultThumbColor)
		{
			if (view == null)
				return;

			Graphics.Color thumbColor = view.ThumbColor;
			uiSwitch.ThumbTintColor = thumbColor?.ToNative() ?? defaultThumbColor;
		}

		internal static UIView GetTrackSubview(this UISwitch uISwitch)
		{
			UIView uIView;
			if (NativeVersion.IsAtLeast(13))
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
