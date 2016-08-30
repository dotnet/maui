using System.Windows.Input;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class TabbedPageWindows : TabbedPage
	{
		public TabbedPageWindows(ICommand restore)
		{
			Children.Add(CreateFirstPage(restore));
			Children.Add(CreateSecondPage());
			WindowsPlatformSpecificsGalleryHelpers.AddToolBarItems(this);
		}

		ContentPage CreateFirstPage(ICommand restore)
		{
			var page = new ContentPage { Title = "Content Page Title" };

			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill
			};
			content.Children.Add(new Label
			{
				Text = "Tabbed Page Windows Features",
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			});

			content.Children.Add(WindowsPlatformSpecificsGalleryHelpers.CreateToolbarPlacementChanger(this));
			content.Children.Add(WindowsPlatformSpecificsGalleryHelpers.CreateAddRemoveToolBarItemButtons(this));

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);
			content.Children.Add(restoreButton);

			page.Content = content;

			return page;
		}

		static Page CreateSecondPage()
		{
			var cp = new ContentPage { Title = "Second Content Page" };

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

			cp.Content = content;

			return cp;
		}
	}
}