#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for a click event.</summary>
	public class ClickedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.ClickedEventArgs"/> with the specified values.</summary>
		/// <param name="buttons">The button or buttons that were pressed.</param>
		/// <param name="commandParameter">The command parameter.</param>
		public ClickedEventArgs(ButtonsMask buttons, object commandParameter)
		{
			Buttons = buttons;
			Parameter = commandParameter;
		}

		/// <summary>Gets the button or buttons that were pressed.</summary>
		public ButtonsMask Buttons { get; private set; }

		/// <summary>Gets the command parameter.</summary>
		public object Parameter { get; private set; }
	}
}