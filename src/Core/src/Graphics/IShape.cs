namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides a base definition class for shape elements, such as
	/// Ellipse, Polygon, or Rectangle.
	/// </summary>
	public interface IShape
	{
		PathF PathForBounds(Rectangle bounds);
	}
}