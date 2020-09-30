using System;

namespace Xamarin.Forms
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