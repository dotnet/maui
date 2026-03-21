using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="TapGestureRecognizer.Tapped"/> event.</summary>
	public class TappedEventArgs : EventArgs
	{
		Func<IElement?, Point?>? _getPosition;

		/// <summary>Initializes a new instance of the <see cref="TappedEventArgs"/> class with the specified parameter.</summary>
		/// <param name="parameter">The command parameter associated with the tap gesture.</param>
		public TappedEventArgs(object? parameter)
		{
			Parameter = parameter;
		}

		internal TappedEventArgs(object? parameter, Func<IElement?, Point?>? getPosition, ButtonsMask buttons) : this(parameter)
		{
			_getPosition = getPosition;
			Buttons = buttons;
		}

		/// <summary>Gets the command parameter passed to the <see cref="TapGestureRecognizer"/>.</summary>
		public object? Parameter { get; private set; }

		/// <summary>Gets the button mask indicating which buttons triggered the tap.</summary>
		public ButtonsMask Buttons { get; private set; }

		/// <summary>Gets the position of the tap relative to the specified element.</summary>
		/// <param name="relativeTo">The element to use as the coordinate reference, or <see langword="null"/> for screen coordinates.</param>
		/// <returns>The tap position, or <see langword="null"/> if not available.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}