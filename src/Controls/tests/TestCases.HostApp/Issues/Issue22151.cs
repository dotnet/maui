namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22151, "Keyboard does not showing up on SearchHandler.Focus()", PlatformAffected.Android)]
public class Issue22151 : Shell
{
	public Issue22151()
	{
		var focusResultLabel = new Label
		{
			Text = "FocusResult: None",
			AutomationId = "focusResultLabel"
		};

		var isFocusedLabel = new Label
		{
			Text = "IsFocused: False",
			AutomationId = "isFocusedLabel"
		};

		var focusedEventLabel = new Label
		{
			Text = "FocusedEvent: False",
			AutomationId = "focusedEventLabel"
		};

		var searchHandler = new SearchHandler
		{
			AutomationId = "searchHandler",
			Placeholder = "Search...",
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			ShowsResults = false
		};

		searchHandler.Focused += (s, e) =>
		{
			focusedEventLabel.Text = "FocusedEvent: True";
		};

		var focusButton = new Button
		{
			Text = "Programmatic Focus",
			AutomationId = "focusButton"
		};

		focusButton.Clicked += async (s, e) =>
		{
			bool result = searchHandler.Focus();
			focusResultLabel.Text = $"FocusResult: {result}";

			await Task.Delay(200);

			isFocusedLabel.Text = $"IsFocused: {searchHandler.IsFocused}";
		};

		var contentPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Spacing = 15,
				Padding = 20,
				Children =
				{
					focusButton,
					focusResultLabel,
					isFocusedLabel,
					focusedEventLabel,
					new Entry
					{
						AutomationId = "dummyEntry",
						Placeholder = "Tap here to unfocus SearchHandler"
					}
				}
			}
		};

		Shell.SetSearchHandler(contentPage, searchHandler);

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = contentPage
		});
	}
}
