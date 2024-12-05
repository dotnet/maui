namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25709, "MenuBarItem foreground was not updated", PlatformAffected.UWP)]
	public partial class Issue25709 : Shell
	{
		public Issue25709()
		{
			InitializeComponent();
		}
	}

	public partial class _25709MainPage : ContentPage
	{
		public _25709MainPage()
		{
			Title = "MainPage";

			var menuFlyoutItem = new MenuFlyoutItem	{ Text = "Exit"	};
			var menuBarItem = new MenuBarItem{ Text = "MenuItem" };
			menuBarItem.Add(menuFlyoutItem);
			MenuBarItems.Add(menuBarItem);

			var stackLayout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "MainPage",
						AutomationId = "label"
					}
				}
			};

			Content = stackLayout;
		}
	}
}