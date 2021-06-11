namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Enable you to describe the geometry of a 2D shape.
	/// </summary>
	public interface IGeometry
	{
		PathF PathForBounds(Rectangle bounds);
	}
}