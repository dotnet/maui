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

		public static void UpdateBackgroundColor(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			if (check.BackgroundColor == XColor.Default)
				nativeCheckBox.SetBackgroundColor(AColor.Transparent);
			else
				nativeCheckBox.SetBackgroundColor(check.BackgroundColor.ToNative());
		}

		public static void UpdateIsChecked(this AppCompatCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.Checked = check.IsChecked;
		}
	}
}