using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IWindowOverlay : IDrawable
	{
		/// <summary>
		/// Gets or sets a value indicating whether to disable UI Touch Event Passthrough.
		/// Enable this when you want to enable hit testing the current overlay without
		/// interfacing with the underlaying UI.
		/// </summary>
		bool DisableUITouchEventPassthrough { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to enable handling touch events when
		/// selecting any drawable element on the overlay.
		/// This setting is overridden by <see cref="DisableUITouchEventPassthrough"/>.
		/// </summary>
		bool EnableDrawableTouchHandling { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to draw the window overlay.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets the containing <see cref="IWindow"/>.
		/// </summary>
		IWindow Window { get; }

		/// <summary>
		/// Gets the Density for the layer.
		/// Can be used to pass through Density settings to underlying drawables.
		/// </summary>
		float Density { get; }

		/// <summary>
		/// Gets the current collection of drawable elements on the overlay.
		/// </summary>
		IReadOnlyCollection<IWindowOverlayElement> WindowElements { get; }

		/// <summary>
		/// Gets a value indicating whether the platform touch and drawing layer has been initialized.
		/// If it has not, you will be unable to draw or use hit testing on the Overlay.
		/// </summary>
		bool IsPlatformViewInitialized { get; }

		/// <summary>
		/// Event Handler for touch events on the Overlay.
		/// Called when a user touched the Overlay.
		/// </summary>
		event EventHandler<WindowOverlayTappedEventArgs> Tapped;

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
		/// <param name="element"><see cref="IWindowOverlayElement"/>.</param>
		/// <returns>Boolean indicating if the drawable was added to the collection.</returns>
		bool AddWindowElement(IWindowOverlayElement element);

		/// <summary>
		/// Removes a drawable element from the overlay.
		/// </summary>
		/// <param name="element"><see cref="IWindowOverlayElement"/>.</param>
		/// <returns>Boolean indicating if the drawable was removed from the collection.</returns>
		bool RemoveWindowElement(IWindowOverlayElement element);

		/// <summary>
		/// Removes all drawable elements from the overlay.
		/// </summary>
		void RemoveWindowElements();

		/// <summary>
		/// Initialize the overlay.
		/// </summary>
		bool Initialize();

		/// <summary>
		/// Deinitialize the overlay.
		/// </summary>
		bool Deinitialize();
	}
}