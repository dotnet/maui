using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IDeviceInfo"/> for Linux. The OS version comes from <c>/etc/os-release</c>, the model and manufacturer from
/// <c>/sys/devices/virtual/dmi/id</c>.
/// </summary>
sealed class GtkDeviceInfo : IDeviceInfo
{
	/// <summary>The platform reported by <see cref="Platform"/>.</summary>
	internal static DevicePlatform Linux { get; } = DevicePlatform.Create("Linux");

	readonly OsRelease osRelease;
	readonly DmiInfo dmi;
	readonly Func<string> kernelVersion;
	readonly Func<string> hostName;

	internal GtkDeviceInfo(OsRelease? osRelease = null, DmiInfo? dmi = null, Func<string>? kernelVersion = null, Func<string>? hostName = null)
	{
		this.osRelease = osRelease ?? OsRelease.Load();
		this.dmi = dmi ?? DmiInfo.Read();
		this.kernelVersion = kernelVersion ?? (() => Environment.OSVersion.Version.ToString());
		this.hostName = hostName ?? (() => Environment.MachineName);
	}

	public string Model => dmi.ProductName ?? "Unknown";

	public string Manufacturer => dmi.Vendor ?? "Unknown";

	public string Name => hostName();

	/// <summary>The distribution version (<c>VERSION_ID</c>), or the kernel version on rolling releases.</summary>
	public string VersionString => osRelease.VersionId ?? kernelVersion();

	public Version Version =>
		AppIdentity.ParseVersion(osRelease.VersionId) ?? AppIdentity.ParseVersion(kernelVersion()) ?? new Version(0, 0);

	public DevicePlatform Platform => Linux;

	public DeviceIdiom Idiom => DeviceIdiom.Desktop;

	public DeviceType DeviceType => dmi.IsVirtual ? DeviceType.Virtual : DeviceType.Physical;
}
