using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Map : IMap, IEnumerable<IMapPin>
	{
		IList<IMapElement> IMap.Elements => MapElements;

		void IMap.Clicked(Location location) => MapClicked?.Invoke(this, new MapClickedEventArgs(location));

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			//The user specified on the ctor a MapSpan we now need the handler to move to that region
			Handler?.Invoke(nameof(IMap.MoveToRegion), _lastMoveToRegion);
		}
	}
}
