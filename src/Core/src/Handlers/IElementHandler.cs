namespace Microsoft.Maui
{
	/// <summary>
	/// Defines the core behavior necessary to create a custom element handler.
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/"> Conceptual documentation on handlers</seealso>
	/// </summary>
	public interface IElementHandler
	{
		/// <summary>
		/// Sets the .NET MAUI context for the element handler.  
		/// </summary>
		/// <param name="mauiContext">The .NET MAUI context to set.</param>
		void SetMauiContext(IMauiContext mauiContext);

		/// <summary>
		/// Sets the cross-platform virtual view associated with the handler.  
		/// </summary>
		/// <param name="view">The element to handle.</param>
		void SetVirtualView(IElement view);

		/// <summary>
		/// Updates the value of the specified property on the handler.
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
		/// Gets the platform-specific view object associated with the handler.
		/// </summary>
		object? PlatformView { get; }

		/// <summary>
		/// Gets the cross-platform virtual view associated with the handler.
		/// </summary>
		IElement? VirtualView { get; }

		/// <summary>
		/// Gets the .NET MAUI context associated with the element.
		/// </summary>
		IMauiContext? MauiContext { get; }
	}

#if ANDROID
	internal interface IElementHandlerWithAndroidContext<THandler>
		where THandler : IElementHandler
	{
		static abstract THandler CreateHandler(global::Android.Content.Context? context);
	}
#endif
}