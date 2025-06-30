#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Internals
{
	static class DeviceOrientationExtensions
	{
		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="orientation">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static bool IsLandscape(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Landscape;

		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="orientation">For internal use by the Microsoft.Maui.Controls platform.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static bool IsPortrait(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Portrait;
	}
}