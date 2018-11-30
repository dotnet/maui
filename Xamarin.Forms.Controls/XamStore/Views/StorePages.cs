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
			Title = title;

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
					() => AddSearchHandler("Added Search", SearchBoxVisiblity.Expanded)),
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
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Hidden),
				0, 6);

			grid.Children.Add(MakeButton("Collapse Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Collapsable),
				1, 6);

			grid.Children.Add(MakeButton("Show Search",
					() => Shell.GetSearchHandler(this).SearchBoxVisibility = SearchBoxVisiblity.Expanded),
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


			Content = new ScrollView { Content = grid };

			//var listView = new ListView();
			//listView.ItemsSource = Enumerable.Range(0, 1000).ToList();

			//Content = listView;
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

		private class CustomSearchHandler : SearchHandler
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

		protected void AddSearchHandler(string placeholder, SearchBoxVisiblity visibility)
		{
			var searchHandler = new CustomSearchHandler();

			searchHandler.ShowsResults = true;

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
			AddSearchHandler("Search Updates", SearchBoxVisiblity.Collapsable);
		}
	}

	[Preserve (AllMembers = true)]
	public class InstalledPage : BasePage
	{
		public InstalledPage() : base("Installed Items", Color.Default)
		{
			AddSearchHandler("Search Installed", SearchBoxVisiblity.Collapsable);
		}
	}

	[Preserve (AllMembers = true)]
	public class LibraryPage : BasePage
	{
		public LibraryPage() : base("My Library", Color.Default)
		{
			AddSearchHandler("Search Apps", SearchBoxVisiblity.Collapsable);
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
		public HomePage() : base("Store Home", Color.Default)
		{
			AddSearchHandler("Search Apps", SearchBoxVisiblity.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class GamesPage : BasePage
	{
		public GamesPage() : base("Games", Color.Default)
		{
			AddSearchHandler("Search Games", SearchBoxVisiblity.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class MoviesPage : BasePage
	{
		public MoviesPage() : base("Hot Movies", Color.Default)
		{
			AddSearchHandler("Search Movies", SearchBoxVisiblity.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class BooksPage : BasePage
	{
		public BooksPage() : base("Bookstore", Color.Default)
		{
			AddSearchHandler("Search Books", SearchBoxVisiblity.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class MusicPage : BasePage
	{
		public MusicPage() : base("Music", Color.Default)
		{
			AddSearchHandler("Search Music", SearchBoxVisiblity.Expanded);
		}
	}

	[Preserve (AllMembers = true)]
	public class NewsPage : BasePage
	{
		public NewsPage() : base("Newspapers", Color.Default)
		{
			AddSearchHandler("Search Papers", SearchBoxVisiblity.Expanded);
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
