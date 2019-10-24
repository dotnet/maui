using System;
using System.ComponentModel;

namespace Xamarin.Forms.Controls
{

	public class ImageLoadingGallery : ContentPage
	{
		public ImageLoadingGallery ()
		{
			Padding = new Thickness (20);

			var source = new UriImageSource {
				Uri = new Uri ("http://www.nasa.gov/sites/default/files/styles/1600x1200_autoletterbox/public/images/298773main_EC02-0282-3_full.jpg"),
				CachingEnabled = false
			};

			var image = new Image {
				Source = source,
				WidthRequest = 200,
				HeightRequest = 200,
			};

			var indicator = new ActivityIndicator {Color = new Color (.5),};
			indicator.SetBinding (ActivityIndicator.IsRunningProperty, "IsLoading");
			indicator.BindingContext = image;

			var grid = new Grid();
			grid.RowDefinitions.Add (new RowDefinition());
			grid.RowDefinitions.Add (new RowDefinition());

			grid.Children.Add (image);
			grid.Children.Add (indicator);

			var cancel = new Button { Text = "Cancel" };
			cancel.Clicked += (s, e) => source.Cancel();
			Grid.SetRow (cancel, 1);
			grid.Children.Add (cancel);

			Content = grid;
		}
	}
}
