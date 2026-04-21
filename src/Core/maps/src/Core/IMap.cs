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
		/// Gets whether pin clustering is enabled for this map.
		/// </summary>
		bool IsClusteringEnabled { get; }

		/// <summary>
		/// Gets the custom map style as a JSON string.
		/// </summary>
		/// <remarks>
		/// This property is only supported on Android where it accepts a Google Maps style JSON string.
		/// Use the Google Maps Styling Wizard (https://mapstyle.withgoogle.com/) to generate a style JSON.
		/// On iOS, MacCatalyst, and Windows this property has no effect as the native map controls
		/// do not support custom JSON styling.
		/// </remarks>
#if !NETSTANDARD
		[System.Runtime.Versioning.UnsupportedOSPlatform("ios")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("maccatalyst")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
#endif
		string? MapStyle { get; }

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
		/// Method called by the handler when a cluster is clicked.
		/// </summary>
		/// <param name="pins">The pins in the cluster.</param>
		/// <param name="location">The location of the cluster.</param>
		/// <returns><see langword="true"/> if the click was handled and default behavior should be suppressed; otherwise <see langword="false"/>.</returns>
		bool ClusterClicked(IReadOnlyList<IMapPin> pins, Location location);

		/// <summary>
		/// Method called by the handler when user long-presses on the Map.
		/// </summary>
		void LongClicked(Location position);

		/// <summary>
		/// Method called by the handler when the user's location is updated.
		/// </summary>
		/// <param name="location">The new user location.</param>
		void UserLocationUpdated(Location location);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region.
		/// </summary>
		void MoveToRegion(MapSpan region);

		/// <summary>
		/// Shows the info window for the specified pin.
		/// </summary>
		/// <param name="pin">The pin whose info window should be shown.</param>
		void ShowInfoWindow(IMapPin pin);

		/// <summary>
		/// Hides the info window for the specified pin.
		/// </summary>
		/// <param name="pin">The pin whose info window should be hidden.</param>
		void HideInfoWindow(IMapPin pin);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region,
		/// with control over whether the transition is animated.
		/// </summary>
		/// <param name="region">The region to display.</param>
		/// <param name="animated">Whether the transition should be animated.</param>
		void MoveToRegion(MapSpan region, bool animated);
	}
}
