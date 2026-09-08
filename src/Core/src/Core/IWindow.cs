using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the ability to create, configure, show, and manage Windows.
	/// </summary>
	public interface IWindow : ITitledElement
	{
		/// <summary>
		/// Gets the current Page displayed in the Window.
		/// </summary>
		IView? Content { get; }

		/// <summary>
		/// Gets the current visual diagnostics overlay for the Window.
		/// </summary>
		IVisualDiagnosticsOverlay VisualDiagnosticsOverlay { get; }

		/// <summary>
		/// Gets the read only collection of Window Overlays on top of the Window.
		/// </summary>
		IReadOnlyCollection<IWindowOverlay> Overlays { get; }

		/// <summary>
		/// Gets the specified X coordinate of the IWindow.
		/// </summary>
		double X { get; }

		/// <summary>
		/// Gets the specified Y coordinate of the IWindow.
		/// </summary>
		double Y { get; }

		/// <summary>
		/// Gets the specified width of the IWindow.
		/// </summary>
		double Width { get; }

		/// <summary>
		/// Gets the specified minimum width constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumWidth { get; }

		/// <summary>
		/// Gets the specified maximum width constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumWidth { get; }

		/// <summary>
		/// Gets the specified height of the IWindow.
		/// </summary>
		double Height { get; }

		/// <summary>
		/// Gets the specified minimum height constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumHeight { get; }

		/// <summary>
		/// Gets the specified maximum height constraint of the IWindow, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumHeight { get; }

		/// <summary>
		/// Direction in which the UI elements are scanned by the eye.
		/// </summary>
		FlowDirection FlowDirection { get; }

		/// <summary>
		/// Adds a Window Overlay to the current Window.
		/// </summary>
		/// <param name="overlay"><see cref="IWindowOverlay"/>.</param>
		/// <returns>Boolean if the window overlay was added.</returns>
		bool AddOverlay(IWindowOverlay overlay);

		/// <summary>
		/// Removes a Window Overlay to the current Window.
		/// </summary>
		/// <param name="overlay"><see cref="IWindowOverlay"/>.</param>
		/// <returns>Boolean if the window overlay was removed.</returns>
		bool RemoveOverlay(IWindowOverlay overlay);

		/// <summary>
		/// Occurs when the Window is created.
		/// </summary>
		void Created();

		/// <summary>
		/// Occurs when the Window is resumed from a sleeping state.
		/// </summary>
		void Resumed();

		/// <summary>
		/// Occurs when the Window is activated.
		/// </summary>
		void Activated();

		/// <summary>
		/// Occurs when the Window is deactivated.
		/// </summary>
		void Deactivated();

		/// <summary>
		/// Occurs when the Window is stopped.
		/// </summary>
		void Stopped();

		/// <summary>
		/// Occurs when the Window is destroyed.
		/// </summary>
		void Destroying();

		/// <summary>
		/// Occurs when the Window is entering a background state.
		/// </summary>
		void Backgrounding(IPersistedState state);

		/// <summary>
		/// Occurs when the back button is pressed.
		/// </summary>
		/// <returns>Whether or not the back navigation was handled.</returns>
		bool BackButtonClicked();

		void DisplayDensityChanged(float displayDensity);

		void FrameChanged(Rect frame);

		float RequestDisplayDensity();

#if WINDOWS || MACCATALYST
		ITitleBar? TitleBar => null;
		/// <summary>Gets a value indicating whether the window can be minimized.</summary>
		bool IsMinimizable => true;
		/// <summary>Gets a value indicating whether the window can be maximized.</summary>
		bool IsMaximizable => true;
#endif

#if WINDOWS
		Rect[]? TitleBarDragRectangles => null;
#endif
	}
}