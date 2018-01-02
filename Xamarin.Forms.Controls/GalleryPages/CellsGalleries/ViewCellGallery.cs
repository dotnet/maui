using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class ViewCellGallery : ContentPage
	{
		public ViewCellGallery ()
		{
			Title = "ViewCell Gallery - Legacy";

			Content = new TableView {
				RowHeight = 150,
				Root = new TableRoot {
					new TableSection ("Testing") {
						new ViewCell {View = new ProductCellView ("0")},
						new ViewCell {View = new ProductCellView ("1")},
						new ViewCell {View = new ProductCellView ("2")},
						new ViewCell {View = new ProductCellView ("3")},
						new ViewCell {View = new ProductCellView ("4")}
					}
				}
			};
		}
	}
	
	public class UrlImageViewCellListPage : ContentPage
	{
		public UrlImageViewCellListPage()
		{
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var stringToImageSourceConverter = new GenericValueConverter (
				obj => new UriImageSource() {
					Uri = new Uri ((string) obj)
				});

			var dataTemplate = new DataTemplate (() => {
				var cell = new ViewCell();

				var image = new Image();
				image.SetBinding(Image.SourceProperty, new Binding("Image", converter: stringToImageSourceConverter));
				image.WidthRequest = 160;
				image.HeightRequest = 160;

				var text = new Label();
				text.SetBinding (Label.TextProperty, new Binding ("Text"));
				text.SetBinding (Label.TextColorProperty, new Binding ("TextColor"));

				cell.View = new StackLayout {
					Orientation = StackOrientation.Horizontal,
					Children = {
						image,
						text
					}
				};

				return cell;
			});

			var albums = new string[25];
			for (int n = 0; n < albums.Length; n++)
			{
				albums[n] =
					string.Format(
						"https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png?ticks={0}", n);
			}

			var label = new Label { Text = "I have not been selected" };

			var listView = new ListView {
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range (0, albums.Length).Select (i => new UrlImageViewCellListPageModel {
					Text = "Text " + i,
					TextColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Image = albums[i]
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => label.Text = "I was selected";

			Content = new StackLayout { Children = { label, listView } };
		}

		[Preserve(AllMembers = true)]
		class UrlImageViewCellListPageModel
		{
			public string Text { get; set; }
			public Color TextColor { get; set; }
			public string Image { get; set; }
		}
	}
}
