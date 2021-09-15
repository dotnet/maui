namespace Microsoft.Maui.Essentials
{
	partial class PlatformDeviceInfo
	{
		string GetModel() => throw ExceptionUtils.NotSupportedOrImplementedException;

		string GetManufacturer() => throw ExceptionUtils.NotSupportedOrImplementedException;

		string GetDeviceName() => throw ExceptionUtils.NotSupportedOrImplementedException;

		string GetVersionString() => throw ExceptionUtils.NotSupportedOrImplementedException;

		DevicePlatform GetPlatform() => DevicePlatform.Unknown;

		DeviceIdiom GetIdiom() => DeviceIdiom.Unknown;

		DeviceType GetDeviceType() => DeviceType.Unknown;
	}
}
