using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Event arguments for the <see cref="Map.ClusterClicked"/> event.
	/// </summary>
	public class ClusterClickedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the collection of pins contained in this cluster.
		/// </summary>
		public IReadOnlyList<Pin> Pins { get; }

		/// <summary>
		/// Gets the location of the cluster on the map.
		/// </summary>
		public Location Location { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the default behavior of expanding 
		/// the cluster (zooming in) should be prevented.
		/// </summary>
		/// <remarks>
		/// By default, tapping a cluster will zoom the map to show all the pins in the cluster.
		/// Set this to <see langword="true"/> to prevent this behavior and handle the click manually.
		/// </remarks>
		public bool Handled { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClusterClickedEventArgs"/> class.
		/// </summary>
		/// <param name="pins">The pins contained in the cluster.</param>
		/// <param name="location">The location of the cluster.</param>
		public ClusterClickedEventArgs(IReadOnlyList<Pin> pins, Location location)
		{
			Pins = pins ?? throw new ArgumentNullException(nameof(pins));
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}
	}
}
