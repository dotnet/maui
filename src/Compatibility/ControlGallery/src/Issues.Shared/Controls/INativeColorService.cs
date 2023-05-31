using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public interface INativeColorService
	{
		Color GetConvertedColor(bool shouldCrash);
	}
}