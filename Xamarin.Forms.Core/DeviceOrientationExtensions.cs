using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	public static class DeviceOrientationExtensions
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsLandscape(this DeviceOrientation orientation)
		{
			return orientation == DeviceOrientation.Landscape || orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsPortrait(this DeviceOrientation orientation)
		{
			return orientation == DeviceOrientation.Portrait || orientation == DeviceOrientation.PortraitDown || orientation == DeviceOrientation.PortraitUp;
		}
	}
}