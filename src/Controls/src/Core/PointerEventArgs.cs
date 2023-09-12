using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	///	Arguments for PointerGestureRecognizer events.
	/// </summary>
	public class PointerEventArgs : EventArgs
	{

		Func<IElement?, Point?>? _getPosition;

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="PointerEventArgs"/>.
		/// </summary>
		public PlatformPointerEventArgs? PlatformArgs { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PointerEventArgs"/> class.
		/// </summary>
		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? args = null)
		{
			_getPosition = getPosition;
			PlatformArgs = args;
		}

		/// <summary>
		/// When overridden in a derived class, gets the position of the pointer.
		/// </summary>
		/// <remarks>
		/// Gets the position of the pointer relative to the element by default.
		/// </remarks>
		/// <param name="relativeTo">Where the pointer will be measured from.</param>
		/// <returns>The position relative to the <see cref="Element"/>.</returns>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}