using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Represents a polyline drawn on the map control.
	/// </summary>
	public partial class Polyline : MapElement
	{
		/// <summary>
		/// Gets a list of locations on the map which forms the polyline on the map.
		/// </summary>
		public IList<Location> Geopath { get; }

		/// <summary>
		/// Occurs when the user clicks/taps on the polyline element
		/// </summary>
		public event EventHandler? PolylineClicked;

		void IMapElement.Clicked()
		{
			PolylineClicked?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Polyline"/> class.
		/// </summary>
		public Polyline()
		{
			var observable = new ObservableCollection<Location>();
			observable.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Geopath));
			Geopath = observable;
		}
	}
}
