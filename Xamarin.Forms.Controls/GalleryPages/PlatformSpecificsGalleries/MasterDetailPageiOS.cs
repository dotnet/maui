using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class MasterDetailPageiOS : MasterDetailPage
	{
		public MasterDetailPageiOS(ICommand restore)
		{
			MasterBehavior = MasterBehavior.Popover;

			var master = new ContentPage { Title = "Master Detail Page" };
			var masterContent = new StackLayout { Spacing = 10, Margin = new Thickness(0, 10, 5, 0) };
			var detail = new ContentPage
			{
				Title = "This is the detail page's Title",
				Padding = new Thickness(0,20,0,0)
			};
			
			var navItems = new List<NavItem>
			{
				new NavItem("Display Alert", "\uE171", new Command(() => DisplayAlert("Alert", "This is an alert", "OK"))),
				new NavItem("Return To Gallery", "\uE106", restore),
				new NavItem("Save", "\uE105", new Command(() => DisplayAlert("Save", "Fake save dialog", "OK"))),
				new NavItem("Audio", "\uE189", new Command(() => DisplayAlert("Audio", "Never gonna give you up...", "OK"))),
				new NavItem("Set Detail to Navigation Page", "\uE16F", new Command(() => Detail = CreateNavigationPage())),
				new NavItem("Set Detail to Content Page", "\uE160", new Command(() => Detail = detail)),
			};

			var navList = new NavList(navItems);
			
			masterContent.Children.Add(navList);
			master.Content = masterContent;

			var detailContent = new StackLayout {
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "This is a ContentPage with StatusBarHiddenMode.True"
					}
				}
			};

			detail.Content = detailContent;

			Master = master;

			detail.On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.True);

			Detail = detail;
		}

		public class NavItem
		{
			public NavItem(string text, string icon, ICommand command)
			{
				Text = text;
				Icon = icon;
				Command = command;
			}

			public ICommand Command { get; set; }

			public string Icon { get; set; }

			public string Text { get; set; }
		}

		public class NavList : ListView
		{
			public NavList(IEnumerable<NavItem> items)
			{
				ItemsSource = items;
				ItemTapped += (sender, args) => (args.Item as NavItem)?.Command.Execute(null);

				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid();
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 48 });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 200 });

					grid.Margin = new Thickness(0, 10, 0, 10);

					var text = new Label
					{
						VerticalOptions = LayoutOptions.Fill
					};
					text.SetBinding(Label.TextProperty, "Text");

					var glyph = new Label
					{
						FontSize = 24,
						HorizontalTextAlignment = TextAlignment.Center
					};

					glyph.SetBinding(Label.TextProperty, "Icon");

					grid.Children.Add(glyph);
					grid.Children.Add(text);

					Grid.SetColumn(glyph, 0);
					Grid.SetColumn(text, 1);

					grid.WidthRequest = 48;

					var cell = new ViewCell
					{
						View = grid
					};

					return cell;
				});
			}
		}

		static NavigationPage CreateNavigationPage()
		{
			var page = new NavigationPage { Title = "This is the Navigation Page Title" };

			page.PushAsync(CreateNavSubPage());

			return page;
		}

		static ContentPage CreateNavSubPage()
		{
			var page = new ContentPage();
			var content = new StackLayout();
			var navigateButton = new Button() { Text = "Push Another Page" };

			navigateButton.Clicked += (sender, args) => page.Navigation.PushAsync(CreateNavSubPage());

			var togglePrefersStatusBarHiddenButtonForPageButton = new Button
			{
				Text = "Toggle PrefersStatusBarHidden for Page"
			};
			var togglePreferredStatusBarUpdateAnimationButton = new Button
			{
				Text = "Toggle PreferredStatusBarUpdateAnimation"
			};

			togglePrefersStatusBarHiddenButtonForPageButton.Command = new Command(() =>
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
			
			content.Children.Add(navigateButton);
			content.Children.Add(togglePrefersStatusBarHiddenButtonForPageButton);
			content.Children.Add(togglePreferredStatusBarUpdateAnimationButton);

			page.Content = content;

			return page;
		}
	}
}
