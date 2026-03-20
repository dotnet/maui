#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// The base class for all geometry objects that describe 2D shapes.
	/// </summary>
	public abstract class Geometry : BindableObject, IGeometry
	{
		/// <summary>
		/// Appends the geometry to the specified <see cref="PathF"/>.
		/// </summary>
		/// <param name="path">The path to append the geometry to.</param>
		public abstract void AppendPath(PathF path);

		PathF IShape.PathForBounds(Graphics.Rect bounds)
		{
			var path = new PathF();

			AppendPath(path);

			return path;
		}
	}
}
