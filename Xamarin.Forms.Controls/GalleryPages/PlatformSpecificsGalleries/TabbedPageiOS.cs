using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class TabbedPageiOS : TabbedPage
	{
		public TabbedPageiOS(ICommand restore)
		{
			Children.Add(CreatePage(restore, "Page One"));
			Children.Add(CreatePage(restore, "Page Two"));
		}

		ContentPage CreatePage(ICommand restore, string title)
		{
			var page = new ContentPage {
				Title = title
			};
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Padding = new Thickness(0, 20, 0, 0)
			};
			content.Children.Add(new Label
			{
				Text = "TabbedPage iOS Features",
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			});

			var togglePrefersStatusBarHiddenButton = new Button
			{
				Text = "Toggle PrefersStatusBarHidden for TabbedPage"
			};
			var togglePrefersStatusBarHiddenForPageButton = new Button
			{
				Text = "Toggle PrefersStatusBarHidden for Page"
			};
			var togglePreferredStatusBarUpdateAnimationButton = new Button
			{
				Text = "Toggle PreferredStatusBarUpdateAnimation"
			};

			togglePrefersStatusBarHiddenButton.Command = new Command(() =>
			{
				var mode = On<iOS>().PrefersStatusBarHidden();
				if (mode == StatusBarHiddenMode.Default)
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.True);
				else if (mode == StatusBarHiddenMode.True)
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.False);
				else
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.Default);
			});

			togglePrefersStatusBarHiddenForPageButton.Command = new Command(() =>
			{
				var mode = page.On<iOS>().PrefersStatusBarHidden();
				if (mode == StatusBarHiddenMode.Default)
					page.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.True);
				else if (mode == StatusBarHiddenMode.True)
					page.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.False);
				else
					page.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.Default);
			});

			togglePreferredStatusBarUpdateAnimationButton.Command = new Command(() =>
			{
				var animation = page.On<iOS>().PreferredStatusBarUpdateAnimation();
				if (animation == UIStatusBarAnimation.Fade)
					page.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Slide);
				else if (animation == UIStatusBarAnimation.Slide)
					page.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.None);
				else
					page.On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Fade);
			});

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);
			content.Children.Add(restoreButton);
			content.Children.Add(togglePrefersStatusBarHiddenButton);
			content.Children.Add(togglePrefersStatusBarHiddenForPageButton);
			content.Children.Add(togglePreferredStatusBarUpdateAnimationButton);
			content.Children.Add(new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				Text = "Note: Setting the PrefersStatusBarHidden value on a TabbedPage applies that value to all its subpages."
			});

			page.Content = content;

			return page;
		}
	}
}
