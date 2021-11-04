using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a view that can be drawn on using drawing commands.
	/// </summary>
	public interface IGraphicsView : IView
	{
		/// <summary>
		/// Define the drawing content.
		/// </summary>
		IDrawable Drawable { get; }

		/// <summary>
		/// Informs the canvas that it needs to redraw itself.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Occurs when the the surface received a touch event.
		/// </summary>
		/// <param name="e">Provides data for the Touch event.</param>
		void OnTouch(TouchEventArgs e);
	}

	public enum TouchAction
	{
		Pressed,
		Moved,
		Released,
		Cancelled
	}

	public class TouchEventArgs : EventArgs
	{
		public TouchEventArgs()
		{

		}

		public TouchEventArgs(TouchAction actionType, Point location)
		{
			ActionType = actionType;
			Location = location;
		}

		public TouchAction ActionType { get; private set; }

		public Point Location { get; private set; }
	}
}