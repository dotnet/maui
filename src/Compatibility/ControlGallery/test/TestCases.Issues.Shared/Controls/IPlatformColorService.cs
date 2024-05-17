using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public interface IPlatformColorService
	{
		Color GetConvertedColor(bool shouldCrash);
	}
}