using System;

namespace Xamarin.Forms
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