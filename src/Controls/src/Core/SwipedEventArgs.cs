using System;

namespace Microsoft.Maui.Controls
{
	public class SwipedEventArgs : EventArgs
	{
		public SwipedEventArgs(object parameter, SwipeDirection direction)
		{
			Parameter = parameter;
			Direction = direction;
		}

		public object Parameter { get; private set; }

		public SwipeDirection Direction { get; private set; }
	}
}