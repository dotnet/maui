using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Circle : ICircleMapElement
	{
		public Paint? Fill => FillColor?.AsPaint();
	}
}
