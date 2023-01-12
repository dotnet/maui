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

	// TODO: Make public for .NET 8
	internal interface IRoundRectangle : IShape
	{
		internal PathF ClipPathForBounds(Rect bounds, double strokeThickness);
	}
}