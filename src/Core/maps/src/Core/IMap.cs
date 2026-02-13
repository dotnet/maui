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
		/// Method called by the handler when user clicks on the Map.
		/// </summary>
		void Clicked(Location position);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region.
		/// </summary>
		void MoveToRegion(MapSpan region);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region,
		/// with control over whether the transition is animated.
		/// </summary>
		/// <param name="region">The region to display.</param>
		/// <param name="animated">Whether the transition should be animated.</param>
		void MoveToRegion(MapSpan region, bool animated);
	}
}
