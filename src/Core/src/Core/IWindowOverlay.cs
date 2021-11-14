using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

#if ANDROID || IOS
using Microsoft.Maui.Graphics.Native;
#endif

namespace Microsoft.Maui
{
	public interface IWindowOverlay : IDisposable
	{
		/// <summary>
		/// Gets or sets a value indicating whether to disable UI Touch Event Passthrough.
		/// Enable this when you want to enable hit testing the current overlay without
		/// interfacing with the underlaying UI.
		/// </summary>
		bool DisableUITouchEventPassthrough { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to draw the window overlay.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets the containing <see cref="IWindow"/>.
		/// </summary>
		IWindow Window { get; }

		/// <summary>
		/// Gets the DPI for the layer.
		/// Can be used to pass through DPI settings to underlying drawables.
		/// </summary>
		public float DPI { get; }

		/// <summary>
		/// Gets the current collection of drawable elements on the overlay.
		/// </summary>
		IReadOnlyCollection<IDrawable> Drawables { get; }

		/// <summary>
		/// Gets a value indicating whether the native touch and drawing layer has been initialized.
		/// If it has not, you will be unable to draw or use hit testing on the Overlay.
		/// </summary>
		bool IsNativeViewInitialized { get; }

		/// <summary>
		/// Event Handler for touch events on the Overlay.
		/// Called when a user touched the Overlay.
		/// </summary>
		event EventHandler<VisualDiagnosticsHitEvent> OnTouch;

		/// <summary>
		/// Invalidates the layer.
		/// Call to force the layer to redraw.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Handles updating the drawing layer when a UI layout change happens.
		/// </summary>
		void HandleUIChange();

		/// <summary>
		/// Adds a new drawable element to the overlay.
		/// </summary>
		/// <param name="drawable"><see cref="IDrawable"/>.</param>
		/// <returns>Boolean indicating if the drawable was added to the collection.</returns>
		bool AddDrawable(IDrawable drawable);

		/// <summary>
		/// Removes a drawable element from the overlay.
		/// </summary>
		/// <param name="drawable"><see cref="IDrawable"/>.</param>
		/// <returns>Boolean indicating if the drawable was removed from the collection.</returns>
		bool RemoveDrawable(IDrawable drawable);

		/// <summary>
		/// Removes all drawable elements from the overlay.
		/// </summary>
		void RemoveDrawables();

		/// <summary>
		/// Initialize the native touch and drawing layer.
		/// </summary>
		bool InitializeNativeLayer();
	}
}
