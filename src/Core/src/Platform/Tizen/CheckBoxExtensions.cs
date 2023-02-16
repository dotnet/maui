using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckBox platformCheck, ICheckBox check)
		{
			platformCheck.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this CheckBox platformCheck, ICheckBox check)
		{
			// For the moment, we're only supporting solid color Paint
			if (check.Foreground is SolidPaint solid)
			{
				platformCheck.Color = solid.Color.ToPlatform();
			}
		}
	}
}