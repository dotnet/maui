using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class NavigationPropertiesGallery
		: ContentPage
	{
		public NavigationPropertiesGallery ()
		{
			var noBack = new ContentPage {
				Title = "No back button",
			};
			NavigationPage.SetHasBackButton (noBack, false);
			var toggleBackButton = new Button { Text = "Toggle Back Button" };
			toggleBackButton.Clicked += (sender, e) => {
				var hasBack = NavigationPage.GetHasBackButton (noBack);
				if (hasBack)
					NavigationPage.SetHasBackButton (noBack, false);
				else
					NavigationPage.SetHasBackButton (noBack, true);
			};
			noBack.Content = toggleBackButton;


			var noBar = new ContentPage {
				Title = "No bar",
				Content = new Label {
					Text = "No bar content",
					Style = Device.Styles.TitleStyle
				}
			};

			var backTitle = new ContentPage {
				Title = "Back button title"
			};

			NavigationPage.SetHasNavigationBar (noBar, false);

			Content = new ListView {
				ItemsSource = new[] {
					noBack,
					noBar,
					backTitle
				},

				ItemTemplate = new DataTemplate (typeof(TextCell)) {
					Bindings = {
						{ TextCell.TextProperty, new Binding ("Title") }
					}
				}
			};

			((ListView) Content).ItemTapped += async (sender, args) => {
				await Navigation.PushAsync ((Page) args.Item);
			};

			NavigationPage.SetBackButtonTitle (this, "Back Title");
		}
	}
}
