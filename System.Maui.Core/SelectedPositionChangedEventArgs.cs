using System;

namespace System.Maui
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