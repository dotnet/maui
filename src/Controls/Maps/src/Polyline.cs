using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls.Compatibility.Maps
{
	public class Polyline : MapElement
	{
		public IList<Position> Geopath { get; }

		public Polyline()
		{
			var observable = new ObservableCollection<Position>();
			observable.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Geopath));
			Geopath = observable;
		}
	}
}
