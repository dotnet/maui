namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34930, "SearchHandler.ShowSoftInputAsync() does not focus the SearchHandler", PlatformAffected.UWP)]
public class Issue34930 : Shell
{
	public Issue34930()
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
					ContentTemplate = new DataTemplate(typeof(_34930TestPage))
				}
			}
		});

		Items.Add(flyoutItem);
	}

	public class _34930TestPage : ContentPage
	{
		public _34930TestPage()
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
				Text = "Show Soft Input",
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
				Text = "Hide Soft Input",
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
