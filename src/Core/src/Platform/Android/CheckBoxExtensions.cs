using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using AAttribute = Android.Resource.Attribute;
using AColor = Android.Graphics.Color;
using XColor = Microsoft.Maui.Color;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		static readonly int[][] CheckedStates = new int[][]
		{
			new int[] { AAttribute.StateEnabled, AAttribute.StateChecked },
			new int[] { AAttribute.StateEnabled, -AAttribute.StateChecked },
			new int[] { -AAttribute.StateEnabled, AAttribute.StateChecked },
			new int[] { -AAttribute.StateEnabled, -AAttribute.StatePressed },
		};

		public static void UpdateBackgroundColor(this AppCompatCheckBox nativeCheckBox, ICheck check)
		{
			if (check.BackgroundColor == XColor.Default)
				nativeCheckBox.SetBackgroundColor(AColor.Transparent);
			else
				nativeCheckBox.SetBackgroundColor(check.BackgroundColor.ToNative());
		}

		public static void UpdateIsChecked(this AppCompatCheckBox nativeCheckBox, ICheck check)
		{
			nativeCheckBox.Checked = check.IsChecked;
		}

		public static void UpdateColor(this AppCompatCheckBox nativeCheckBox, ICheck check)
		{
			// TODO: Delete when implementing the logic to set the system accent color. 
			XColor accent = XColor.FromHex("#ff33b5e5");

			var tintColor = check.Color == XColor.Default ? accent.ToNative() : check.Color.ToNative();

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