namespace Microsoft.Maui.Controls.Maps
{
	public class PinClickedEventArgs : System.EventArgs
	{
		public bool HideInfoWindow { get; set; }

		public PinClickedEventArgs()
		{
			HideInfoWindow = false;
		}
	}
}
