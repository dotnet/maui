using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a <see cref="IView"/> that displays a map.
	/// </summary>
	public interface IMap : IView
	{
		/// <summary>
		/// The currently visible MapSpan of this Map.
		/// </summary>
		MapSpan? VisibleRegion { get; set; }

		/// <summary>
		/// Gets the display type of map that can be shown.
		/// </summary>
		MapType MapType { get; }

		/// <summary>
		/// Get whether the Map is showing the user's current location.
		/// </summary>
		bool IsShowingUser { get; }

		/// <summary>
		/// Get whether the Map is allowed to scroll.
		/// </summary>
		bool IsScrollEnabled { get; }

		/// <summary>
		/// Get whether this Map is allowed to zoom.
		/// </summary>
		bool IsZoomEnabled { get; }

		/// <summary>
		/// Get whether this Map is showing traffic information.
		/// </summary>
		bool IsTrafficEnabled { get; }

		/// <summary>
		/// The pins that are to be shown on this Map.
		/// </summary>
		IList<IMapPin> Pins { get; }

		/// <summary>
		/// The pins that are to be shown on this Map.
		/// </summary>
		IList<IMapElement> Elements { get; }

		/// <summary>
		/// Gets the last known user location from the map, or null if not available.
		/// </summary>
		/// <remarks>
		/// This property requires <see cref="IsShowingUser"/> to be set to true.
		/// The location is updated as the user moves and the map receives location updates.
		/// </remarks>
		Location? LastUserLocation { get; }

		/// <summary>
		/// Method called by the handler when user clicks on the Map.
		/// </summary>
		void Clicked(Location position);

		/// <summary>
		/// Method called by the handler when the user's location is updated.
		/// </summary>
		/// <param name="location">The new user location.</param>
		void UserLocationUpdated(Location location);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region.
		/// </summary>
		void MoveToRegion(MapSpan region);
	}
}
