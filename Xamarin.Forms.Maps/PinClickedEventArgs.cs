using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Maps
{
	public class PinClickedEventArgs : EventArgs
	{
		public bool HideInfoWindow { get; set; }

		public PinClickedEventArgs()
		{
			HideInfoWindow = false;
		}
	}
}