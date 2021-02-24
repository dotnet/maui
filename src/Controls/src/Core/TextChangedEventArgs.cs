using System;

namespace Microsoft.Maui.Controls
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