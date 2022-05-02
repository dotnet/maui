using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IVisualDiagnosticsOverlay : IWindowOverlay
	{
		/// <summary>
		/// Gets or sets a value indicating whether to automatically scroll to an element when adding an adorner, if available.
		/// </summary>
		bool ScrollToElement { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to enable the element selector on the diagnostics overlay.
		/// If enabled, this will also enable <see cref="IWindowOverlay.DisableUITouchEventPassthrough"/>.
		/// </summary>
		bool EnableElementSelector { get; set; }

		/// <summary>
		/// Gets the offset point used to adjust the platform drawing bounds for a given Adorner.
		/// Used when the underlying operating system may not give exact placement for where elements are.
		/// Ex. Android and the Status Bar.
		/// </summary>
		Point Offset { get; }

		/// <summary>
		/// Adds a new adorner to the Visual Diagnostics Overlay.
		/// </summary>
		/// <param name="adorner"><see cref="IAdorner"/>.</param>
		/// <param name="scrollToElement">When adding the adorner, scroll to the element. Only applies if the element is contained in an <see cref="IScrollView"/>.</param>
		bool AddAdorner(IAdorner adorner, bool scrollToElement);

		/// <summary>
		/// Adds a new adorner to the Visual Diagnostics Overlay. Uses the default Adorner for drawing.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/>.</param>
		/// <param name="scrollToElement">When adding the adorner, scroll to the element. Only applies if the element is contained in an <see cref="IScrollView"/>.</param>
		bool AddAdorner(IVisualTreeElement visualElement, bool scrollToElement);

		/// <summary>
		/// Removes adorner from Visual Diagnostics Overlay.
		/// </summary>
		/// <param name="adorner"><see cref="IAdorner"/>.</param>
		bool RemoveAdorner(IAdorner adorner);

		/// <summary>
		/// Removes all adorners from the Visual Diagnostics Overlay.
		/// </summary>
		void RemoveAdorners();

		/// <summary>
		/// Removes all adorners containing the inner <see cref="IVisualTreeElement"/>.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/>.</param>
		bool RemoveAdorners(IVisualTreeElement visualElement);

		/// <summary>
		/// Adds scrollable element handles attached to <see cref="IScrollView"/>.
		/// Used for tracking when a user has scrolled, in order to update the layer to redraw.
		/// </summary>
		void AddScrollableElementHandlers();

		/// <summary>
		/// Adds scrollable element handle attached to <see cref="IScrollView"/>.
		/// Used for tracking when a user has scrolled, in order to update the layer to redraw.
		/// </summary>
		void AddScrollableElementHandler(IScrollView view);

		/// <summary>
		/// Removes any existing scrollable element handles attached to <see cref="IScrollView"/>.
		/// </summary>
		void RemoveScrollableElementHandler();

		/// <summary>
		/// Automatically scroll to a given element within the view, if available.
		/// </summary>
		/// <param name="element">Element to scroll to.</param>
		void ScrollToView(IVisualTreeElement element);

		/// <summary>
		/// Gets the Scroll Views in a given window, to be handled by the layer for
		/// when they scroll to update the underlying adorners.
		/// </summary>
		IReadOnlyCollection<IScrollView> ScrollViews { get; }
	}
}