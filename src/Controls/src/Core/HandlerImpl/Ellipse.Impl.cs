using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Ellipse : IShape
	{
		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);

			path.AppendEllipse(x, y, w, h);

			return path;
		}
	}
}