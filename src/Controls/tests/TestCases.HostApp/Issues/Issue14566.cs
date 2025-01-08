namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 14566, "SearchBar IsEnabled property not functioning", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue14566 : TestContentPage
	{
		const string SearchBar = "SearchBar";
		const string ResultText = "ResultText";
		const string CheckResultButton = "CheckResultButton";
		const string Success = "Success";
		const string Failure = "Failure";

		protected override void Init()
		{
			var layout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(30, 0),
				Spacing = 25
			};

			var searchBar = new SearchBar
			{
				AutomationId = SearchBar,
				Placeholder = "Search Placeholder",
				IsEnabled = false
			};
			var searchText = new Label();

			searchBar.TextChanged += (sender, e) =>
			{
				// Handle text changed event
				searchText.Text = $"{e.NewTextValue}";
			};

			var result = new Label() { AutomationId = ResultText };
			var checkResultButton = new Button() { AutomationId = CheckResultButton, Text = "Check Result" };
			checkResultButton.Clicked += (sender, e) =>
			{
				result.Text = string.IsNullOrWhiteSpace(searchBar.Text) ? Success : Failure;
			};


			layout.Children.Add(searchBar);
			layout.Children.Add(checkResultButton);
			layout.Children.Add(result);

			Content = layout;
		}
	}
}