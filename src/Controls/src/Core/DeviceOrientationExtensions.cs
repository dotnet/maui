using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls.Internals
{
	static class DeviceOrientationExtensions
	{
		public static bool IsLandscape(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Landscape;

		public static bool IsPortrait(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Portrait;
	}
}