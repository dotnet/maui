using System;

namespace System.Maui
{
	public class TextChangedEventArgs : EventArgs
	{
		public TextChangedEventArgs(string oldTextValue, string newTextValue)
		{
			OldTextValue = oldTextValue;
			NewTextValue = newTextValue;
		}

		public string NewTextValue { get; private set; }

		public string OldTextValue { get; private set; }
	}
}