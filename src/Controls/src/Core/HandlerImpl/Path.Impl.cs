using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Path : IShape
	{
		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			Data.AppendToPath(path);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}
