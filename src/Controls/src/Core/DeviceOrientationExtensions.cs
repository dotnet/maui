#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Internals
{
	static class DeviceOrientationExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/DeviceOrientationExtensions.xml" path="//Member[@MemberName='IsLandscape']/Docs/*" />
		public static bool IsLandscape(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Landscape;

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/DeviceOrientationExtensions.xml" path="//Member[@MemberName='IsPortrait']/Docs/*" />
		public static bool IsPortrait(this DisplayOrientation orientation) =>
			orientation == DisplayOrientation.Portrait;
	}
}