namespace Maui.Controls.Sample
{
	internal class CoreRootPage : ContentPage
	{
		public CoreRootPage(Page rootPage)
		{
			Title = "Controls TestCases";

			var corePageView = new CorePageView(rootPage);

			var searchBar = new Entry()
			{
				AutomationId = "SearchBar"
			};

			searchBar.TextChanged += (sender, e) =>
			{
				corePageView.FilterPages(e.NewTextValue);
			};

			var testCasesButton = new Button
			{
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				Command = new Command(async () =>
				{
					if (!string.IsNullOrEmpty(searchBar.Text))
					{
						await corePageView.NavigateToTest(searchBar.Text);
					}
					else
					{
						await Navigation.PushModalAsync(TestCases.GetTestCases());
					}
				})
			};

			var rootLayout = new Grid();
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });


			rootLayout.Add(testCasesButton);
			Grid.SetRow(testCasesButton, 0);

			rootLayout.Add(searchBar);
			Grid.SetRow(searchBar, 1);

			var gcButton = new Button
			{
				Text = "Click to Force GC",
				Command = new Command(() =>
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				})
			};
			rootLayout.Add(gcButton);
			Grid.SetRow(gcButton, 2);

			rootLayout.Add(corePageView);
			Grid.SetRow(corePageView, 3);

			AutomationId = "Gallery";

			Content = rootLayout;
		}
	}
}