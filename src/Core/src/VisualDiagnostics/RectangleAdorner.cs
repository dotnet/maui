using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Adorner.
	/// </summary>
	public class RectangleAdorner : IAdorner
	{

		readonly AdornerModel model = new();

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

			// Sanity check
			if (Density < 0.1)
			{
				Density = 1;
				Debug.Fail($"Invalid density {density}");
			}
		}

		/// <inheritdoc/>
		public float Density { get; }

		/// <inheritdoc/>
		public IView VisualView { get; }

		public Point Offset { get; }

		public Color FillColor { get; }

		public Color StrokeColor { get; }

		public Rect DrawnRectangle => model.BoundingBox;

		/// <inheritdoc/>
		public virtual bool Contains(Point point)
		{
			if (model.BoundingBox.Contains(point))
			{
				return true;
			}

			if (model.MarginZones.Any(r => r.Contains(point)))
			{
				return true;
			}

			return false;
		}

		public virtual void Draw(ICanvas canvas, RectF dirtyRect)
		{
			UpdateModel();

			// Draw highlight rectangle
			canvas.FillColor = FillColor;
			canvas.StrokeColor = StrokeColor;
			canvas.FillRectangle(model.BoundingBox);
		}

		void UpdateModel()
		{
			Rect box = VisualView.GetBoundingBox();

			box.X = box.X + Offset.X;
			box.Y = box.Y + Offset.Y;

			Matrix4x4 transform = VisualView.GetViewTransform();
			Thickness margin;
			if (VisualView is IView view)
			{
				margin = view.Margin;
			}
			else
			{
				margin = new Thickness();
			}

			model.Update(box, margin, transform, Density);
		}
	}
}