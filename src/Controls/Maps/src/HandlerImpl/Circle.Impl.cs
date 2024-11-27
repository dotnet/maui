using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Circle : ICircleMapElement
	{
		/// <inheritdoc cref="IFilledMapElement.Fill"/>
		public Paint? Fill => FillColor?.AsPaint();
	}
}
