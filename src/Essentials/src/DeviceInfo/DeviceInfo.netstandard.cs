namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceInfo']/Docs" />
	public static partial class DeviceInfo
	{
		static string GetModel() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetManufacturer() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetDeviceName() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static string GetVersionString() => throw ExceptionUtils.NotSupportedOrImplementedException;

		static DevicePlatform GetPlatform() => DevicePlatform.Unknown;

		static DeviceIdiom GetIdiom() => DeviceIdiom.Unknown;

		static DeviceType GetDeviceType() => DeviceType.Unknown;
	}
}
