using System;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Represents the device platform that the application is running on.
	/// </summary>
	public readonly struct DevicePlatform : IEquatable<DevicePlatform>
	{
		readonly string devicePlatform;

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents Android.
		/// </summary>
		public static DevicePlatform Android { get; } = new DevicePlatform(nameof(Android));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents iOS.
		/// </summary>
		public static DevicePlatform iOS { get; } = new DevicePlatform(nameof(iOS));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents macOS.
		/// </summary>
		/// <remarks>Note, this is different than <see cref="MacCatalyst"/>.</remarks>
		public static DevicePlatform macOS { get; } = new DevicePlatform(nameof(macOS));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents Mac Catalyst.
		/// </summary>
		/// <remarks>Note, this is different than <see cref="macOS"/>.</remarks>
		public static DevicePlatform MacCatalyst { get; } = new DevicePlatform(nameof(MacCatalyst));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents Apple tvOS.
		/// </summary>
		public static DevicePlatform tvOS { get; } = new DevicePlatform(nameof(tvOS));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents Samsung Tizen.
		/// </summary>
		public static DevicePlatform Tizen { get; } = new DevicePlatform(nameof(Tizen));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents UWP.
		/// </summary>
		[Obsolete("Use WinUI instead.")]
		public static DevicePlatform UWP { get; } = new DevicePlatform(nameof(WinUI));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents WinUI.
		/// </summary>
		public static DevicePlatform WinUI { get; } = new DevicePlatform(nameof(WinUI));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents Apple watchOS.
		/// </summary>
		public static DevicePlatform watchOS { get; } = new DevicePlatform(nameof(watchOS));

		/// <summary>
		/// Gets an instance of <see cref="DevicePlatform"/> that represents an unknown platform. This is used for when the current platform is unknown.
		/// </summary>
		public static DevicePlatform Unknown { get; } = new DevicePlatform(nameof(Unknown));

		DevicePlatform(string devicePlatform)
		{
			if (devicePlatform == null)
				throw new ArgumentNullException(nameof(devicePlatform));

			if (devicePlatform.Length == 0)
				throw new ArgumentException(nameof(devicePlatform));

			this.devicePlatform = devicePlatform;
		}

		/// <summary>
		/// Creates a new device platform instance. This can be used to define your custom platforms.
		/// </summary>
		/// <param name="devicePlatform">The device platform identifier.</param>
		/// <returns>A new instance of <see cref="DevicePlatform"/> with the specified platform identifier.</returns>
		public static DevicePlatform Create(string devicePlatform) =>
			new DevicePlatform(devicePlatform);

		/// <summary>
		/// Compares the underlying <see cref="DevicePlatform"/> instances.
		/// </summary>
		/// <param name="other"><see cref="DevicePlatform"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(DevicePlatform other) =>
			Equals(other.devicePlatform);

		internal bool Equals(string other) =>
			string.Equals(devicePlatform, other, StringComparison.Ordinal);

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public override bool Equals(object obj) =>
			obj is DevicePlatform && Equals((DevicePlatform)obj);

		/// <summary>
		/// Gets the hash code for this platform instance.
		/// </summary>
		/// <returns>The computed hash code for this device platform or <c>0</c> when the device platform is <see langword="null"/>.</returns>
		public override int GetHashCode() =>
			devicePlatform == null ? 0 : devicePlatform.GetHashCode(
#if !NETSTANDARD2_0
					StringComparison.Ordinal
#endif
				);

		/// <summary>
		/// Returns a string representation of the current value of the device platform.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>{device platform}</c> or an empty string when no device platform is set.</returns>
		public override string ToString() =>
			devicePlatform ?? string.Empty;

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(DevicePlatform left, DevicePlatform right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(DevicePlatform left, DevicePlatform right) =>
			!left.Equals(right);
	}
}
