namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides a base definition class for shape elements, such as
	/// Ellipse, Polygon, or Rectangle.
	/// </summary>
	public interface IShape
	{
		PathF PathForBounds(Rect bounds);
	}

	internal interface IRoundRectangle : IShape
	{
		PathF InnerPathForBounds(Rect bounds, float strokeThickness);
	}

	internal interface IVersionedShape : IShape
	{
		int Version { get; }
	}
}