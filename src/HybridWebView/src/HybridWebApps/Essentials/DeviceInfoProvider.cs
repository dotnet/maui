using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.DeviceInfo;

/// <summary>Device metadata from the host's existing Essentials API.</summary>
/// <param name="deviceInfo">The host's device metadata.</param>
public sealed class DeviceInfoProvider(IDeviceInfo deviceInfo)
{
	[HybridAction("device.info", MainThread = true)]
	internal DeviceInfoResult Info() => new(deviceInfo.Platform.ToString(), deviceInfo.VersionString, deviceInfo.Model);
}

sealed record DeviceInfoResult(string Platform, string OsVersion, string Model);
