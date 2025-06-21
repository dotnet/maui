using Android.Graphics.Drawables;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view) =>
			aSwitch.Checked = view.IsOn;

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;

			if (aSwitch.Checked)
			{
				if (trackColor != null)
					aSwitch.TrackDrawable?.SetColorFilter(trackColor, FilterMode.SrcAtop);
			}
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			if (thumbColor != null)
				aSwitch.ThumbDrawable?.SetColorFilter(thumbColor, FilterMode.SrcAtop);
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}