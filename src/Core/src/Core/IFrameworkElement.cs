#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a framework-level set of properties, events, and methods for .NET MAUI elements. 
	/// </summary>
	public interface IFrameworkElement
	{
		/// <summary>
		/// Gets a value indicating whether this FrameworkElement is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Gets or sets the View Handler of the FrameworkElement.
		/// </summary>
		IFrameworkElementHandler? Handler { get; set; }

		/// <summary>
		/// Gets the Parent of the Element.
		/// </summary>
		IFrameworkElement? Parent { get; }

		/// <summary>
		/// Id used by automation tools to interact with this FrameworkElement
		/// </summary>
		string AutomationId { get; }

		/// <summary>
		/// Adds semantics to every FrameworkElement for accessibility
		/// </summary>
		Semantics Semantics { get; }
	}
}
