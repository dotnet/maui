using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class SearchBariOS : ContentPage
	{
		public SearchBariOS()
		{
			var prominent = new SearchBar { Placeholder = "Prominent" };
			prominent.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Prominent);

			var minimal = new SearchBar { Placeholder = "Minimal" };
			minimal.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Minimal);

			var prominentBackground = new SearchBar { Placeholder = "Prominent on colored background" };
			prominentBackground.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Prominent);

			var minimalBackground = new SearchBar { Placeholder = "Minimal on colored background" };
			minimalBackground.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Minimal);

			Content = new StackLayout
			{
				Children =
				{
					prominent,
					minimal,
					new StackLayout()
					{
						BackgroundColor = Color.Red, 
						Children =
						{
							prominentBackground,
							minimalBackground
						}
					}
				}
			};
		}
	}
}
