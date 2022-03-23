using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceIdiom']/Docs" />
	public readonly struct DeviceIdiom : IEquatable<DeviceIdiom>
	{
		readonly string deviceIdiom;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Phone']/Docs" />
		public static DeviceIdiom Phone { get; } = new DeviceIdiom(nameof(Phone));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Tablet']/Docs" />
		public static DeviceIdiom Tablet { get; } = new DeviceIdiom(nameof(Tablet));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Desktop']/Docs" />
		public static DeviceIdiom Desktop { get; } = new DeviceIdiom(nameof(Desktop));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='TV']/Docs" />
		public static DeviceIdiom TV { get; } = new DeviceIdiom(nameof(TV));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Watch']/Docs" />
		public static DeviceIdiom Watch { get; } = new DeviceIdiom(nameof(Watch));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Unknown']/Docs" />
		public static DeviceIdiom Unknown { get; } = new DeviceIdiom(nameof(Unknown));

		DeviceIdiom(string deviceIdiom)
		{
			if (deviceIdiom == null)
				throw new ArgumentNullException(nameof(deviceIdiom));

			if (deviceIdiom.Length == 0)
				throw new ArgumentException(nameof(deviceIdiom));

			this.deviceIdiom = deviceIdiom;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Create']/Docs" />
		public static DeviceIdiom Create(string deviceIdiom) =>
			new DeviceIdiom(deviceIdiom);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
		public bool Equals(DeviceIdiom other) =>
			Equals(other.deviceIdiom);

		internal bool Equals(string other) =>
			string.Equals(deviceIdiom, other, StringComparison.Ordinal);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object obj) =>
			obj is DeviceIdiom && Equals((DeviceIdiom)obj);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			deviceIdiom == null ? 0 : deviceIdiom.GetHashCode(
					#if !NETSTANDARD2_0
					StringComparison.Ordinal
					#endif
				);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceIdiom.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			deviceIdiom ?? string.Empty;

		public static bool operator ==(DeviceIdiom left, DeviceIdiom right) =>
			left.Equals(right);

		public static bool operator !=(DeviceIdiom left, DeviceIdiom right) =>
			!left.Equals(right);
	}
}
