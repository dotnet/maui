#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the <see cref="SwipeGestureRecognizer.Swiped"/> event.
	/// </summary>
	public class SwipedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SwipedEventArgs"/> class.
		/// </summary>
		/// <param name="parameter">The command parameter associated with the swipe.</param>
		/// <param name="direction">The direction of the swipe gesture.</param>
		public SwipedEventArgs(object parameter, SwipeDirection direction)
		{
			Parameter = parameter;
			Direction = direction;
		}

		/// <summary>
		/// Gets the command parameter associated with the swipe gesture.
		/// </summary>
		public object Parameter { get; private set; }

		/// <summary>
		/// Gets the direction of the swipe gesture.
		/// </summary>
		public SwipeDirection Direction { get; private set; }
	}
}