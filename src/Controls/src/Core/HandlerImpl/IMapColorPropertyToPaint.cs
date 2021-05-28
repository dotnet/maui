using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	// Provides a way for controls which implement IColorElement to update core properties
	// when updating legacy Color property values
	internal interface IMapColorPropertyToPaint
	{
		void MapColorPropertyToPaint(Color color);
	}
}