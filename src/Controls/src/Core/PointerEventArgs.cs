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
#pragma warning disable RS0016 // Add public types and members to the declared API
		public PointerPlatformEventArgs? Args { get; set; }

		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition, PointerPlatformEventArgs? args = null)
		{
			_getPosition = getPosition;
			Args = args;
		}

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}