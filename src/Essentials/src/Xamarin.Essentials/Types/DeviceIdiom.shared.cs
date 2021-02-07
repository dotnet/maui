using System;

namespace Xamarin.Essentials
{
	public readonly struct DeviceIdiom : IEquatable<DeviceIdiom>
	{
		readonly string deviceIdiom;

		public static DeviceIdiom Phone { get; } = new DeviceIdiom(nameof(Phone));

		public static DeviceIdiom Tablet { get; } = new DeviceIdiom(nameof(Tablet));

		public static DeviceIdiom Desktop { get; } = new DeviceIdiom(nameof(Desktop));

		public static DeviceIdiom TV { get; } = new DeviceIdiom(nameof(TV));

		public static DeviceIdiom Watch { get; } = new DeviceIdiom(nameof(Watch));

		public static DeviceIdiom Unknown { get; } = new DeviceIdiom(nameof(Unknown));

		DeviceIdiom(string deviceIdiom)
		{
			if (deviceIdiom == null)
				throw new ArgumentNullException(nameof(deviceIdiom));

			if (deviceIdiom.Length == 0)
				throw new ArgumentException(nameof(deviceIdiom));

			this.deviceIdiom = deviceIdiom;
		}

		public static DeviceIdiom Create(string deviceIdiom) =>
			new DeviceIdiom(deviceIdiom);

		public bool Equals(DeviceIdiom other) =>
			Equals(other.deviceIdiom);

		internal bool Equals(string other) =>
			string.Equals(deviceIdiom, other, StringComparison.Ordinal);

		public override bool Equals(object obj) =>
			obj is DeviceIdiom && Equals((DeviceIdiom)obj);

		public override int GetHashCode() =>
			deviceIdiom == null ? 0 : deviceIdiom.GetHashCode();

		public override string ToString() =>
			deviceIdiom ?? string.Empty;

		public static bool operator ==(DeviceIdiom left, DeviceIdiom right) =>
			left.Equals(right);

		public static bool operator !=(DeviceIdiom left, DeviceIdiom right) =>
			!left.Equals(right);
	}
}
