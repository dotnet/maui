using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="LongPressGestureRecognizer.LongPressed"/> event.
	/// </summary>
	public class LongPressedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LongPressedEventArgs"/> class.
		/// </summary>
		/// <param name="parameter">The command parameter.</param>
		/// <param name="position">The position where the long press occurred, if available.</param>
		public LongPressedEventArgs(object? parameter, Point? position)
		{
			Parameter = parameter;
			Position = position;
		}

		/// <summary>
		/// Gets the command parameter associated with the gesture.
		/// </summary>
		public object? Parameter { get; }

		/// <summary>
		/// Gets the position where the long press occurred, relative to the element.
		/// </summary>
		public Point? Position { get; }
	}
}
