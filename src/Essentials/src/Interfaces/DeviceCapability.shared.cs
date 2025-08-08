#nullable enable

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Represents a device capability that can be queried for support.
	/// </summary>
	public interface IDeviceCapability
	{
		/// <summary>
		/// Gets a value indicating whether this capability is supported on the current device.
		/// </summary>
		bool IsSupported { get; }
	}
}