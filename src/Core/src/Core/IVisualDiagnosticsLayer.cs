using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

#if ANDROID || IOS
using Microsoft.Maui.Graphics.Native;
#endif

namespace Microsoft.Maui
{
	public interface IVisualDiagnosticsLayer
	{
		/// <summary>
		/// Gets or sets a value indicating whether to disable UI Touch Event Passthrough.
		/// Enable this when you want to enable hit testing the current visual diagnostics layer without
		/// interfacing with the underlaying UI.
		/// </summary>
		bool DisableUITouchEventPassthrough { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to automatically scroll to an element when adding an adorner, if available.
		/// </summary>
		bool AutoScrollToElement { get; set; }

		/// <summary>
		/// Gets the hash set for Adroner Borders. When filled, they will be drawn on the screen.
		/// </summary>
		HashSet<IAdornerBorder> AdornerBorders { get; }

		/// <summary>
		/// Gets the containing <see cref="IWindow"/>.
		/// </summary>
		IWindow Window { get; }

		/// <summary>
		/// Gets the offset rectangle used to adjust the native drawing bounds for a given adorner border.
		/// Used when the underlying operating system may not give exact placement for where elements are.
		/// Ex. Android and the Status Bar.
		/// </summary>
		public Rectangle Offset { get; }

		/// <summary>
		/// Gets the DPI for the layer.
		/// Can be used to pass through DPI settings to underlying Adorner Borders.
		/// </summary>
		public float DPI { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the native touch and drawing layer has been initialized.
		/// If it has not, you will be unable to draw or use hit testing on the visual diagnostics layer.
		/// </summary>
		bool IsNativeViewInitialized { get; }

		/// <summary>
		/// Event Handler for touch events on the visual diagnostics layer.
		/// Called when a user touched the visual diagnostics layer.
		/// </summary>
		event EventHandler<VisualDiagnosticsHitEvent> OnTouch;

		/// <summary>
		/// Adds a new adorner to the visual diagnostics layer.
		/// </summary>
		/// <param name="adornerBorder"><see cref="IAdornerBorder"/>.</param>
		/// <param name="scrollToView">When adding the adorner, scroll to the element. Only applies if the element is contained in an <see cref="IScrollView"/>.</param>
		void AddAdorner(IAdornerBorder adornerBorder, bool scrollToView);

		/// <summary>
		/// Adds a new adorner to the visual diagnostics layer. Uses the default adorner border for drawing.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/>.</param>
		/// <param name="scrollToView">When adding the adorner, scroll to the element. Only applies if the element is contained in an <see cref="IScrollView"/>.</param>
		void AddAdorner(IVisualTreeElement visualElement, bool scrollToView);

		/// <summary>
		/// Removes adorner from visual diagnostics layer.
		/// </summary>
		/// <param name="adornerBorder"><see cref="IAdornerBorder"/>.</param>
		void RemoveAdorner(IAdornerBorder adornerBorder);

		/// <summary>
		/// Removes all adorners from the visual diagnostics layer.
		/// </summary>
		void RemoveAdorners();

		/// <summary>
		/// Removes all adorners containing the inner <see cref="IVisualTreeElement"/>.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/>.</param>
		void RemoveAdorners(IVisualTreeElement visualElement);

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
		/// Invalidates the layer.
		/// Call to force the layer to redraw.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Automatically scroll to a given element within the view, if available.
		/// </summary>
		/// <param name="element">Element to scroll to.</param>
		void ScrollToView(IVisualTreeElement element);

#if ANDROID || IOS
		NativeGraphicsView? VisualDiagnosticsGraphicsView { get; }
#endif

#if ANDROID
		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		/// <param name="context"><see cref="IMauiContext"/>.</param>
		/// <param name="nativeLayer">Native OS ViewGroup.</param>
		void InitializeNativeLayer(IMauiContext context, Android.Views.ViewGroup nativeLayer);

		/// <summary>
		/// Gets the Scroll Views in a given window, to be handled by the layer for
		/// when they scroll to update the underlying adorners.
		/// </summary>
		HashSet<Tuple<IScrollView, Android.Views.View>> ScrollViews { get; }
#elif IOS
		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		/// <param name="context"><see cref="IMauiContext"/>.</param>
		/// <param name="nativeLayer">Native OS Window.</param>
		void InitializeNativeLayer(IMauiContext context, UIKit.UIWindow nativeLayer);

		/// <summary>
		/// Gets the Scroll Views in a given window, to be handled by the layer for
		/// when they scroll to update the underlying adorners.
		/// </summary>
		HashSet<Tuple<IScrollView, IDisposable>> ScrollViews { get; }
#elif WINDOWS
		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		/// <param name="context"><see cref="IMauiContext"/>.</param>
		/// <param name="nativeLayer">Native OS ViewGroup.</param>
		void InitializeNativeLayer(IMauiContext context, Microsoft.Maui.RootPanel nativeLayer);

		/// <summary>
		/// Gets the Scroll Views in a given window, to be handled by the layer for
		/// when they scroll to update the underlying adorners.
		/// </summary>
		HashSet<Tuple<IScrollView, Microsoft.UI.Xaml.Controls.ScrollViewer>> ScrollViews { get; }
#else
		/// <summary>
		/// Gets the Scroll Views in a given window, to be handled by the layer for
		/// when they scroll to update the underlying adorners.
		/// </summary>
		HashSet<Tuple<IScrollView, object>> ScrollViews { get; }
#endif
	}

	public class VisualDiagnosticsHitEvent
	{
		public VisualDiagnosticsHitEvent(Point point, IList<IVisualTreeElement> elements)
		{
			this.Point = point;
			this.VisualTreeElements = elements;
		}

		public IList<IVisualTreeElement> VisualTreeElements { get; }

		public Point Point { get; }
	}
}
