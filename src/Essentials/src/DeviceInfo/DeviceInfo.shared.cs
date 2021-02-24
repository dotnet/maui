using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class DeviceInfo
	{
		public static string Model => GetModel();

		public static string Manufacturer => GetManufacturer();

		public static string Name => GetDeviceName();

		public static string VersionString => GetVersionString();

		public static Version Version => Utils.ParseVersion(VersionString);

		public static DevicePlatform Platform => GetPlatform();

		public static DeviceIdiom Idiom => GetIdiom();

		public static DeviceType DeviceType => GetDeviceType();
	}

	public enum DeviceType
	{
		Unknown = 0,
		Physical = 1,
		Virtual = 2
	}
}
