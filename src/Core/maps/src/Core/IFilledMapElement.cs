using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a visual element on the map control that has a fill color.
	/// </summary>
	public interface IFilledMapElement : IMapElement
	{
		/// <summary>
		/// Gets the fill color.
		/// </summary>
		Paint? Fill { get; }
	}
}
