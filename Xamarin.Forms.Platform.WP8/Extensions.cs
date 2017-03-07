using Microsoft.Phone.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal static class Extensions
	{
		public static DeviceOrientation ToDeviceOrientation(this PageOrientation pageOrientation)
		{
			switch (pageOrientation)
			{
				case PageOrientation.None:
					return DeviceOrientation.Other;
				case PageOrientation.Portrait:
					return DeviceOrientation.Portrait;
				case PageOrientation.Landscape:
					return DeviceOrientation.Landscape;
				case PageOrientation.PortraitUp:
					return DeviceOrientation.PortraitUp;
				case PageOrientation.PortraitDown:
					return DeviceOrientation.PortraitDown;
				case PageOrientation.LandscapeRight:
					return DeviceOrientation.LandscapeRight;
				case PageOrientation.LandscapeLeft:
					return DeviceOrientation.LandscapeLeft;
				default:
					return DeviceOrientation.Other;
			}
		}
	}
}