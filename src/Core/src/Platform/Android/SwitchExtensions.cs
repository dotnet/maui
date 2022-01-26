using Android.Graphics.Drawables;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view) =>
			aSwitch.Checked = view.IsOn;

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view, Drawable? defaultTrackDrawable)
		{
			var trackColor = view.TrackColor;

			if (aSwitch.Checked)
			{
				if (trackColor == null)
					aSwitch.TrackDrawable = defaultTrackDrawable;
				else
					aSwitch.TrackDrawable?.SetColorFilter(trackColor, FilterMode.SrcAtop);
			}
			else
				aSwitch.TrackDrawable?.ClearColorFilter();
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view, Drawable? defaultThumbDrawable)
		{
			var thumbColor = view.ThumbColor;

			if (thumbColor != null)
				aSwitch.ThumbDrawable?.SetColorFilter(thumbColor, FilterMode.SrcAtop);
			else
				aSwitch.ThumbDrawable = defaultThumbDrawable;
		}

		public static Drawable GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}