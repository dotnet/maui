#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Internals
{
	static class DeviceOrientationExtensions
	{
		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="orientation">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static bool IsLandscape(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Landscape;

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="orientation">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static bool IsPortrait(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Portrait;
	}
}