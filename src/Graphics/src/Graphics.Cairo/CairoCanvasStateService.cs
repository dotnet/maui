namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// Creates, copies and resets <see cref="CairoCanvasState"/> instances for a <see cref="CairoCanvas"/>.
	/// </summary>
	public class CairoCanvasStateService : ICanvasStateService<CairoCanvasState>
	{
		/// <inheritdoc/>
		public CairoCanvasState CreateNew(object context) => new CairoCanvasState();

		/// <inheritdoc/>
		public CairoCanvasState CreateCopy(CairoCanvasState prototype) => new CairoCanvasState(prototype);

		/// <summary>Resets <paramref name="state"/> to the default values.</summary>
		/// <param name="state">The state to reset.</param>
		public void Reset(CairoCanvasState state) => state.Reset();
	}
}
