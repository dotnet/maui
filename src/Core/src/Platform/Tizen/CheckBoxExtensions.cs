using ElmSharp;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this Check nativeCheck, ICheckBox check)
		{
			nativeCheck.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this Check nativeCheck, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint
			if (check.Foreground is SolidPaint solid)
			{
				nativeCheck.Color = solid.Color.ToNativeEFL();
			}
		}
	}
}