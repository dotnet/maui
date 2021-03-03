using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Maps
{
	public class MapClickedEventArgs
	{
		public Position Position { get; }

		public MapClickedEventArgs(Position position)
		{
			Position = position;
		}
	}
}
