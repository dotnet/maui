using ElmSharp;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this Check platformCheck, ICheckBox check)
		{
			platformCheck.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this Check platformCheck, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint
			if (check.Foreground is SolidPaint solid)
			{
				platformCheck.Color = solid.Color.ToPlatformEFL();
			}
		}
	}
}