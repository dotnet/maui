using Android.Content.Res;
using Android.Graphics.Drawables;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using MSwitch = Google.Android.Material.MaterialSwitch.MaterialSwitch;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static ColorStateList? _defaultTrackTintList;
		static ColorStateList? _defaultThumbTintList;

		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view)
		{
			aSwitch.Checked = view.IsOn;
		}

		internal static void UpdateIsOn(this MSwitch materialSwitch, ISwitch view)
		{
			materialSwitch.Checked = view.IsOn;
		}

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;

			if (trackColor is not null)
			{
				aSwitch.TrackDrawable?.SetColorFilter(trackColor, FilterMode.SrcAtop);
			}
			else
			{
				aSwitch.TrackDrawable?.ClearColorFilter();
			}
		}

		internal static void UpdateTrackColor(this MSwitch materialSwitch, ISwitch view)
		{
			var trackColor = view.TrackColor;
			_defaultTrackTintList ??= materialSwitch.TrackTintList;

			if (trackColor is not null)
			{
				materialSwitch.TrackTintList = ColorStateList.ValueOf(trackColor.ToPlatform());
			}
			else
			{
				materialSwitch.TrackTintList = _defaultTrackTintList;
			}
		}

		internal static void UpdateThumbColor(this MSwitch materialSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;
			_defaultThumbTintList ??= materialSwitch.ThumbTintList;
			if (thumbColor is not null)
			{
				materialSwitch.ThumbTintList = ColorStateList.ValueOf(thumbColor.ToPlatform());
			}
			else if (thumbColor is null && materialSwitch is MauiMaterialSwitch mauiSwitch)
			{
				materialSwitch.ThumbTintList = _defaultThumbTintList;

			}
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view)
		{
			var thumbColor = view.ThumbColor;

			if (thumbColor is not null)
			{
				aSwitch.ThumbDrawable?.SetColorFilter(thumbColor, FilterMode.SrcAtop);
			}
		}

		public static Drawable? GetDefaultSwitchTrackDrawable(this ASwitch aSwitch) =>
			aSwitch.TrackDrawable;

		public static Drawable? GetDefaultSwitchThumbDrawable(this ASwitch aSwitch) =>
			aSwitch.ThumbDrawable;
	}
}