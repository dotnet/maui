using System.ComponentModel;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a Pin that displays a map.
	/// </summary>
	public interface IMapPin : IElement, INotifyPropertyChanged
	{
		/// <summary>
		/// The physical address that is associated with this pin.
		/// </summary>
		string Address { get; }
		
		/// <summary>
		/// The label that is shown for this pin.
		/// </summary>
		string Label { get; }

		object MarkerId { get; set; }

		/// <summary>
		/// The geographical location of this pin.
		/// </summary>
		Location Position { get; }

		bool SendMarkerClick();

		bool SendInfoWindowClick();
	}
}
