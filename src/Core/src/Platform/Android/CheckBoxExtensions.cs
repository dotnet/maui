using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Graphics;
using static Android.Resource;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		static readonly int[][] CheckedStates = new int[][]
		{
			new int[] { Attribute.StateEnabled, Attribute.StateChecked },
			new int[] { Attribute.StateEnabled, -Attribute.StateChecked },
			new int[] { -Attribute.StateEnabled, Attribute.StateChecked },
			new int[] { -Attribute.StateEnabled, -Attribute.StatePressed },
		};

		public static void UpdateBackground(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			var paint = check.Background;

			if (paint.IsNullOrEmpty())
				nativeCheckBox.SetBackgroundColor(AColor.Transparent);
			else
				nativeCheckBox.UpdateBackground((IView)check);
		}

		public static void UpdateIsChecked(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.Checked = check.IsChecked;
		}

		public static void UpdateForeground(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			// TODO: Delete when implementing the logic to set the system accent color. 
			Graphics.Color accent = Graphics.Color.FromArgb("#ff33b5e5");

			var targetColor = accent;

			// For the moment, we're only supporting solid color Paint for the Android Checkbox
			if (check.Foreground is SolidPaint solid)
			{
				targetColor = solid.Color;
			}

			var tintColor = targetColor.ToNative();

			var tintList = new ColorStateList(
				CheckedStates,
				new int[]
				{
 					tintColor,
 					tintColor,
 					tintColor,
 					tintColor
				});

			var tintMode = PorterDuff.Mode.SrcIn;

			CompoundButtonCompat.SetButtonTintList(nativeCheckBox, tintList);
			CompoundButtonCompat.SetButtonTintMode(nativeCheckBox, tintMode);
		}
	}
}