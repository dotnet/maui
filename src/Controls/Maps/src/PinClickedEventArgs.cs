namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Event arguments that are associated with a click/tap event that occurs on a pin element of the map control.
	/// </summary>
	public class PinClickedEventArgs : System.EventArgs
	{
		/// <summary>
		/// Gets or sets whether the info window should be hidden after the event occurred.
		/// </summary>
		public bool HideInfoWindow { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PinClickedEventArgs"/> class.
		/// </summary>
		public PinClickedEventArgs()
		{
			HideInfoWindow = false;
		}
	}
}
