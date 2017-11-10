using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	public class ImageCellTest {
		public object Text { get; set; }
		public object TextColor { get; set; }
		public object Detail { get; set; }
		public object DetailColor { get; set; }
		public object Image { get; set; }
	}

	public class ImageCellListPage : ContentPage
	{
	
		public ImageCellListPage ()
		{
			Title = "ImageCell List Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var dataTemplate = new DataTemplate (typeof (ImageCell));
			var stringToImageSourceConverter = new GenericValueConverter (
				obj => new FileImageSource {
					File = (string) obj
				}
				);
	
			dataTemplate.SetBinding (TextCell.TextProperty, new Binding ("Text"));
			dataTemplate.SetBinding (TextCell.TextColorProperty, new Binding ("TextColor"));
			dataTemplate.SetBinding (TextCell.DetailProperty, new Binding ("Detail"));
			dataTemplate.SetBinding (TextCell.DetailColorProperty, new Binding ("DetailColor"));
			dataTemplate.SetBinding (ImageCell.ImageSourceProperty, new Binding ("Image", converter: stringToImageSourceConverter));

			Random rand = new Random(250);

			var albums = new [] {
				"crimsonsmall.jpg",
				"oasissmall.jpg",
				"cover1small.jpg"
			};

			var label = new Label { Text = "I have not been selected" };

			var listView = new ListView {
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range (0, 100).Select (i => new ImageCellTest {
					Text = "Text " + i,
					TextColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Detail = "Detail " + i,
					DetailColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Image = albums[rand.Next(0,3)]
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => label.Text = "I was selected";

			Content = new StackLayout { Children = { label, listView } };

		}
	}

	public class UrlImageCellListPage : ContentPage
	{
		public UrlImageCellListPage()
		{
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var dataTemplate = new DataTemplate (typeof (ImageCell));
			var stringToImageSourceConverter = new GenericValueConverter (
				obj => new UriImageSource() {
					Uri = new Uri ((string) obj)
				});

			dataTemplate.SetBinding (TextCell.TextProperty, new Binding ("Text"));
			dataTemplate.SetBinding (TextCell.TextColorProperty, new Binding ("TextColor"));
			dataTemplate.SetBinding (TextCell.DetailProperty, new Binding ("Detail"));
			dataTemplate.SetBinding (TextCell.DetailColorProperty, new Binding ("DetailColor"));
			dataTemplate.SetBinding (ImageCell.ImageSourceProperty,
				new Binding ("Image", converter: stringToImageSourceConverter));

			var albums = new List<string> ();
			for (int i = 0; i < 30; i++) {
				albums.Add (string.Format ("https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png?ticks={0}", i ));
			}


			var random = new Random();
			var label = new Label { Text = "I have not been selected" };

			var listView = new ListView {
				AutomationId = "ImageUrlCellListView",
				ItemsSource = Enumerable.Range (0, 300).Select (i => new ImageCellTest {
					Text = "Text " + i,
					TextColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Detail = "Detail " + i,
					DetailColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Image = albums [random.Next (0, albums.Count - 1)]
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => label.Text = "I was selected";

			Content = new StackLayout { Children = { label, listView } };

		}
	}
}