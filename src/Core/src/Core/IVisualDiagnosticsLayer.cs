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

		HashSet<IAdornerBorder> AdornerBorders { get; }

		IWindow Window { get; }

		public Rectangle Offset { get; }

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
		void AddAdorner(IAdornerBorder adornerBorder);

		/// <summary>
		/// Adds a new adorner to the visual diagnostics layer. Uses the default adorner border for drawing.
		/// </summary>
		/// <param name="visualElement"><see cref="IVisualTreeElement"/>.</param>
		void AddAdorner(IVisualTreeElement visualElement);

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

		void Invalidate();

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
#elif IOS
		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		/// <param name="context"><see cref="IMauiContext"/>.</param>
		/// <param name="nativeLayer">Native OS ViewGroup.</param>
		void InitializeNativeLayer(IMauiContext context, UIKit.UIViewController nativeLayer);
#elif WINDOWS
		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		/// <param name="context"><see cref="IMauiContext"/>.</param>
		/// <param name="nativeLayer">Native OS ViewGroup.</param>
		void InitializeNativeLayer(IMauiContext context, Microsoft.Maui.RootPanel nativeLayer);
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
