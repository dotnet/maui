using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakeDeviceInfo : IDeviceInfo
{
	public string Model => "Test device";
	public string VersionString => "16";
	public DevicePlatform Platform => DevicePlatform.Android;
	public string Manufacturer => throw new InvalidOperationException("Not in the exposed subset.");
	public string Name => throw new InvalidOperationException("Not in the exposed subset.");
	public Version Version => throw new InvalidOperationException("Not in the exposed subset.");
	public DeviceIdiom Idiom => throw new InvalidOperationException("Not in the exposed subset.");
	public DeviceType DeviceType => throw new InvalidOperationException("Not in the exposed subset.");
}
