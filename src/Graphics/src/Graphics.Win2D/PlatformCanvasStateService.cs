namespace Microsoft.Maui.Graphics.Win2D
{
	public class PlatformCanvasStateService : ICanvasStateService<PlatformCanvasState>
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
