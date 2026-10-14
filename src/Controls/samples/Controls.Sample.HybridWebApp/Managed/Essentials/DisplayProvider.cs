using Microsoft.Maui.Devices;

namespace Maui.HybridApp.Essentials.Display;

/// <summary>Read-only main-display snapshots. Never changes KeepScreenOn.</summary>
/// <param name="display">The host's Essentials display API.</param>
public sealed class DisplayProvider(IDeviceDisplay display)
{
	[HybridAction("display.info", MainThread = true)]
	internal DisplayResult Info()
	{
		var info = display.MainDisplayInfo;
		return new(info.Width, info.Height, info.Density, info.Orientation.ToString(), info.Rotation.ToString(), info.RefreshRate);
	}
}

sealed record DisplayResult(double Width, double Height, double Density, string Orientation, string Rotation, double RefreshRate);
