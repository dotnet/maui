using System;
using Gtk;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk
{
	/// <summary>
	/// Base interface for VisualElement renderer.
	/// </summary>
	public interface IVisualElementRenderer : IRegisterable, IDisposable
	{
		/// <summary>
		/// Gets the VisualElement associated with this renderer.
		/// </summary>
		/// <value>The VisualElement.</value>
		VisualElement Element
		{
			get;
		}

		/// <summary>
		/// Gets the native view associated with this renderer.
		/// </summary>
		/// <value>The native view.</value>
		Widget NativeView
		{
			get;
		}

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		/// <summary>
		/// Sets the VisualElement associated with this renderer.
		/// </summary>
		/// <param name="element">New element.</param>
		void SetElement(VisualElement element);

		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);

		void UpdateLayout();

	}
}
