namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35471, "iOS Shell back button history menu does not update after runtime culture change", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35471 : TestShell
{
	ContentPage _rootPage;

	protected override void Init()
	{
		_rootPage = CreateContentPage("HomePage");
		_rootPage.Title = "Home";

		var navigateButton = new Button
		{
			AutomationId = "NavigateToDetail",
			Text = "Go to Detail Page",
			Command = new Command(async () =>
			{
				await Navigation.PushAsync(new Issue35471DetailPage(_rootPage));
			})
		};

		_rootPage.Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label { Text = "Root Page", AutomationId = "RootPageLabel" },
				navigateButton
			}
		};
	}

	class Issue35471DetailPage : ContentPage
	{
		readonly ContentPage _previousPage;

		public Issue35471DetailPage(ContentPage previousPage)
		{
			_previousPage = previousPage;
			Title = "Detail";

			var changeTitleButton = new Button
			{
				AutomationId = "ChangePreviousPageTitle",
				Text = "Change Previous Page Title (simulate culture change)",
				Command = new Command(() =>
				{
					// Simulate runtime culture change by updating the previous page's title
					_previousPage.Title = "Accueil";
				})
			};

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label { Text = "Detail Page", FontSize = 20 },
					changeTitleButton
				}
			};
		}
	}
}
