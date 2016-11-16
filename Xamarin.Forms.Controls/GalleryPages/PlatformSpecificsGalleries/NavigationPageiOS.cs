using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class NavigationPageiOS : NavigationPage
	{
		public NavigationPageiOS(Page root, ICommand restore) : base(root)
		{
			BackgroundColor = Color.Pink;
			On<iOS>().EnableTranslucentNavigationBar();
			CurrentPage.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Fade);
		}

		public static NavigationPageiOS Create(ICommand restore)
		{
			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);

			var translucentToggleButton = new Button { Text = "Toggle Translucent NavBar" };
			var togglePrefersStatusBarHiddenButton = new Button
			{
				Text = "Toggle PrefersStatusBarHidden"
			};
			var togglePreferredStatusBarUpdateAnimationButton = new Button
			{
				Text = "Toggle PreferredStatusBarUpdateAnimation"
			};
			var content = new ContentPage
			{
				Title = "Navigation Page Features",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children = { translucentToggleButton, restoreButton, togglePrefersStatusBarHiddenButton, togglePreferredStatusBarUpdateAnimationButton}
				}
			};

			var navPage = new NavigationPageiOS(content, restore);

			translucentToggleButton.Clicked += (sender, args) => navPage.On<iOS>().SetIsNavigationBarTranslucent(!navPage.On<iOS>().IsNavigationBarTranslucent());

			togglePrefersStatusBarHiddenButton.Command = new Command(() =>
			{
				var mode = navPage.CurrentPage.On<iOS>().PrefersStatusBarHidden();
				if (mode == StatusBarHiddenMode.Default)
					navPage.CurrentPage.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.True);
				else if (mode == StatusBarHiddenMode.True)
					navPage.CurrentPage.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.False);
				else
					navPage.CurrentPage.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.Default);
			});

			togglePreferredStatusBarUpdateAnimationButton.Command = new Command(() =>
			{
				var animation = navPage.On<iOS>().PreferredStatusBarUpdateAnimation();
				if (animation == UIStatusBarAnimation.Fade)
					navPage.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Slide);
				else if (animation == UIStatusBarAnimation.Slide)
					navPage.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.None);
				else
					navPage.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Fade);
			});

			return navPage;
		}
	}
}