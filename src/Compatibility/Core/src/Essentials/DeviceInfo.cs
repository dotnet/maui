#nullable enable
using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceInfo']/Docs" />
	public static class DeviceInfo
	{
		static IDeviceInfo Current => Devices.DeviceInfo.Current;

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
	}
}
