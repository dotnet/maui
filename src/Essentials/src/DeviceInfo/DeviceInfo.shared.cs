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

	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceInfo']/Docs" />
	public static class DeviceInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Model']/Docs" />
		public static string Model => Current.Model;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Manufacturer']/Docs" />
		public static string Manufacturer => Current.Manufacturer;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		public static string Name => Current.Name;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		public static string VersionString => Current.VersionString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		public static Version Version => Current.Version;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Platform']/Docs" />
		public static DevicePlatform Platform => Current.Platform;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Idiom']/Docs" />
		public static DeviceIdiom Idiom => Current.Idiom;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='DeviceType']/Docs" />
		public static DeviceType DeviceType => Current.DeviceType;

		static IDeviceInfo? currentImplementation;

		public static IDeviceInfo Current =>
			currentImplementation ??= new DeviceInfoImplementation();

		internal static void SetCurrent(IDeviceInfo? implementation) =>
			currentImplementation = implementation;
	}
}
