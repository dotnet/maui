namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents an element that can be added to the map control.
	/// </summary>
	public interface IMapElement : IElement, IStroke
	{
		/// <summary>
		/// Gets or sets the platform counterpart of this map element.
		/// </summary>
		object? MapElementId { get; set; }

		/// <summary>
		/// Gets a value indicating whether the map element is visible on the map.
		/// </summary>
		bool IsVisible { get; }

		/// <summary>
		/// Gets the z-index of the map element, which controls its draw order relative to other elements.
		/// Higher values are drawn on top of lower values.
		/// </summary>
		int ZIndex { get; }
	}
}
