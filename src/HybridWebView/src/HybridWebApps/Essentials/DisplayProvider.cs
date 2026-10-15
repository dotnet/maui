using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.Display;

/// <summary>Read-only main-display snapshots. Never changes KeepScreenOn.</summary>
/// <param name="display">The host's Essentials display API.</param>
public sealed class DisplayProvider(IDeviceDisplay display)
{
	[HybridAction("display.info", MainThread = true)]
	internal DisplayResult Info()
	{
		var info = display.MainDisplayInfo;
		return new(info.Width, info.Height, info.Density, info.Orientation, info.Rotation, info.RefreshRate);
	}
}

sealed record DisplayResult(double Width, double Height, double Density, DisplayOrientation Orientation, DisplayRotation Rotation, double RefreshRate);
