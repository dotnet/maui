namespace Microsoft.Maui.Graphics
{
	public interface ICanvasStateService<TState>
		where TState : CanvasState
	{
		TState CreateNew(object context);

		TState CreateCopy(TState prototype);
	}
}
