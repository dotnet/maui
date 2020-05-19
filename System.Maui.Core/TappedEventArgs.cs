using System;

namespace System.Maui
{
	public class TappedEventArgs : EventArgs
	{
		public TappedEventArgs(object parameter)
		{
			Parameter = parameter;
		}

		public object Parameter { get; private set; }
	}
}