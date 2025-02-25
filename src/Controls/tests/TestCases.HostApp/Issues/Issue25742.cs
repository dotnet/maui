namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25742, "iOS BackButton title does not update when set to an empty string or whitespace", PlatformAffected.iOS)]
	public class Issue25742 : Shell
	{
		public Issue25742()
		{
			// Register the routes
			Routing.RegisterRoute("page2", typeof(Issue25742Page2));

			// Create and add Shell contents
			var page1 = new ShellContent
			{
				Title = "Page 1",
				Content = new Issue25742Page1()
			};

			Items.Add(page1);
		}
	}

	public class Issue25742Page1 : ContentPage
	{
		public Issue25742Page1()
		{
			Title = "Page 1";
			var navigateButton = new Button
			{
				Text = "Go to Page 2",
				AutomationId = "GotoPage2"
			};
			navigateButton.Clicked += OnNavigateToPage2Clicked;

			Content = new StackLayout
			{
				Children = { navigateButton }
			};
		}

		private async void OnNavigateToPage2Clicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("page2"); // Use the route name "page2" here
		}
	}

	public class Issue25742Page2 : ContentPage
	{
		public Issue25742Page2()
		{
			Title = "Page 2";
			Content = new StackLayout
			{
				Children = { new Label { Text = "Welcome to Page 2" } }
			};

			// Set back button behavior with empty TextOverride
			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				TextOverride = "" // Removes the back button text
			});
		}
	}
}
