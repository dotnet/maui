using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				image.SetBinding (Image.SourceProperty, new Binding ("Image", converter: stringToImageSourceConverter));
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

			var albums = new[] {
				"https://evolve.xamarin.com/images/sessions/joseph-mayo-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/jon-skeet-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/rachel-reese-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/mike-james-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/daniel-cazzulino-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/michael-hutchinson-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/laurent-bugnion-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/craig-dunn-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/charles-petzold-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/jason-smith-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/frank-krueger-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/james-clancey-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/daniel-plaisted-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/jesse-liberty-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/miguel-de-icaza-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/rene-ruppert-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/brent-schooley-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/adrian-stevens-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/rodrigo-kumpera-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/alex-corrado-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/jonathan-pryor-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/michael-stonis-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/jeremie-laval-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/james-montemagno-icon.jpg",
				"https://evolve.xamarin.com/images/sessions/brett-duncavage-icon.jpg"
			};

			var label = new Label { Text = "I have not been selected" };

			var listView = new ListView {
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range (0, albums.Length).Select (i => new {
					Text = "Text " + i,
					TextColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Detail = "Detail " + i,
					DetailColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Image = albums[i]
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => label.Text = "I was selected";

			Content = new StackLayout { Children = { label, listView } };

		}
	}
}
