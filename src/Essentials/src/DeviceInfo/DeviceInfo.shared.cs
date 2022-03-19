#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceType.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceType']/Docs" />
	public enum DeviceType
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceType.xml" path="//Member[@MemberName='Unknown']/Docs" />
		Unknown = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceType.xml" path="//Member[@MemberName='Physical']/Docs" />
		Physical = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceType.xml" path="//Member[@MemberName='Virtual']/Docs" />
		Virtual = 2
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

	public static class DeviceInfo
	{
		static IDeviceInfo? currentImplementation;

		public static IDeviceInfo Current =>
			currentImplementation ??= new DeviceInfoImplementation();

		internal static void SetCurrent(IDeviceInfo? implementation) =>
			currentImplementation = implementation;
	}
}
