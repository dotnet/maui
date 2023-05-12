using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Polygon : MapElement
	{
		/// <summary>Bindable property for <see cref="FillColor"/>.</summary>
		public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
			nameof(FillColor),
			typeof(Color),
			typeof(Polygon),
			default(Color));

		public Color FillColor
		{
			get => (Color)GetValue(FillColorProperty);
			set => SetValue(FillColorProperty, value);
		}

		public IList<Location> Geopath { get; }

		public Polygon()
		{
			var observable = new ObservableCollection<Location>();
			observable.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Geopath));
			Geopath = observable;
		}
	}
}
