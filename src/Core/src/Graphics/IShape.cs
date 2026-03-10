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
		PathF InnerPath();
	}

	/// <summary>
	/// Provides a version for the <see cref="IShape"/> so that every time something
	/// changes in the shape definition the <see cref="Version"/> is increased by 1.
	/// </summary>
	/// <remarks>
	/// This is especially useful in handler's mappers to improve the performance
	/// by avoiding useless calls to platform when nothing has changed.
	/// </remarks>
	internal interface IVersionedShape : IShape
	{
		int Version { get; }
	}
}