using System.Windows.Input;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class NavigationPageWindows : NavigationPage
	{
		public NavigationPageWindows(ICommand restore)
		{
			PushAsync(CreateRoot(restore));
			WindowsPlatformSpecificsGalleryHelpers.AddToolBarItems(this);

			BarBackgroundColor = Color.FromHex("6495ED");
		}

		ContentPage CreateRoot(ICommand restore)
		{
			var page = new ContentPage { Title = "Content Page Title" };

			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill
			};
			content.Children.Add(new Label
			{
				Text = "Navigation Page Windows Features",
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			});

			content.Children.Add(WindowsPlatformSpecificsGalleryHelpers.CreateToolbarPlacementChanger(this));
			content.Children.Add(WindowsPlatformSpecificsGalleryHelpers.CreateAddRemoveToolBarItemButtons(this));

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);
			content.Children.Add(restoreButton);

			var navButton = new Button { Text = "Push Page (with no title)" };
			navButton.Clicked += (sender, args) => PushAsync(CreatePageWithNoTitle());
			content.Children.Add(navButton);

			page.Content = content;

			return page;
		}

		ContentPage CreatePageWithNoTitle()
		{
			var page = new ContentPage { };

			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill
			};
			content.Children.Add(new Label
			{
				Text = "Page 2",
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			});

			page.Content = content;

			return page;
		}
	}
}