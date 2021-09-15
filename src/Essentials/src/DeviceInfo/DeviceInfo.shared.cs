#nullable enable
using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class DeviceInfo
	{
		static readonly Lazy<IDeviceInfo> PlatformDeviceInfo = new(() => new PlatformDeviceInfo());
		static IDeviceInfo? CurrentDeviceInfo;

		public static void SetCurrent(IDeviceInfo? current) => CurrentDeviceInfo = current;

		public static IDeviceInfo Current => CurrentDeviceInfo ?? PlatformDeviceInfo.Value;

		public static string Model => Current.Model;

		public static string Manufacturer => Current.Manufacturer;

		public static string Name => Current.Name;

		public static string VersionString => Current.VersionString;

		public static Version Version => Current.Version;

		public static DevicePlatform Platform => Current.Platform;

		public static DeviceIdiom Idiom => Current.Idiom;

		public static DeviceType DeviceType => Current.DeviceType;
	}

	partial class PlatformDeviceInfo : IDeviceInfo
	{
		public string Model => GetModel();

		public string Manufacturer => GetManufacturer();

		public string Name => GetDeviceName();

		public string VersionString => GetVersionString();

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform => GetPlatform();

		public DeviceIdiom Idiom => GetIdiom();

		public DeviceType DeviceType => GetDeviceType();
	}

	public interface IDeviceInfo
	{
		string Model { get; }

		string Manufacturer { get; }

		string Name { get; }

		string VersionString { get; }

		Version Version { get; }

		DevicePlatform Platform { get; }

		DeviceIdiom Idiom { get; }

		DeviceType DeviceType { get; }
	}

	public enum DeviceType
	{
		Unknown = 0,
		Physical = 1,
		Virtual = 2
	}
}
