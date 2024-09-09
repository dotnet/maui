using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a visual element on the map control shaped like a circle.
	/// </summary>
	public interface ICircleMapElement : IMapElement, IFilledMapElement
	{
		/// <summary>
		/// Gets the center location.
		/// </summary>
		Location Center { get; }

		/// <summary>
		/// Gets the radius.
		/// </summary>
		Distance Radius { get; }
	}
}
