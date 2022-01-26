namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformCanvasStateService : ICanvasStateService<PlatformCanvasState>
	{
		public PlatformCanvasState CreateNew(object context) =>
			new PlatformCanvasState();

		public PlatformCanvasState CreateCopy(PlatformCanvasState prototype) =>
			new PlatformCanvasState(prototype);
	}
}
