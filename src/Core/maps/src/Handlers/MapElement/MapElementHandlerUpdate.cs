using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Represents an update to a map element, including its index in the collection.
	/// </summary>
	/// <param name="Index">The index of the map element in the collection.</param>
	/// <param name="MapElement">The map element that was updated.</param>
	public record MapElementHandlerUpdate(int Index, IMapElement MapElement);
}
