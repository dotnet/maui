namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22151, "SearchHandler keyboard does not show on programmatic focus", PlatformAffected.UWP)]
public class Issue22151 : Shell
{
	public Issue22151()
	{
		var flyoutItem = new FlyoutItem
		{
			Route = "home",
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem
		};

		flyoutItem.Items.Add(new Tab
		{
			Title = "Home",
			Route = "hometab",
			Items =
			{
				new ShellContent
				{
					Title = "Home",
					Route = "homepage",
					ContentTemplate = new DataTemplate(typeof(_22151TestPage))
				}
			}
		});

		Items.Add(flyoutItem);
	}

	public class _22151TestPage : ContentPage
	{
		public _22151TestPage()
		{
			Title = "Home";

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

			Shell.SetSearchHandler(this, searchHandler);

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
			};
		}
	}
}
