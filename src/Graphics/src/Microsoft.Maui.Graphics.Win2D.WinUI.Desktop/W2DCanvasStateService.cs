namespace Microsoft.Maui.Graphics.Win2D
{
	public class W2DCanvasStateService : ICanvasStateService<W2DCanvasState>
	{
		public W2DCanvasState CreateNew(object context)
		{
			return new W2DCanvasState((W2DCanvas)context);
		}

		public W2DCanvasState CreateCopy(W2DCanvasState prototype)
		{
			return new W2DCanvasState(prototype);
		}
	}
}
