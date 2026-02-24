#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for text changes.</summary>
	public class TextChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="TextChangedEventArgs"/> with the old and new text values.</summary>
		/// <param name="oldTextValue">The previous text value.</param>
		/// <param name="newTextValue">The new text value.</param>
		public TextChangedEventArgs(string oldTextValue, string newTextValue)
		{
			OldTextValue = oldTextValue;
			NewTextValue = newTextValue;
		}

		/// <summary>Gets the new text value.</summary>
		public string NewTextValue { get; private set; }

		/// <summary>Gets the previous text value.</summary>
		public string OldTextValue { get; private set; }
	}
}