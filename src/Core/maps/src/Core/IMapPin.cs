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
		/// Gets the custom image source for this pin's icon.
		/// </summary>
		/// <remarks>When set, this image will be used instead of the default platform pin icon.</remarks>
		IImageSource? ImageSource { get; }

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
