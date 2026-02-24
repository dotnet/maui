using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a map element that has a path defined by a collection of geographic locations.
	/// </summary>
	public interface IGeoPathMapElement : IMapElement, IList<Location>
	{
	}
}
