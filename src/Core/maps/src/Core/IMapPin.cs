using System.ComponentModel;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a Pin that displays a map.
	/// </summary>
	public interface IMapPin : IElement
	{
		/// <summary>
		/// The physical address that is associated with this pin.
		/// </summary>
		string Address { get; }

		/// <summary>
		/// The label that is shown for this pin.
		/// </summary>
		string Label { get; }

		/// <summary>
		/// The geographical location of this pin.
		/// </summary>
		Location Location { get; }

		/// <summary>
		/// Gets or sets the platform-specific marker identifier.
		/// </summary>
		/// <remarks>This should typically not be set by the developer. Doing so might result in unpredictable behavior.</remarks>
		object? MarkerId { get; set; }

		/// <summary>
		/// Sends a marker click event.
		/// </summary>
		/// <returns><see langword="true"/> if the info window should be hidden; otherwise, <see langword="false"/>.</returns>
		bool SendMarkerClick();

		/// <summary>
		/// Sends an info window click event.
		/// </summary>
		/// <returns><see langword="true"/> if the info window should be hidden; otherwise, <see langword="false"/>.</returns>
		bool SendInfoWindowClick();
	}
}
