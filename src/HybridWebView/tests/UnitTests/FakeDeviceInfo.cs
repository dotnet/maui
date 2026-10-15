using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakeDeviceInfo : IDeviceInfo
{
	public string Model => "Test device";
	public string VersionString => "16";
	public DevicePlatform Platform { get; set; } = DevicePlatform.Android;
	public string Manufacturer => "Native manufacturer";
	public string Name => "Native device";
	public Version Version => new(16, 0, 1, 2);
	public DeviceIdiom Idiom { get; set; } = DeviceIdiom.Phone;
	public DeviceType DeviceType => DeviceType.Virtual;
}
