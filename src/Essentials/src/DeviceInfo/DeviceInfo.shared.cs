using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceInfo']/Docs" />
	public static partial class DeviceInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Model']/Docs" />
		public static string Model => GetModel();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Manufacturer']/Docs" />
		public static string Manufacturer => GetManufacturer();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		public static string Name => GetDeviceName();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		public static string VersionString => GetVersionString();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		public static Version Version => Utils.ParseVersion(VersionString);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Platform']/Docs" />
		public static DevicePlatform Platform => GetPlatform();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Idiom']/Docs" />
		public static DeviceIdiom Idiom => GetIdiom();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='DeviceType']/Docs" />
		public static DeviceType DeviceType => GetDeviceType();
	}

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
}
