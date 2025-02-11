namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21105, "iOS: Flyout Menu Backdrop Area does not Readjust on Orientation Change", PlatformAffected.iOS)]
	public class Issue21105 : Shell
	{
		public Issue21105()
		{
			this.FlyoutBehavior = FlyoutBehavior.Flyout;
            this.SetValue(Shell.FlyoutBackdropProperty, Color.FromArgb("#7FFFFF00"));

            var homeShellContent = new ShellContent
            {
                Title = "Home",
                ContentTemplate = new DataTemplate(typeof(Issue21105_MainPage)),
                Route = nameof(Issue21105_MainPage)
            };
            var menuItem1 = new MenuItem { Text = "Item 1" };
            var menuItem2 = new MenuItem { Text = "Item 2" };

			this.Items.Add(homeShellContent);
            this.Items.Add(menuItem1);
            this.Items.Add(menuItem2);
		}

		public class Issue21105_MainPage : ContentPage
		{
			public Issue21105_MainPage()
			{
				var OpenFlyoutButton = new Button
				{
					Text = "Open Flyout",
					AutomationId = "OpenFlyoutButton"
				};

				OpenFlyoutButton.Clicked += (sender, args) => Shell.Current.FlyoutIsPresented = true;
				Content = new StackLayout()
				{
					Children = { OpenFlyoutButton }
				};
			}
		}
	}
}