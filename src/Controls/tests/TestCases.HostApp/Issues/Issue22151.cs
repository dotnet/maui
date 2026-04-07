namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22151, "Keyboard does not showing up on SearchHandler.Focus()", PlatformAffected.Android)]
public class Issue22151 : Shell
{
	public Issue22151()
	{
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

		var showKeyboardButton = new Button
		{
			Text = "Show Keyboard",
			AutomationId = "ShowKeyboardButton"
		};

		showKeyboardButton.Clicked += async (s, e) =>
		{
			searchHandler.ShowSoftInputAsync();
			await Task.Delay(200);
			isFocusedLabel.Text = $"IsFocused: {searchHandler.IsFocused}";
		};

		var hideKeyboardButton = new Button
		{
			Text = "Hide Keyboard",
			AutomationId = "HideKeyboardButton"
		};

		hideKeyboardButton.Clicked += (s, e) =>
		{
			searchHandler.HideSoftInputAsync();
		};

		var contentPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Spacing = 15,
				Padding = 20,
				Children =
				{
					showKeyboardButton,
					hideKeyboardButton,
					isFocusedLabel,
					focusedEventLabel
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
