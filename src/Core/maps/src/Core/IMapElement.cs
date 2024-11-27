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
	}
}
