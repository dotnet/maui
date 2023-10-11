using System;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Represents the idiom (form factor) of the device.
	/// </summary>
	public readonly struct DeviceIdiom : IEquatable<DeviceIdiom>
	{
		readonly string deviceIdiom;

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents a (mobile) phone idiom.
		/// </summary>
		public static DeviceIdiom Phone { get; } = new DeviceIdiom(nameof(Phone));

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents a tablet idiom.
		/// </summary>
		public static DeviceIdiom Tablet { get; } = new DeviceIdiom(nameof(Tablet));

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents a desktop computer idiom.
		/// </summary>
		public static DeviceIdiom Desktop { get; } = new DeviceIdiom(nameof(Desktop));

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents a television (TV) idiom.
		/// </summary>
		public static DeviceIdiom TV { get; } = new DeviceIdiom(nameof(TV));

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents a watch idiom.
		/// </summary>
		public static DeviceIdiom Watch { get; } = new DeviceIdiom(nameof(Watch));

		/// <summary>
		/// Gets an instance of <see cref="DeviceIdiom"/> that represents an unknown idiom. This is used for when the current device idiom is unknown.
		/// </summary>
		public static DeviceIdiom Unknown { get; } = new DeviceIdiom(nameof(Unknown));

		DeviceIdiom(string deviceIdiom)
		{
			if (deviceIdiom == null)
				throw new ArgumentNullException(nameof(deviceIdiom));

			if (deviceIdiom.Length == 0)
				throw new ArgumentException(nameof(deviceIdiom));

			this.deviceIdiom = deviceIdiom;
		}

		/// <summary>
		/// Creates a new device idiom instance. This can be used to define your custom idioms.
		/// </summary>
		/// <param name="deviceIdiom">The idiom name of the device.</param>
		/// <returns>A new instance of <see cref="DeviceIdiom"/> with the specified idiom type.</returns>
		public static DeviceIdiom Create(string deviceIdiom) =>
			new DeviceIdiom(deviceIdiom);

		/// <summary>
		/// Compares the underlying <see cref="DeviceIdiom"/> instances.
		/// </summary>
		/// <param name="other"><see cref="DeviceIdiom"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(DeviceIdiom other) =>
			Equals(other.deviceIdiom);

		internal bool Equals(string other) =>
			string.Equals(deviceIdiom, other, StringComparison.Ordinal);

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public override bool Equals(object obj) =>
			obj is DeviceIdiom && Equals((DeviceIdiom)obj);

		/// <summary>
		/// Gets the hash code for this idiom instance.
		/// </summary>
		/// <returns>The computed hash code for this device idiom or <c>0</c> when the device idiom is <see langword="null"/>.</returns>
		public override int GetHashCode() =>
			deviceIdiom == null ? 0 : deviceIdiom.GetHashCode(
#if !NETSTANDARD2_0
					StringComparison.Ordinal
#endif
				);

		/// <summary>
		/// Returns a string representation of the current device idiom.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>{device idiom}</c> or an empty string when no device idiom is set.</returns>
		public override string ToString() =>
			deviceIdiom ?? string.Empty;

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(DeviceIdiom left, DeviceIdiom right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(DeviceIdiom left, DeviceIdiom right) =>
			!left.Equals(right);
	}
}
