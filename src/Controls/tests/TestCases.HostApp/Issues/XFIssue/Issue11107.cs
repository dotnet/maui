namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11107, "[Bug][iOS] Shell Navigation implicitly adds Tabbar", PlatformAffected.iOS)]

public class Issue11107 : TestShell
{
	bool _tabBarIsVisible = false;
	Label _tabBarLabel;
	public Issue11107() : this(false)
	{
	}

	public Issue11107(bool tabBarIsVisible)
	{
		_tabBarIsVisible = tabBarIsVisible;

		Shell.SetTabBarIsVisible(this, _tabBarIsVisible);
		Shell.SetNavBarHasShadow(this, false);
		_tabBarLabel.Text = $"TabBarIsVisible: {_tabBarIsVisible}";
	}

	protected override void Init()
	{
		_tabBarLabel = new Label();
		ContentPage firstPage = new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					_tabBarLabel,
					new Label()
					{
						Text = "If this page has a tab bar the test has failed",
						AutomationId = "Page1Loaded"
					},
					new Button()
					{
						Text = "Run test again with TabBarIsVisible Toggled",
						Command = new Command(() =>
						{
							Application.Current.MainPage = new Issue11107(!Shell.GetTabBarIsVisible(this));
						}),
						AutomationId = "RunTestTabBarIsVisible"
					},
					new Button()
					{
						Text = "Run with Two Tabs and TabBarIsVisible False",
						Command = new Command(() =>
						{
							var shell = new Issue11107(false);
							shell.AddBottomTab("Second Tab");
							Application.Current.MainPage = shell;
						}),
						AutomationId = "RunTestTwoTabs"
					}
				}
			}
		};

		ContentPage secondPage = new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Hold Please!! Or fail the test if nothing happens."
					}
				}
			}
		};

		var item1 = AddFlyoutItem(firstPage, "Page1");
		item1.Items[0].Title = "Tab 1";
		item1.Items[0].AutomationId = "Tab1AutomationId";
		var item2 = AddFlyoutItem(secondPage, "Page2");

		item1.Route = "FirstPage";
		Routing.RegisterRoute("Issue11107HeaderPage", typeof(Issue11107HeaderPage));

		CurrentItem = item2;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(async () =>
		{
			await Task.Delay(1000);
			await GoToAsync("//FirstPage/Issue11107HeaderPage");
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}


	public class Issue11107HeaderPage : ContentPage
	{
		public Issue11107HeaderPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If this page has a tab bar the test has failed",
						AutomationId = "SecondPageLoaded"
					},
					new Label()
					{
						Text = "Click the Back Button"
					}
				}
			};
		}
	}
}
