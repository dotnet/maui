using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays a map.
	/// </summary>
	public interface IMap : IView
	{
		/// <summary>
		/// Gets the display type of map that can be shown.
		/// </summary>
		MapType MapType { get; }

		/// <summary>
		/// Get whether this Map is showing the user's current location.
		/// </summary>
		bool IsShowingUser { get; }
	}
}
