#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="ICanvasStateService{T}"/>
	/// that creates new or copies of <see cref="PlatformCanvasState"/>.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
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
