using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class DeviceOrientationExtensions
	{
		public static bool IsLandscape(this DeviceOrientation orientation)
		{
			return orientation == DeviceOrientation.Landscape || orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight;
		}

		public static bool IsPortrait(this DeviceOrientation orientation)
		{
			return orientation == DeviceOrientation.Portrait || orientation == DeviceOrientation.PortraitDown || orientation == DeviceOrientation.PortraitUp;
		}
	}
}