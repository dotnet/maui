#nullable enable
namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Base interface for all interfaces related to device capabilities.
	/// </summary>
	public interface IDeviceCapabilities
	{
		/// <summary>
		/// Gets a value indicating whether the device supports the specific capability.
		/// </summary>
		bool IsSupported { get; }

	}
}