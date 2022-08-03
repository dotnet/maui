using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Check platformCheck, ISwitch view)
		{
			platformCheck.IsChecked = view.IsOn;
		}

		public static void UpdateTrackColor(this Check platformCheck, ISwitch view)
		{
			if (view.ThumbColor != null)
			{
				platformCheck.Color = view.TrackColor.ToPlatformEFL();
			}
		}

		public static void UpdateThumbColor(this Check platformCheck, ISwitch view)
		{
			if (view.ThumbColor == null)
			{
				platformCheck.DeleteOnColors();
			}
			else
			{
				platformCheck.SetOnColors(view.ThumbColor.ToPlatformEFL());
			}
		}
	}
}