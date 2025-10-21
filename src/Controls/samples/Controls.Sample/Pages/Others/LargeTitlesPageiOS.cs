using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using LargeTitleDisplayMode = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode;

namespace Maui.Controls.Sample.Pages
{
	public class LargeTitlesPageiOS : ContentPage
	{
		public LargeTitlesPageiOS()
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
							var navPage = (NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(!navPage.On<iOS>().PrefersLargeTitles());
						} )
					},

					new Button
					{
						Text = "UseLargeTitles on Navigation with safe Area",
						Command = new Command( () =>{
							var navPage = (NavigationPage)Parent;
							navPage.On<iOS>().SetPrefersLargeTitles(true);
							var page = new ContentPage { Title = "New Title", BackgroundColor = Colors.Red };
							page.On<iOS>().SetUseSafeArea(true);
							var listView = new ListView(ListViewCachingStrategy.RecycleElementAndDataTemplate)
							{
								HasUnevenRows = true,
								VerticalOptions = LayoutOptions.Fill
							};

							listView.ItemTemplate = new DataTemplate(()=>{
								var cell = new ViewCell();
								cell.View = new Label { Text ="Hello", FontSize = 30};
								return cell;
							});
							listView.ItemsSource = Enumerable.Range(1, 40);
							listView.Header = new Label { BackgroundColor = Colors.Pink , Text = "I'm a header, background is red"};
							listView.Footer = new Label { BackgroundColor = Colors.Yellow , Text = "I'm a footer, you should see no white below me"};
							page.Content = listView;
							navPage.PushAsync(page);
						} )
					},
					offscreenPageLimit
				}
			};

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += async (sender, args) => await Navigation.PopAsync();
			content.Children.Add(restoreButton);

			Content = content;
		}
	}
}
