using Android.App;
using Android.Content.Res;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		internal static DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return DeviceDisplay.Current.MainDisplayInfo.Orientation;

			return window.Handler?.MauiContext?.GetPlatformWindow()?.Resources?.Configuration?.Orientation switch
			{
				Orientation.Landscape => DisplayOrientation.Landscape,
				Orientation.Portrait => DisplayOrientation.Portrait,
				Orientation.Square => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown
			};
		}
	}
}
