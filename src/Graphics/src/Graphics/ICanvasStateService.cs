namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a service for creating and managing canvas state objects.
	/// </summary>
	/// <typeparam name="TState">The type of canvas state managed by this service, which must derive from <see cref="CanvasState"/>.</typeparam>
	public interface ICanvasStateService<TState>
		where TState : CanvasState
	{
		/// <summary>
		/// Creates a new canvas state with the specified context.
		/// </summary>
		/// <param name="context">The platform-specific context for the canvas state.</param>
		/// <returns>A new canvas state instance.</returns>
		TState CreateNew(object context);

		/// <summary>
		/// Creates a copy of an existing canvas state.
		/// </summary>
		/// <param name="prototype">The canvas state to copy.</param>
		/// <returns>A new canvas state instance that is a copy of the prototype.</returns>
		TState CreateCopy(TState prototype);
	}
}
