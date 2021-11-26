using Android.Content.Res;
using Microsoft.Maui;
using AAttribute = Android.Resource.Attribute;
using APorterDuff = Android.Graphics.PorterDuff;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using MauiAttribute = Microsoft.Maui.Resource.Attribute;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this ASwitch aSwitch, ISwitch view) =>
			aSwitch.Checked = view.IsOn;

		public static void UpdateTrackColor(this ASwitch aSwitch, ISwitch view, ColorStateList? defaultTrackColor)
		{
			var trackColor = view.TrackColor;

			if (aSwitch.Context == null)
				return;

			if (trackColor == null)
			{
				aSwitch.TrackTintMode = APorterDuff.Mode.SrcAtop;
				aSwitch.TrackTintList = defaultTrackColor;
			}
			else
			{
				aSwitch.TrackTintMode = APorterDuff.Mode.SrcIn;
				aSwitch.TrackTintList = trackColor.ToDefaultColorStateList();
			}
		}

		public static void UpdateThumbColor(this ASwitch aSwitch, ISwitch view, ColorStateList? defaultColorStateList)
		{
			var thumbColor = view.ThumbColor;
			if (thumbColor != null)
			{
				aSwitch.ThumbTintMode = APorterDuff.Mode.SrcAtop;
				aSwitch.ThumbTintList = thumbColor.ToDefaultColorStateList();
			}
			else
			{
				aSwitch.ThumbTintList = defaultColorStateList;
				aSwitch.ThumbTintMode = APorterDuff.Mode.SrcIn;
			}
		}

		public static ColorStateList GetDefaultSwitchTrackColorStateList(this ASwitch aSwitch)
		{
			var context = aSwitch.Context;
			if (context == null)
				return new ColorStateList(null, null);

			return ColorStateListExtensions.CreateSwitch(
				context.GetThemeAttrColor(AAttribute.ColorForeground, 0.1f),
				context.GetThemeAttrColor(AAttribute.ColorControlActivated, 0.3f),
				context.GetThemeAttrColor(AAttribute.ColorForeground, 0.3f));
		}

		public static ColorStateList GetDefaultSwitchThumbColorStateList(this ASwitch aSwitch)
		{
			var context = aSwitch.Context;
			if (context == null)
				return new ColorStateList(null, null);

			return ColorStateListExtensions.CreateSwitch(
				context.GetDisabledThemeAttrColor(MauiAttribute.colorSwitchThumbNormal),
				context.GetThemeAttrColor(AAttribute.ColorControlActivated),
				context.GetThemeAttrColor(MauiAttribute.colorSwitchThumbNormal));
		}
	}
}
