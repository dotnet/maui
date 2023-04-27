#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
#if MAUI_GRAPHICS_WIN2D
	[System.Obsolete("Use Microsoft.Maui.Graphics.Platform.PlatformCanvasStateService instead.")]
	public class W2DCanvasStateService
#else
	public class PlatformCanvasStateService
#endif
		: ICanvasStateService<PlatformCanvasState>
	{
		public PlatformCanvasState CreateNew(object context)
		{
			return new PlatformCanvasState((PlatformCanvas)context);
		}

		public PlatformCanvasState CreateCopy(PlatformCanvasState prototype)
		{
			return new PlatformCanvasState(prototype);
		}
	}
}
