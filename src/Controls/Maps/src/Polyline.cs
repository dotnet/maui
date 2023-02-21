using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Polyline : MapElement
	{
		public IList<Location> Geopath { get; }

		public Polyline()
		{
			var observable = new ObservableCollection<Location>();
			observable.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Geopath));
			Geopath = observable;
		}
	}
}
