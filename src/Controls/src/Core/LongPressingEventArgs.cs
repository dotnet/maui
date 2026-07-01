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
		readonly Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="LongPressingEventArgs"/> class.
		/// </summary>
		/// <param name="status">The current status of the gesture.</param>
		public LongPressingEventArgs(GestureStatus status)
		{
			Status = status;
		}

		internal LongPressingEventArgs(GestureStatus status, Func<IElement?, Point?>? getPosition) : this(status)
		{
			_getPosition = getPosition;
		}

		/// <summary>
		/// Gets the current status of the gesture (Started, Running, Completed, or Canceled).
		/// </summary>
		public GestureStatus Status { get; }

		/// <summary>
		/// Gets the current position of the touch, relative to the specified element.
		/// </summary>
		/// <param name="relativeTo">The element to get the position relative to. Pass null to get the position relative to the containing window.</param>
		/// <returns>The position of the touch relative to the specified element, or null if the position cannot be determined.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}
