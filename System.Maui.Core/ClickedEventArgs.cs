using System;

namespace Xamarin.Forms
{
	public class ClickedEventArgs : EventArgs
	{
		public ClickedEventArgs(ButtonsMask buttons, object commandParameter)
		{
			Buttons = buttons;
			Parameter = commandParameter;
		}

		public ButtonsMask Buttons { get; private set; }

		public object Parameter { get; private set; }
	}
}