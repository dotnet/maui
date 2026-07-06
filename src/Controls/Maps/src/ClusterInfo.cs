using System;
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
			ClusteringIdentifier = clusteringIdentifier ?? throw new ArgumentNullException(nameof(clusteringIdentifier));
			Pins = pins ?? throw new ArgumentNullException(nameof(pins));
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}

		/// <summary>Gets the number of pins contained in the cluster.</summary>
		/// <remarks>This is the authoritative member count, independent of how many entries <see cref="Pins"/> holds.</remarks>
		public int Count { get; }

		/// <summary>Gets the clustering identifier shared by the pins in this cluster.</summary>
		/// <remarks>Falls back to <see cref="Pin.DefaultClusteringIdentifier"/> when no member pin could be resolved.</remarks>
		public string ClusteringIdentifier { get; }

		/// <summary>Gets the pins contained in this cluster.</summary>
		/// <remarks>
		/// On some platforms (iOS) not every cluster member can be resolved back to a <see cref="Pin"/>,
		/// so this list can contain fewer than <see cref="Count"/> entries - use <see cref="Count"/> for badge numbers.
		/// </remarks>
		public IReadOnlyList<Pin> Pins { get; }

		/// <summary>Gets the geographic location (centroid) of the cluster.</summary>
		public Location Location { get; }
	}
}
