using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RoundRectangleHandler
	{
		public static void MapCornerRadius(RoundRectangleHandler handler, RoundRectangle roundRectangle)
		{
			handler.PlatformView?.InvalidateShape(roundRectangle);
		}
	}
}