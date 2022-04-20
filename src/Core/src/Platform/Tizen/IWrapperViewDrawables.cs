using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public interface IWrapperViewDrawables : IDrawable
	{
		IDrawable? ShadowDrawable { get; set; }

		IDrawable? BackgroundDrawable { get; set; }

		IDrawable? BorderDrawable { get; set; }
	}
}
