using Foundation;
using System;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[PortHandler("Following methods still need to be ported.")]
	public static class Extensions
	{
		public static UIModalPresentationStyle ToNativeModalPresentationStyle(this PlatformConfiguration.iOSSpecific.UIModalPresentationStyle style)
		{
			switch (style)
			{
				case PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FormSheet:
					return UIModalPresentationStyle.FormSheet;
				case PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.FullScreen:
					return UIModalPresentationStyle.FullScreen;
				case PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.Automatic:
					return UIModalPresentationStyle.Automatic;
				case PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.OverFullScreen:
					return UIModalPresentationStyle.OverFullScreen;
				case PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.PageSheet:
					return UIModalPresentationStyle.PageSheet;
				default:
					throw new ArgumentOutOfRangeException(nameof(style));
			}
		}

		internal static UISearchBarStyle ToNativeSearchBarStyle(this PlatformConfiguration.iOSSpecific.UISearchBarStyle style)
		{
			switch (style)
			{
				case PlatformConfiguration.iOSSpecific.UISearchBarStyle.Default:
					return UISearchBarStyle.Default;
				case PlatformConfiguration.iOSSpecific.UISearchBarStyle.Prominent:
					return UISearchBarStyle.Prominent;
				case PlatformConfiguration.iOSSpecific.UISearchBarStyle.Minimal:
					return UISearchBarStyle.Minimal;
				default:
					throw new ArgumentOutOfRangeException(nameof(style));
			}
		}
		internal static DeviceOrientation ToDeviceOrientation(this UIDeviceOrientation orientation)
		{
			switch (orientation)
			{
				case UIDeviceOrientation.Portrait:
					return DeviceOrientation.Portrait;
				case UIDeviceOrientation.PortraitUpsideDown:
					return DeviceOrientation.PortraitDown;
				case UIDeviceOrientation.LandscapeLeft:
					return DeviceOrientation.LandscapeLeft;
				case UIDeviceOrientation.LandscapeRight:
					return DeviceOrientation.LandscapeRight;
				default:
					return DeviceOrientation.Other;
			}
		}

		internal static bool IsHorizontal(this Button.ButtonContentLayout layout) =>
			layout.Position == Button.ButtonContentLayout.ImagePosition.Left ||
			layout.Position == Button.ButtonContentLayout.ImagePosition.Right;
	}
}