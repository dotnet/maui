#nullable disable
using Microsoft.Maui.Devices;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents the geometry of a rectangle.
	/// </summary>
	public class RectangleGeometry : Geometry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleGeometry"/> class.
		/// </summary>
		public RectangleGeometry()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleGeometry"/> class with the specified rectangle bounds.
		/// </summary>
		/// <param name="rect">The rectangle bounds for the geometry.</param>
		public RectangleGeometry(Rect rect)
		{
			Rect = rect;
		}

		/// <summary>Bindable property for <see cref="Rect"/>.</summary>
		public static readonly BindableProperty RectProperty =
			BindableProperty.Create(nameof(Rect), typeof(Rect), typeof(RectangleGeometry), new Rect());

		/// <summary>
		/// Gets or sets the rectangle bounds for this geometry. This is a bindable property.
		/// </summary>
		public Rect Rect
		{
			set { SetValue(RectProperty, value); }
			get { return (Rect)GetValue(RectProperty); }
		}

		public override void AppendPath(Graphics.PathF path)
		{
			float x = (float)Rect.X;
			float y = (float)Rect.Y;
			float w = (float)Rect.Width;
			float h = (float)Rect.Height;

			path.AppendRectangle(x, y, w, h);
		}
	}
}
