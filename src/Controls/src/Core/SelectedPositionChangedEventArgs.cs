using System;

namespace Microsoft.Maui.Controls
{
	public class SelectedPositionChangedEventArgs : EventArgs
	{
		public SelectedPositionChangedEventArgs(int selectedPosition)
		{
			SelectedPosition = selectedPosition;
		}

		public object SelectedPosition { get; private set; }
	}
}