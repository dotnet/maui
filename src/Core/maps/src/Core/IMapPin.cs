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
		/// Gets the clustering identifier for this pin.
		/// Pins with the same identifier will be grouped together when clustering is enabled.
		/// </summary>
		string ClusteringIdentifier { get; }

		object? MarkerId { get; set; }

		bool SendMarkerClick();

		bool SendInfoWindowClick();
	}
}
