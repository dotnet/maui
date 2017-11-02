using System;
using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class LargeTitlesPageiOS : ContentPage
	{
		public LargeTitlesPageiOS(ICommand restore)
		{
			Title = "Large Titles";

			var offscreenPageLimit = new Label();
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
				{
					new Button
					{
						Text = "LargeTitleDisplayMode.Never",
						Command = new Command(() => On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never))
					},
					new Button
					{
						Text = "LargeTitleDisplayMode.Always",
						Command = new Command(() => On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always))
					},
					new Button
					{
						Text = "LargeTitleDisplayMode.Automatic -> next page will have same LargeTitleDisplayMode as this one",
						Command = new Command(async () =>{
							var page = new ContentPage { Title = "Page Title" };
							page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Automatic);
							await Navigation.PushAsync(page);
						} )
					},
					new Button
					{
						Text = "Tooggle UseLargeTitles on Navigation",
						Command = new Command( () =>{
							var navPage = (Parent as NavigationPage);
							navPage.On<iOS>().SetPrefersLargeTitles(!navPage.On<iOS>().PrefersLargeTitles());
						} )
					},
					offscreenPageLimit
				}
			};

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked +=  async (sender, args) => await Navigation.PopAsync();
			content.Children.Add(restoreButton);

			Content = content;
		}
	}
}
