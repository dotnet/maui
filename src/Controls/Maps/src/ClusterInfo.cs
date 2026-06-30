using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Describes a pin cluster, passed to an image provider callback so the
	/// application can produce a custom icon for the cluster marker.
	/// </summary>
	public sealed class ClusterInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ClusterInfo"/> class.
		/// </summary>
		/// <param name="count">The number of pins in the cluster.</param>
		/// <param name="clusteringIdentifier">The clustering identifier shared by the cluster's pins.</param>
		/// <param name="pins">The pins contained in the cluster.</param>
		/// <param name="location">The geographic location (centroid) of the cluster.</param>
		public ClusterInfo(int count, string clusteringIdentifier, IReadOnlyList<Pin> pins, Location location)
		{
			Count = count;
			ClusteringIdentifier = clusteringIdentifier;
			Pins = pins;
			Location = location;
		}

		/// <summary>Gets the number of pins contained in the cluster.</summary>
		public int Count { get; }

		/// <summary>Gets the clustering identifier shared by the pins in this cluster.</summary>
		public string ClusteringIdentifier { get; }

		/// <summary>Gets the pins contained in this cluster.</summary>
		public IReadOnlyList<Pin> Pins { get; }

		/// <summary>Gets the geographic location (centroid) of the cluster.</summary>
		public Location Location { get; }
	}
}
