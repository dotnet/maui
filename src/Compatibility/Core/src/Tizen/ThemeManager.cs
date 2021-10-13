using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	public static class ThemeManager
	{
		public static double GetPhysicalPortraitSizeInDP()
		{
			var screenSize = Forms.PhysicalScreenSize;
			return Math.Min(screenSize.Width, screenSize.Height);
		}

		static double CalculateDoubleScaledSizeInLargeScreen(double size)
		{
			if (Forms.DisplayResolutionUnit.UseVP)
				return size;

			if (!Forms.DisplayResolutionUnit.UseDeviceScale && GetPhysicalPortraitSizeInDP() > 1000)
			{
				size *= 2.5;
			}

			if (!Forms.DisplayResolutionUnit.UseDP)
			{
				size = Forms.ConvertToPixel(size);
			}
			return size;
		}

	}
}
