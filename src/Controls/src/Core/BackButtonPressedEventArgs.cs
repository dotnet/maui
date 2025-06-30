#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Internal use only. Contains arguments for the event that is raised when a back button is pressed.</summary>
	public class BackButtonPressedEventArgs : EventArgs
	{
		/// <summary>Internal use only. Gets or sets a value that indicates whether the back button event has already been handled.</summary>
		public bool Handled { get; set; }
	}
}