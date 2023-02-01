#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	///Types of devices.
	/// </summary>
	public enum DeviceType
	{
		/// <summary>An unknown device type.</summary>
		Unknown = 0,

		/// <summary>The device is a physical device, such as an iPhone, Android tablet, or Windows/macOS desktop.</summary>
		Physical = 1,

		/// <summary>The device is virtual, such as the iOS Simulator, Android emulators, or Windows emulators.</summary>
		Virtual = 2
	}

	/// <summary>
	/// Represents information about the device.
	/// </summary>
	public interface IDeviceInfo
	{
		/// <summary>
		/// Gets the model of the device.
		/// </summary>
		string Model { get; }

		/// <summary>
		/// Gets the manufacturer of the device.
		/// </summary>
		string Manufacturer { get; }

		/// <summary>
		/// Gets the name of the device.
		/// </summary>
		/// <remarks>This value is often specified by the user of the device.</remarks>
		string Name { get; }

		/// <summary>
		/// Gets the string representation of the version of the operating system.
		/// </summary>
		string VersionString { get; }

		/// <summary>
		/// Gets the version of the operating system.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Gets the platform or operating system of the device.
		/// </summary>
		DevicePlatform Platform { get; }

		/// <summary>
		/// Gets the idiom (form factor) of the device.
		/// </summary>
		DeviceIdiom Idiom { get; }

		/// <summary>
		/// Gets the type of device the application is running on.
		/// </summary>
		DeviceType DeviceType { get; }
	}

	/// <summary>
	/// Represents information about the device.
	/// </summary>
	public static class DeviceInfo
	{
		/// <summary>
		/// Gets the model of the device.
		/// </summary>
		public static string Model => Current.Model;

		/// <summary>
		/// Gets the manufacturer of the device.
		/// </summary>
		public static string Manufacturer => Current.Manufacturer;

		/// <summary>
		/// Gets the name of the device.
		/// </summary>
		/// <remarks>This value is often specified by the user of the device.</remarks>
		public static string Name => Current.Name;

		/// <summary>
		/// Gets the string representation of the version of the operating system.
		/// </summary>
		public static string VersionString => Current.VersionString;

		/// <summary>
		/// Gets the version of the operating system.
		/// </summary>
		public static Version Version => Current.Version;

		/// <summary>
		/// Gets the platform or operating system of the device.
		/// </summary>
		public static DevicePlatform Platform => Current.Platform;

		/// <summary>
		/// Gets the idiom (form factor) of the device.
		/// </summary>
		public static DeviceIdiom Idiom => Current.Idiom;

		/// <summary>
		/// Gets the type of device the application is running on.
		/// </summary>
		public static DeviceType DeviceType => Current.DeviceType;

		static IDeviceInfo? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IDeviceInfo Current =>
			currentImplementation ??= new DeviceInfoImplementation();

		internal static void SetCurrent(IDeviceInfo? implementation) =>
			currentImplementation = implementation;
	}
}
