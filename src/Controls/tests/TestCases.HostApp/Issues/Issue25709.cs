namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25709, "MenuBarItem foreground was not updated", PlatformAffected.UWP)]
	public class Issue25709 : Shell
	{
		public Issue25709() 
		{
			CurrentItem = new _25709MainPage();
		}
	}

	public partial class _25709MainPage : ContentPage
	{
		public _25709MainPage()
		{
			Title = "MainPage";
			Shell.SetTitleColor(this, Colors.Blue);

			var menuFlyoutItem = new MenuFlyoutItem { Text = "Exit" };
			var menuBarItem = new MenuBarItem { Text = "MenuItem" };
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
