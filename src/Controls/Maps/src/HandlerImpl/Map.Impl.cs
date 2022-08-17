using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Map : IMap, IEnumerable<IMapPin>
	{
		IList<IMapElement> IMap.Elements => MapElements;

		void IMap.Clicked(Location location) => MapClicked?.Invoke(this, new MapClickedEventArgs(location));

		MapSpan? IMap.VisibleRegion
		{
			get
			{
				return _visibleRegion;
			}
			set
			{
				SetVisibleRegion(value);
			}

		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			//The user specified on the ctor a MapSpan we now need the handler to move to that region
			Handler?.Invoke(nameof(IMap.MoveToRegion), _lastMoveToRegion);
		}

	}
}
