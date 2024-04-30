using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Represents a polygon drawn on the map control.
	/// </summary>
	public partial class Polygon : MapElement
	{
		/// <summary>Bindable property for <see cref="FillColor"/>.</summary>
		public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
			nameof(FillColor),
			typeof(Color),
			typeof(Polygon),
			default(Color));

		/// <summary>
		/// Gets or sets the fill color. This is a bindable property.
		/// </summary>
		public Color FillColor
		{
			get => (Color)GetValue(FillColorProperty);
			set => SetValue(FillColorProperty, value);
		}

		/// <summary>
		/// Gets a list of locations on the map which forms the outline of this polygon on the map.
		/// </summary>
		public IList<Location> Geopath { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Polygon"/> class.
		/// </summary>
		public Polygon()
		{
			var observable = new ObservableCollection<Location>();
			observable.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Geopath));
			Geopath = observable;
		}
	}
}
