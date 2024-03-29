using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{

	public class ImageLoadingGallery : ContentPage
	{
		public ImageLoadingGallery()
		{
			Padding = new Thickness(20);

			var source = new UriImageSource
			{
				Uri = new Uri("https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/banner.png"),
				CachingEnabled = false
			};

			var image = new Image
			{
				Source = source,
				WidthRequest = 200,
				HeightRequest = 200,
			};

			var indicator = new ActivityIndicator { Color = new Color(.5f), };
			indicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");
			indicator.BindingContext = image;

			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());

			grid.Children.Add(image);
			grid.Children.Add(indicator);

			var cancel = new Button { Text = "Cancel" };
			cancel.Clicked += (s, e) => source.Cancel();
			Grid.SetRow(cancel, 1);
			grid.Children.Add(cancel);

			Content = grid;
		}
	}
}
