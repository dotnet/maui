using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	public interface IElement
	{
		/// <summary>
		/// Gets or sets the View Handler of the Element.
		/// </summary>
		IElementHandler? Handler { get; set; }

		/// <summary>
		/// Gets the Parent of the Element.
		/// </summary>
		IElement? Parent { get; }

		/// <summary>
		/// Creates the Handler for the Element.
		/// </summary>
		IElementHandler? GetElementHandler(IMauiContext context); // TODO rename to `CreateElementHandler`?

		/// <summary>
		/// Gets the Handler Type for the Element.
		/// </summary>
		Type? GetElementHandlerType();
	}
}