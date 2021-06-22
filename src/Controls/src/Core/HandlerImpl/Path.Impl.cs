using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Path : IShape
	{
		public override PathF GetPath()
		{
			var path = new PathF();

			Data.AppendPath(path);

			return path;
		}
	}
}
