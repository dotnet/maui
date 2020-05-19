using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Maps
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
