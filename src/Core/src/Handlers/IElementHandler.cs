namespace Microsoft.Maui
{
	/// <summary>
	/// Allows you to create customized element handlers.
	/// </summary>
	public interface IElementHandler
	{
		/// <summary>  
		/// Sets the Maui context for the element handler.  
		/// </summary>  
		/// <param name="mauiContext">The Maui context to set.</param>  
		void SetMauiContext(IMauiContext mauiContext);
		
		/// <summary>  
		/// Sets the element to be handled by the renderer.  
		/// </summary>  
		/// <param name="view">The element to handle.</param>  
		void SetVirtualView(IElement view);

		/// <summary>  
		/// Updates the value of the specified property on the element.  
		/// </summary>  
		/// <param name="property">The name of the property to update.</param>  
		void UpdateValue(string property);

		/// <summary>  
		/// Invokes the specified command on the element with the given arguments.  
		/// </summary>  
		/// <param name="command">The name of the command to invoke.</param>  
		/// <param name="args">Optional arguments to pass to the command.</param>  
		void Invoke(string command, object? args = null);

		/// <summary>  
		/// Disconnects the element handler from the element for clean up.  
		/// </summary>  
		void DisconnectHandler();

		/// <summary>  
		/// Gets the platform-specific view object associated with the element.  
		/// </summary>  
		object? PlatformView { get; }

		/// <summary>  
		/// Gets the cross-platform element being handled by the renderer.  
		/// </summary>  
		IElement? VirtualView { get; }

		/// <summary>  
		/// Gets the Maui context associated with the element.  
		/// </summary>  
		IMauiContext? MauiContext { get; }
	}
}