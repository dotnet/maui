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
		/// Platform-specific arguments associated with the PointerEventArgs
		/// </summary>
		public PlatformPointerEventArgs? PlatformArgs { get; private set; }

		/// <summary>
		/// Public constructor that creates an instance of <see cref="PointerEventArgs"/>
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
		/// Returns the position of the pointer relative to the element
		/// </summary>
		/// <param name="relativeTo">Element from which the position is relative to</param>
		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}