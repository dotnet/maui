using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Rectangle Grid Adorner.
	/// </summary>
	public class RectangleGridAdorner : RectangleAdorner
	{
		const int LineMaxLength = 10000;

		/// <summary>
		/// Initializes a new instance of the <see cref="RectangleGridAdorner"/> class.
		/// </summary>
		/// <param name="view">An <see cref="IView"/> to create the Adorner around.</param>
		/// <param name="density">Override density setting. Default: 1</param>
		/// <param name="offset">Offset Point used for positioning drawable object. Default: null</param>
		/// <param name="fillColor">Canvas Fill Color.</param>
		/// <param name="strokeColor">Canvas Stroke Color.</param>
		public RectangleGridAdorner(IView view, float density = 1, Point? offset = null, Color? fillColor = null, Color? strokeColor = null)
			: base(view, density, offset, fillColor, strokeColor)
		{
		}

		public override void Draw(ICanvas canvas, RectF dirtyRect)
		{
			base.Draw(canvas, dirtyRect);

			var y = (float)DrawnRectangle.Y;
			var x = (float)DrawnRectangle.X;
			var width = (float)DrawnRectangle.Width;
			var height = (float)DrawnRectangle.Height;

			canvas.DrawLine(0, y, LineMaxLength, y);
			canvas.DrawLine(0, y + height, LineMaxLength, y + height);
			canvas.DrawLine(x, 0, x, LineMaxLength);
			canvas.DrawLine(x + width, 0, x + width, LineMaxLength);
		}
	}
}