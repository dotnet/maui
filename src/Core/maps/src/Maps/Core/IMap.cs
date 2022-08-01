using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a <see cref="IView"/> that displays a map.
	/// </summary>
	public interface IMap : IView
	{

		MapSpan LastMoveToRegion { get; }

		MapSpan VisibleRegion { get; set;  }

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
		bool HasScrollEnabled { get; }

		/// <summary>
		/// Get whether this Map is allowed to zoom.
		/// </summary>
		bool HasZoomEnabled { get; }

		/// <summary>
		/// Get whether this Map is showing traffic information.
		/// </summary>
		bool HasTrafficEnabled { get; }

		/// <summary>
		/// The pins that are to be shown on this Map.
		/// </summary>
		IList<IMapPin> Pins { get; }


		bool MoveToLastRegionOnLayoutChange { get; }

		public void SendMapClick(Location position);

	}
}
