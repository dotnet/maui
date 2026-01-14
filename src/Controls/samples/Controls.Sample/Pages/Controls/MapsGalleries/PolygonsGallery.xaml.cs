using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class PolygonsGallery : ContentPage
	{
		public PolygonsGallery()
		{
			InitializeComponent();
		}

		void OnClearMapElementsClicked(object? sender, EventArgs args)
		{
			MapElementsMap.MapElements.Clear();
		}
	}
}