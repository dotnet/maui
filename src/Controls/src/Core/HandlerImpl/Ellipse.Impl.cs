using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Ellipse : IShape
	{
		public override PathF GetPath()
		{
			var path = new PathF();
			
			float x = (float)StrokeThickness;
			float y = (float)StrokeThickness;
			float w = (float)(Width - StrokeThickness * 2);
			float h = (float)(Height - StrokeThickness * 2);

			path.AppendEllipse(x, y, w, h);

			return path;
		}
	}
}