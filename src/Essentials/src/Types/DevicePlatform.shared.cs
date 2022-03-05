using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DevicePlatform']/Docs" />
	public readonly struct DevicePlatform : IEquatable<DevicePlatform>
	{
		readonly string devicePlatform;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Android']/Docs" />
		public static DevicePlatform Android { get; } = new DevicePlatform(nameof(Android));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='iOS']/Docs" />
		public static DevicePlatform iOS { get; } = new DevicePlatform(nameof(iOS));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='macOS']/Docs" />
		public static DevicePlatform macOS { get; } = new DevicePlatform(nameof(macOS));

		public static DevicePlatform MacCatalyst { get; } = new DevicePlatform(nameof(MacCatalyst));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='tvOS']/Docs" />
		public static DevicePlatform tvOS { get; } = new DevicePlatform(nameof(tvOS));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Tizen']/Docs" />
		public static DevicePlatform Tizen { get; } = new DevicePlatform(nameof(Tizen));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='UWP']/Docs" />
		[Obsolete("Use WinUI instead.")]
		public static DevicePlatform UWP { get; } = new DevicePlatform(nameof(WinUI));

		public static DevicePlatform WinUI { get; } = new DevicePlatform(nameof(WinUI));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='watchOS']/Docs" />
		public static DevicePlatform watchOS { get; } = new DevicePlatform(nameof(watchOS));

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Unknown']/Docs" />
		public static DevicePlatform Unknown { get; } = new DevicePlatform(nameof(Unknown));

		DevicePlatform(string devicePlatform)
		{
			if (devicePlatform == null)
				throw new ArgumentNullException(nameof(devicePlatform));

			if (devicePlatform.Length == 0)
				throw new ArgumentException(nameof(devicePlatform));

			this.devicePlatform = devicePlatform;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Create']/Docs" />
		public static DevicePlatform Create(string devicePlatform) =>
			new DevicePlatform(devicePlatform);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(DevicePlatform other) =>
			Equals(other.devicePlatform);

		internal bool Equals(string other) =>
			string.Equals(devicePlatform, other, StringComparison.Ordinal);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			obj is DevicePlatform && Equals((DevicePlatform)obj);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			devicePlatform == null ? 0 : devicePlatform.GetHashCode(
					#if !NETSTANDARD2_0
					StringComparison.Ordinal
					#endif
				);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DevicePlatform.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			devicePlatform ?? string.Empty;

		public static bool operator ==(DevicePlatform left, DevicePlatform right) =>
			left.Equals(right);

		public static bool operator !=(DevicePlatform left, DevicePlatform right) =>
			!left.Equals(right);
	}
}
