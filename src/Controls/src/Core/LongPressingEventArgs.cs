using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="LongPressGestureRecognizer.LongPressing"/> event.
	/// Provides real-time state updates during the gesture (primarily on iOS and Mac Catalyst).
	/// </summary>
	public class LongPressingEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LongPressingEventArgs"/> class.
		/// </summary>
		/// <param name="status">The current status of the gesture.</param>
		/// <param name="position">The current position of the touch, if available.</param>
		public LongPressingEventArgs(GestureStatus status, Point? position)
		{
			Status = status;
			Position = position;
		}

		/// <summary>
		/// Gets the current status of the gesture (Started, Running, Completed, or Canceled).
		/// </summary>
		public GestureStatus Status { get; }

		/// <summary>
		/// Gets the current position of the touch, relative to the element.
		/// </summary>
		public Point? Position { get; }
	}
}
