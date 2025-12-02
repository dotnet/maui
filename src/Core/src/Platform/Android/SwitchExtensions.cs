using Android.Content.Res;
using Android.Graphics.Drawables;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using MaterialSwitch = Google.Android.Material.MaterialSwitch.MaterialSwitch;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view)
		{
			aSwitch.Checked = view.IsOn;
		}

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;

			if (aSwitch is MaterialSwitch materialSwitch)
			{
				// Material 3: Use ThumbTintList and TrackTintList
				if (trackColor is not null)
				{
					materialSwitch.TrackTintList = ColorStateList.ValueOf(trackColor.ToPlatform());
				}
				else
				{
					materialSwitch.TrackTintList = null;
				}
			}
			else
			{
				// Material 2: Use drawable color filter
				if (trackColor is not null)
				{
					aSwitch.TrackDrawable?.SetColorFilter(trackColor, FilterMode.SrcAtop);
				}
				else
				{
					aSwitch.TrackDrawable?.ClearColorFilter();
				}
			}
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			if (aSwitch is MaterialSwitch materialSwitch)
			{
				// Material 3: Use ThumbTintList
				if (thumbColor is not null)
				{
					materialSwitch.ThumbTintList = ColorStateList.ValueOf(thumbColor.ToPlatform());
				}
				else
				{
					materialSwitch.ThumbTintList = null;
				}
			}
			else
			{
				// Material 2: Use drawable color filter
				if (thumbColor is not null)
				{
					aSwitch.ThumbDrawable?.SetColorFilter(thumbColor, FilterMode.SrcAtop);
				}
			}
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}