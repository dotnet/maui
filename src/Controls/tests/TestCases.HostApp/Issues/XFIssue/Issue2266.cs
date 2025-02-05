namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2266, "Setting a different Detail page from a FlyoutPage after 2nd time on MainPage", PlatformAffected.iOS)]
public class Issue2266 : TestContentPage
{
	protected override void Init()
	{
		InitPageContent();
	}

	void InitPageContent()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		var labelHeader = new Label
		{
			Text = "Select a test",
			FontSize = 30,
			FontAttributes = FontAttributes.Bold,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.CenterAndExpand
		};
#pragma warning restore CS0618 // Type or member is obsolete

		string[] listItems = {
			"FlyoutPage Navigation",
			"FlyoutPage Navigation ->> Page 1",
			"FlyoutPage Navigation ->> Page 2",
			"FlyoutPage Navigation ->> Page 3"
		};

		var listView = new ListView
		{
			ItemsSource = listItems
		};

		Content = new StackLayout
		{
			Padding = new Thickness(0, 20, 0, 0),
			Children = {
				labelHeader,
				listView
			}
		};

		listView.ItemSelected += delegate (object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null)
				return;
			if (e.SelectedItem.Equals(listItems[0]))
			{
				Application.Current.MainPage = FlyoutPageHost;
			}
			else if (e.SelectedItem.Equals(listItems[1]) || e.SelectedItem.Equals(listItems[2]) || e.SelectedItem.Equals(listItems[3]))
			{
				// FlyoutPage Navigation - direct page open
				var item = e.SelectedItem.ToString();
				var index = int.Parse(item.Substring(item.Length - 1)) - 1;
				Application.Current.MainPage = FlyoutPageHost;
				FlyoutPageHost.OpenPage(index);
			}

			listView.SelectedItem = null;
		};
	}

	static FlyoutPageNavigation s_FlyoutPageHost;

	static FlyoutPageNavigation FlyoutPageHost
	{
		get
		{
			if (s_FlyoutPageHost == null)
				s_FlyoutPageHost = new FlyoutPageNavigation();
			return s_FlyoutPageHost;
		}
	}
}


public class FlyoutPageNavigation : FlyoutPage
{
	List<NavigationPage> _pages;

	public FlyoutPageNavigation()
	{
		InitPages();

		// Set FlyoutBehavior to Popover to ensure consistent behavior across desktop and mobile platforms.
		// Windows and Catalyst default (FlyoutLayoutBehavior.Default) uses Split mode, which differs from mobile platforms.
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		var menuList = new ListView
		{
			BackgroundColor = Colors.Transparent,
			ItemsSource = _pages,
			ItemTemplate = new DataTemplate(typeof(TextCell))
		};
		menuList.ItemTemplate.SetBinding(TextCell.TextProperty, "Title");

		Flyout = new ContentPage
		{
			BackgroundColor = Color.FromArgb("363636"),
			Title = "Menu",
			Content = menuList
		};

		Detail = new NavigationPage(new ContentPage
		{
			Padding = new Thickness(20, 20),
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Select a menu item" },
					new Button {Command = new Command(() => this.IsPresented = true), AutomationId = "OpenMaster", Text = "Open Flyout"}
				}
			}
		});

		menuList.ItemSelected += (sender, e) =>
		{
			var page = e.SelectedItem as NavigationPage;
			if (page != null)
			{
				Detail = page;
				IsPresented = false;
			}
		};
	}

	void InitPages()
	{
		_pages = new List<NavigationPage>();

		for (int i = 1; i <= 10; i++)
		{
			var btnSubPage = new Button
			{
				Text = string.Format("Open sub-page"),
			};
			btnSubPage.Clicked += delegate
			{
				OpenSubPage(string.Format("Sub for page: {0}", i));
			};
			var page = new ContentPage
			{
				Padding = new Thickness(20, 20),
				Title = string.Format("Page {0}", i),
				Content = new StackLayout
				{
					Children = {
						new Label { AutomationId = "Page {0}",  Text = string.Format ("Page {0}", i) },
						btnSubPage
					}
				}
			};
			page.ToolbarItems.Add(new ToolbarItem("START", null, delegate
			{
				Application.Current.MainPage = new Issue2266();
			})
			{
				AutomationId = "START"
			});

			_pages.Add(new NavigationPage(page) { Title = page.Title });
		}
	}

	public void OpenPage(int index)
	{
		if (index >= _pages.Count)
		{
			// Index out of range
			return;
		}
		Detail = _pages[index];
	}

	async void OpenSubPage(string text)
	{
		await Detail.Navigation.PushAsync(new ContentPage
		{
			Content = new Label { Text = text }
		});
	}
}
