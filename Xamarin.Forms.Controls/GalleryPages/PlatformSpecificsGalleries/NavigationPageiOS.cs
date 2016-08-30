using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class NavigationPageiOS : NavigationPage
	{
		public static NavigationPageiOS Create(ICommand restore)
		{
			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);

			var translucentToggleButton = new Button { Text = "Toggle Translucent NavBar" };

			var content = new ContentPage
			{
				Title = "Navigation Page Features",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children = { translucentToggleButton, restoreButton }
				}
			};

			var navPage = new NavigationPageiOS(content, restore);

			translucentToggleButton.Clicked += (sender, args) => navPage.On<iOS>().SetIsNavigationBarTranslucent(!navPage.On<iOS>().IsNavigationBarTranslucent());

			return navPage;
		}

		public NavigationPageiOS(Page root, ICommand restore) : base(root)
		{
			BackgroundColor = Color.Pink;
			On<iOS>().EnableTranslucentNavigationBar();
		}
	}
}