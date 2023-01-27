#nullable enable

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

		public PointerEventArgs()
		{
		}

		internal PointerEventArgs(Func<IElement?, Point?>? getPosition)
		{
			_getPosition = getPosition;
		}

		public virtual Point? GetPosition(Element? relativeTo) =>
			_getPosition?.Invoke(relativeTo);
	}
}