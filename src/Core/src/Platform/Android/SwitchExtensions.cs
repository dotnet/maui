using Android.Content.Res;
using Microsoft.Maui;
using AAttribute = Android.Resource.Attribute;
using APorterDuff = Android.Graphics.PorterDuff;
using ASwitch = AndroidX.AppCompat.Widget.SwitchCompat;
using MauiAttribute = Microsoft.Maui.Resource.Attribute;

namespace Microsoft.Maui
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

		static int[] GetCurrentState(this ASwitch aSwitch)
		{
			if (!aSwitch.Enabled)
				return _checkedStates[0];

			if (aSwitch.Checked)
				return _checkedStates[1];

			return _checkedStates[2];
		}

		public static ColorStateList GetDefaultSwitchTrackColorStateList(this ASwitch aSwitch)
		{
			var context = aSwitch.Context;
			if (context == null)
				return new ColorStateList(null, null);

			int[][] states = new int[3][];
			int[] colors = new int[3];

			states[0] = new int[] { -AAttribute.StateEnabled };
			colors[0] = context.GetThemeAttrColor(AAttribute.ColorForeground, 0.1f);

			states[1] = new int[] { AAttribute.StateChecked };
			colors[1] = context.GetThemeAttrColor(AAttribute.ColorControlActivated, 0.3f);

			states[2] = new int[0];
			colors[2] = context.GetThemeAttrColor(AAttribute.ColorForeground, 0.3f);
			return new ColorStateList(states, colors);
		}

		public static ColorStateList GetDefaultSwitchThumbColorStateList(this ASwitch aSwitch)
		{
			var context = aSwitch.Context;
			if (context == null)
				return new ColorStateList(null, null);

			int[][] states = new int[3][];
			int[] colors = new int[3];

			states[0] = new int[] { -AAttribute.StateEnabled };
			colors[0] = context.GetDisabledThemeAttrColor(MauiAttribute.colorSwitchThumbNormal);

			states[1] = new int[] { AAttribute.StateChecked };
			colors[1] = context.GetThemeAttrColor(AAttribute.ColorControlActivated);

			states[2] = new int[0];
			colors[2] = context.GetThemeAttrColor(MauiAttribute.colorSwitchThumbNormal);
			return new ColorStateList(states, colors);
		}

		static int[][] _checkedStates = new int[][]
		{
			new int[] { -AAttribute.StateEnabled },
			new int[] { AAttribute.StateChecked },
			new int[0],
		};
	}
}
