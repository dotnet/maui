using System;

namespace System.Maui
{
	public class CheckedChangedEventArgs : EventArgs
	{
		public CheckedChangedEventArgs(bool value)
		{
			Value = value;
		}

		public bool Value { get; private set; }
	}
}