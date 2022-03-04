using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Adorner.
	/// </summary>
	public class RectangleAdorner : IAdorner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleAdorner"/> class.
		/// </summary>
		/// <param name="view">An <see cref="IView"/> to create the adorner around.</param>
		/// <param name="density">Override density setting. Default: 1</param>
		/// <param name="offset">Offset point used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleAdorner(IView view, float density = 1, Point? offset = null, Color? fillColor = null, Color? strokeColor = null)
		{
			FillColor = fillColor ?? Color.FromRgba(225, 0, 0, 125);
			StrokeColor = strokeColor ?? Color.FromRgba(225, 0, 0, 125);
			Offset = offset ?? Point.Zero;
			VisualView = view;
			Density = density;
			DrawnRectangle = Rect.Zero;
		}

		/// <inheritdoc/>
		public float Density { get; }

		/// <inheritdoc/>
		public IView VisualView { get; }

		public Point Offset { get; }

		public Color FillColor { get; }

		public Color StrokeColor { get; }

		public Rect DrawnRectangle { get; private set; }

		/// <inheritdoc/>
		public virtual bool Contains(Point point) =>
			DrawnRectangle.Contains(point);

		/// <inheritdoc/>
		public virtual void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.FillColor = FillColor;
			canvas.StrokeColor = StrokeColor;

			var boundingBox = VisualView.GetBoundingBox();
			var x = (boundingBox.X / Density) + Offset.X;
			var y = (boundingBox.Y / Density) + Offset.Y;
			var width = boundingBox.Width / Density;
			var height = boundingBox.Height / Density;

			DrawnRectangle = new Rect(x, y, width, height);

			canvas.FillRectangle(DrawnRectangle);
		}
	}
}