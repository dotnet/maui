using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.XamStore
{
    public class BasePage : ContentPage
	{
		private Button MakeButton (string title, Action callback)
		{
			var result = new Button();
			result.Text = title;
			result.Clicked += (s, e) => callback();
			return result;
		}

		public BasePage(string title, Color tint)
		{
			ToolbarItems.Add(new ToolbarItem() { Text = "text" });
			ToolbarItems.Add(new ToolbarItem() { IconImageSource = "coffee.png" });

			Title = title;
			Shell.SetForegroundColor(this, tint);
			var grid = new Grid()
			{
				Padding = 20,
				ColumnDefinitions =
				{
					new ColumnDefinition {Width = GridLength.Star},
					new ColumnDefinition {Width = GridLength.Star},
					new ColumnDefinition {Width = GridLength.Star},
				}
			};

			grid.Children.Add(new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "Welcome to the " + GetType().Name
			}, 0, 3, 0, 1);

			grid.Children.Add(MakeButton("Push",
					() => Navigation.PushAsync((Page)Activator.CreateInstance(GetType()))),
				0, 1);

			grid.Children.Add(MakeButton("Pop",
					() => Navigation.PopAsync()),
				1, 1);

			grid.Children.Add(MakeButton("Pop To Root",
					() => Navigation.PopToRootAsync()),
				2, 1);

			grid.Children.Add(MakeButton("Insert",
					() => Navigation.InsertPageBefore((Page)Activator.CreateInstance(GetType()), this)),
				0, 2);

			grid.Children.Add(MakeButton("Remove",
					() => Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2])),
				1, 2);

			grid.Children.Add(MakeButton("Add Search",
					() => AddSearchHandler("Added Search", SearchBoxVisibility.Expanded)),
				2, 2);

			grid.Children.Add(MakeButton("Add Toolbar",
					() => ToolbarItems.Add(new ToolbarItem("Test", "calculator.png", () => { }))),
				0, 3);

			grid.Children.Add(MakeButton("Remove Toolbar",
					() => ToolbarItems.RemoveAt(0)),
				1, 3);

			grid.Children.Add(MakeButton("Remove Search",
					RemoveSearchHandler),
				2, 3);

			grid.Children.Add(MakeButton("Add Tab",
					AddBottomTab),
				0, 4);

			grid.Children.Add(MakeButton("Remove Tab",
					RemoveBottomTab),
				1, 4);

			grid.Children.Add(MakeButton("Hide Tabs",
					() => Shell.SetTabBarIsVisible(this, false)),
				2, 4);

			grid.Children.Add(MakeButton("Show Tabs",
					() => Shell.SetTabBarIsVisible(this, true)),
				0, 5);

			grid.Children.Add(MakeButton("Hide Nav",
					() => Shell.SetNavBarIsVisible(this, false)),
				1, 5);

			grid.Children.Add(MakeButton("Show Nav",
					() => Shell.SetNavBarIsVisible(this, true)),
				2, 5);

			grid.Children.Add(MakeButton("Hide Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisibility.Hidden),
				0, 6);

			grid.Children.Add(MakeButton("Collapse Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisibility.Collapsible),
				1, 6);

			grid.Children.Add(MakeButton("Show Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisibility.Expanded),
				2, 6);

			grid.Children.Add(MakeButton("Set Back",
					() => Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
					{
						IconOverride = "calculator.png"
					})),
				0, 7);

			grid.Children.Add(MakeButton("Clear Back",
					() => Shell.SetBackButtonBehavior(this, null)),
				1, 7);

			grid.Children.Add(MakeButton("Disable Tab",
					() => ((Forms.ShellSection)Parent.Parent).IsEnabled = false),
				2, 7);

			grid.Children.Add(MakeButton("Enable Tab",
					() => ((Forms.ShellSection)Parent.Parent).IsEnabled = true),
				0, 8);

			grid.Children.Add(MakeButton("Enable Search",
					() => Shell.GetSearchHandler(this).IsSearchEnabled = true),
				1, 8);

			grid.Children.Add(MakeButton("Disable Search",
					() => Shell.GetSearchHandler(this).IsSearchEnabled = false),
				2, 8);

			grid.Children.Add(MakeButton("Set Title",
					() => Title = "New Title"),
				0, 9);

			grid.Children.Add(MakeButton("Set Tab Title",
					() => ((Forms.ShellSection)Parent.Parent).Title = "New Title"),
				1, 9);

			grid.Children.Add(MakeButton("Set GroupTitle",
					() => ((ShellItem)Parent.Parent.Parent).Title = "New Title"),
				2, 9);

			grid.Children.Add(MakeButton("New Tab Icon",
					() => ((Forms.ShellSection)Parent.Parent).Icon = "calculator.png"),
				0, 10);

			grid.Children.Add(MakeButton("Flyout Disabled",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled)),
				1, 10);

			grid.Children.Add(MakeButton("Flyout Collapse",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout)),
				2, 10);

			grid.Children.Add(MakeButton("Flyout Locked",
					() => Shell.SetFlyoutBehavior(this, FlyoutBehavior.Locked)),
				0, 11);

			grid.Children.Add(MakeButton("Add TitleView",
					() => Shell.SetTitleView(this, new Label {
						BackgroundColor = Color.Purple,
						Margin = new Thickness(5, 10),
						Text = "TITLE VIEW"
					})),
				1, 11);

			grid.Children.Add(MakeButton("Null TitleView",
					() => Shell.SetTitleView(this, null)),
				2, 11);

			grid.Children.Add(MakeButton("FH Fixed",
					() => ((Shell)Parent.Parent.Parent.Parent).FlyoutHeaderBehavior = FlyoutHeaderBehavior.Fixed),
				0, 12);

			grid.Children.Add(MakeButton("FH Scroll",
					() => ((Shell)Parent.Parent.Parent.Parent).FlyoutHeaderBehavior = FlyoutHeaderBehavior.Scroll),
				1, 12);

			grid.Children.Add(MakeButton("FH Collapse",
					() => ((Shell)Parent.Parent.Parent.Parent).FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll),
				2, 12);

			grid.Children.Add(MakeButton("Add TopTab",
					AddTopTab),
				0, 13);

			grid.Children.Add(MakeButton("Remove TopTab",
					RemoveTopTab),
				1, 13);

			grid.Children.Add(MakeButton("Flow Direction",
					ChangeFlowDirection),
				2, 13);

			grid.Children.Add(MakeSwitch("Nav Visible", out _navBarVisibleSwitch), 0, 14);
			grid.Children.Add(MakeSwitch("Tab Visible", out _tabBarVisibleSwitch), 1, 14);

			grid.Children.Add(MakeButton("Push Special",
					() => {
					var page = (Page)Activator.CreateInstance(GetType());
						Shell.SetNavBarIsVisible (page, _navBarVisibleSwitch.IsToggled);
						Shell.SetTabBarIsVisible(page, _tabBarVisibleSwitch.IsToggled);
						Navigation.PushAsync(page);
					}),
				2, 14);
			
			grid.Children.Add(MakeButton("Show Alert",
				async () => {
					var result = await DisplayAlert("Title", "Message", "Ok", "Cancel");
					Console.WriteLine($"Alert result: {result}");
				}), 0, 15);

			grid.Children.Add(MakeButton("Navigate to 'demo' route",
				async () => await Shell.Current.GoToAsync("demo", true)),
			1, 15);

			grid.Children.Add(MakeButton("Go Back with Text",
			async () => {
					var page = (Page)Activator.CreateInstance(GetType());
					Shell.SetForegroundColor(page, Color.Pink);
					Shell.SetBackButtonBehavior(page, new BackButtonBehavior()
					{
						//IconOverride = "calculator.png",
						
						TextOverride = "back"
					});
					await Navigation.PushAsync(page);
				}),2, 15);

			grid.Children.Add(new Label {
				Text = "Navigate to",
				VerticalOptions = LayoutOptions.CenterAndExpand
			}, 0, 16);
			var navEntry = new Entry { Text = "demo/demo" };
			grid.Children.Add(navEntry, 1, 16);
			grid.Children.Add(MakeButton("GO!",
				async () => await Shell.Current.GoToAsync(navEntry.Text, true)),
			2, 16);

			var headerWidth = new Slider
			{
				Minimum = 0,
				Maximum = 400,
				Value = (Shell.Current.FlyoutHeader as VisualElement)?.HeightRequest ?? 0
			};
			headerWidth.ValueChanged += (_, e) =>
			{
				if (Shell.Current.FlyoutHeader is VisualElement ve)
					ve.HeightRequest = e.NewValue;
			};
			grid.Children.Add(new Label
			{
				Text = "fly Header",
				VerticalOptions = LayoutOptions.CenterAndExpand
			}, 0, 17);
			grid.Children.Add(headerWidth, 1, 17);

			grid.Children.Add(MakeButton("bg image",
				() => Shell.Current.FlyoutBackgroundImage = ImageSource.FromFile("photo.jpg")),
			0, 18);
			grid.Children.Add(MakeButton("bg color",
				() => Shell.Current.FlyoutBackgroundColor = Color.DarkGreen),
			1, 18);
			grid.Children.Add(MakeButton("bg aFit",
				() => Shell.Current.FlyoutBackgroundImageAspect = Aspect.AspectFit),
			2, 18);
			grid.Children.Add(MakeButton("bg aFill",
				() => Shell.Current.FlyoutBackgroundImageAspect = Aspect.AspectFill),
			0, 19);
			grid.Children.Add(MakeButton("bg Fill",
				() => Shell.Current.FlyoutBackgroundImageAspect = Aspect.Fill),
			1, 19);
			grid.Children.Add(MakeButton("clear bg",
				() => {
					Shell.Current.ClearValue(Shell.FlyoutBackgroundColorProperty);
					Shell.Current.ClearValue(Shell.FlyoutBackgroundImageProperty);
				}),
			2, 19);

			Entry flyheaderMargin = new Entry();
			flyheaderMargin.TextChanged += (_, __) =>
			{
				int topMargin;
				if (Int32.TryParse(flyheaderMargin.Text, out topMargin))
					(Shell.Current.FlyoutHeader as View).Margin = new Thickness(0, topMargin, 0, 0);
				else
					(Shell.Current.FlyoutHeader as View).ClearValue(View.MarginProperty);
			};


			grid.Children.Add(new Label() { Text = "FH Top Margin" }, 0, 20);
			grid.Children.Add(flyheaderMargin, 1, 20);

			Content = new ScrollView { Content = grid };


			grid.Children.Add(MakeButton("FlyoutBackdrop Color",
					() =>
					{
						if (Shell.GetFlyoutBackdropColor(Shell.Current) == Color.Default)
							Shell.SetFlyoutBackdropColor(Shell.Current, Color.Purple);
						else
							Shell.SetFlyoutBackdropColor(Shell.Current, Color.Default);
					}),
				0, 21);

			grid.Children.Add(MakeButton("Hide Nav Shadow",
                    () => Shell.SetNavBarHasShadow(this, false)),
                1, 21);

            grid.Children.Add(MakeButton("Show Nav Shadow",
                    () => Shell.SetNavBarHasShadow(this, true)),
                2, 21);
		}

		Switch _navBarVisibleSwitch;
		Switch _tabBarVisibleSwitch;

		private View MakeSwitch (string label, out Switch control)
		{
			return new StackLayout
			{
				Children =
				{
					new Label {Text = label},
					(control = new Switch {IsToggled = true})
				}
			};
		}

		private void ChangeFlowDirection()
		{
			var ve = (Shell)Parent.Parent.Parent.Parent;

			if (ve.FlowDirection != FlowDirection.RightToLeft)
				ve.FlowDirection = FlowDirection.RightToLeft;
			else
				ve.FlowDirection = FlowDirection.LeftToRight;
		}

		private void RemoveTopTab()
		{
			var shellSection = (ShellSection)Parent.Parent;
			shellSection.Items.Remove(shellSection.Items[shellSection.Items.Count - 1]);
		}

		private void AddTopTab()
		{
			var shellSection = (ShellSection)Parent.Parent;
			shellSection.Items.Add(
				new Forms.ShellContent()
					{
						Title = "New Top Tab",
						Content = new UpdatesPage()
					}
				);
		}

		private void RemoveBottomTab()
		{
			var shellitem = (ShellItem)Parent.Parent.Parent;
			shellitem.Items.Remove(shellitem.Items[shellitem.Items.Count - 1]);
		}

		private void AddBottomTab()
		{
			var shellitem = (ShellItem)Parent.Parent.Parent;
			shellitem.Items.Add(new ShellSection
			{
				Route = "newitem",
				Title = "New Item",
				Icon = "calculator.png",
				Items =
				{
					new Forms.ShellContent()
					{
						Content = new UpdatesPage()
					}
				}
			});
		}

		internal class CustomSearchHandler : SearchHandler
		{
			protected async override void OnQueryChanged(string oldValue, string newValue)
			{
				base.OnQueryChanged(oldValue, newValue);

				if (string.IsNullOrEmpty(newValue))
				{
					ItemsSource = null;
				}
				else
				{
					List<string> results = new List<string>();
					results.Add(newValue + "initial");

					ItemsSource = results;

					await Task.Delay(2000);

					results = new List<string>();

					for (int i = 0; i < 10; i++)
					{
						results.Add(newValue + i);
					}

					ItemsSource = results;
				}
			}
		}

		protected void AddSearchHandler(string placeholder, SearchBoxVisibility visibility)
		{
			var searchHandler = new CustomSearchHandler();

			searchHandler.BackgroundColor = Color.Orange;
			searchHandler.CancelButtonColor = Color.Pink;
			searchHandler.TextColor = Color.White;
			searchHandler.PlaceholderColor = Color.Yellow;
			searchHandler.HorizontalTextAlignment = TextAlignment.Center;
			searchHandler.ShowsResults = true;

			searchHandler.Keyboard = Keyboard.Numeric;

			searchHandler.FontFamily = "ChalkboardSE-Regular";
			searchHandler.FontAttributes = FontAttributes.Bold;

			searchHandler.ClearIconName = "Clear";
			searchHandler.ClearIconHelpText = "Clears the search field text";

			searchHandler.ClearPlaceholderName = "Voice Search";
			searchHandler.ClearPlaceholderHelpText = "Start voice search";

			searchHandler.QueryIconName = "Search";
			searchHandler.QueryIconHelpText = "Press to search app";

			searchHandler.Placeholder = placeholder;
			searchHandler.SearchBoxVisibility = visibility;

			searchHandler.ClearPlaceholderEnabled = true;
			searchHandler.ClearPlaceholderIcon = "mic.png";

			Shell.SetSearchHandler(this, searchHandler);
		}

		protected void RemoveSearchHandler()
		{
			ClearValue(Shell.SearchHandlerProperty);
		}
	}

	[Preserve (AllMembers = true)]
	public class UpdatesPage : BasePage
	{
		public UpdatesPage() : base("Available Updates", Color.Default)
		{
			AddSearchHandler("Search Updates", SearchBoxVisibility.Collapsible);
		}
	}

	[Preserve (AllMembers = true)]
	public class InstalledPage : BasePage
	{
		public InstalledPage() : base("Installed Items", Color.Default)
		{
			AddSearchHandler("Search Installed", SearchBoxVisibility.Collapsible);
		}
	}

	[Preserve (AllMembers = true)]
	public class LibraryPage : BasePage
	{
		public LibraryPage() : base("My Library", Color.Default)
		{
			AddSearchHandler("Search Apps", SearchBoxVisibility.Collapsible);
		}
	}

	[Preserve (AllMembers = true)]
	public class NotificationsPage : BasePage
	{
		public NotificationsPage() : base("Notifications", Color.Default) { }
	}

	[Preserve (AllMembers = true)]
	public class SubscriptionsPage : BasePage
	{
		public SubscriptionsPage() : base("My Subscriptions", Color.Default) { }
	}

	[Preserve (AllMembers = true)]
	public class HomePage : BasePage
	{
		public HomePage() : base("Store Home", Color.Black)
		{
			AddSearchHandler("Search Apps", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class GamesPage : BasePage
	{
		public GamesPage() : base("Games", Color.Black)
		{
			AddSearchHandler("Search Games", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class MoviesPage : BasePage
	{
		public MoviesPage() : base("Hot Movies", Color.Default)
		{
			AddSearchHandler("Search Movies", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class BooksPage : BasePage
	{
		public BooksPage() : base("Bookstore", Color.Default)
		{
			AddSearchHandler("Search Books", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class MusicPage : BasePage
	{
		public MusicPage() : base("Music", Color.Default)
		{
			AddSearchHandler("Search Music", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class NewsPage : BasePage
	{
		public NewsPage() : base("Newspapers", Color.Default)
		{
			AddSearchHandler("Search Papers", SearchBoxVisibility.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class AccountsPage : BasePage
	{
		public AccountsPage() : base("Account Items", Color.Default) { }
	}

	[Preserve (AllMembers = true)]
	public class WishlistPage : BasePage
	{
		public WishlistPage() : base("My Wishlist", Color.Default) { }
	}

	[Preserve (AllMembers = true)]
	public class SettingsPage : BasePage
	{
		public SettingsPage() : base("Settings", Color.Default) { }
	}

}
