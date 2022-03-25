#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceInfo']/Docs" />
	public static partial class DeviceInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Model']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static string Model => Current.Model;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Manufacturer']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static string Manufacturer => Current.Manufacturer;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static string Name => Current.Name;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static string VersionString => Current.VersionString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static Version Version => Current.Version;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Platform']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static DevicePlatform Platform => Current.Platform;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='Idiom']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static DeviceIdiom Idiom => Current.Idiom;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceInfo.xml" path="//Member[@MemberName='DeviceType']/Docs" />
		[Obsolete($"Use {nameof(DeviceInfo)}.{nameof(Current)} instead.", true)]
		public static DeviceType DeviceType => Current.DeviceType;
	}
}
