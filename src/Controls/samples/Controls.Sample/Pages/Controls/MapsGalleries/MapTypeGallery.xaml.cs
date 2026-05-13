using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapTypeGallery
	{
		public MapTypeGallery()
		{
			InitializeComponent();
		}

		void MapTypePicker_SelectedIndexChanged(object? sender, System.EventArgs e)
		{
			var picker = (Picker)sender!;

			switch (picker.SelectedItem!.ToString())
			{
				default:
				case "Street":
					mapTypeMap.MapType = MapType.Street;
					break;
				case "Satellite":
					mapTypeMap.MapType = MapType.Satellite;
					break;
				case "Hybrid":
					mapTypeMap.MapType = MapType.Hybrid;
					break;
			}
		}

		void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
		{
			double zoomLevel = e.NewValue;
			double latlongDegrees = 360 / (Math.Pow(2, zoomLevel));

			if (mapTypeMap.VisibleRegion != null)
			{
				mapTypeMap.MoveToRegion(new Microsoft.Maui.Maps.MapSpan(mapTypeMap.VisibleRegion.Center, latlongDegrees, latlongDegrees));
			}
		}
	}
}