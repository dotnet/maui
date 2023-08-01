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
		public PlatformPointerEventArgs? PlatformArgs { get; private set; }

		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition, PlatformPointerEventArgs? args = null)
		{
			_getPosition = getPosition;
			PlatformArgs = args;
		}

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}