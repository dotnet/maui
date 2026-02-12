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
		/// Method called by the handler when user clicks on the Map.
		/// </summary>
		void Clicked(Location position);

		/// <summary>
		/// Moves the map so that it displays the specified <see cref="MapSpan"/> region.
		/// </summary>
		void MoveToRegion(MapSpan region);
	}
}
