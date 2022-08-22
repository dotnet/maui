namespace Microsoft.Maui.Essentials
{

	public static partial class DeviceInfo
	{

		static string GetModel() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetManufacturer() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetDeviceName() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetVersionString() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static DevicePlatform Gtk { get; } = DevicePlatform.Create(nameof(Gtk));

		static DevicePlatform GetPlatform() => Gtk;

		static DeviceIdiom GetIdiom() => DeviceIdiom.Desktop;

		static DeviceType GetDeviceType() => DeviceType.Unknown;

	}

}