using Microsoft.Maui.Devices;

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		internal static DisplayOrientation GetOrientation(this IWindow? window) =>
			DeviceDisplay.Current.MainDisplayInfo.Orientation;
	}
}
