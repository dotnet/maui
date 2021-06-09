using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Rectangle : IShape
	{
		public override PathF GetPath()
		{
			var path = new PathF();

			path.AppendRoundedRectangle(0f, 0f, (float)Width, (float)Height, (float)RadiusY + (float)RadiusX);

			return path;
		}
	}
}