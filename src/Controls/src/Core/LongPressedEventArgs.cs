using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Event arguments for the <see cref="LongPressGestureRecognizer.LongPressed"/> event.
	/// </summary>
	public class LongPressedEventArgs : EventArgs
	{
		readonly Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="LongPressedEventArgs"/> class.
		/// </summary>
		/// <param name="parameter">The command parameter.</param>
		public LongPressedEventArgs(object? parameter)
		{
			Parameter = parameter;
		}

		internal LongPressedEventArgs(object? parameter, Func<IElement?, Point?>? getPosition) : this(parameter)
		{
			_getPosition = getPosition;
		}

		/// <summary>
		/// Gets the command parameter associated with the gesture.
		/// </summary>
		public object? Parameter { get; }

		/// <summary>
		/// Gets the position where the long press occurred, relative to the specified element.
		/// </summary>
		/// <param name="relativeTo">The element to get the position relative to. Pass null to get the position relative to the containing window.</param>
		/// <returns>The position of the long press relative to the specified element, or null if the position cannot be determined.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}
