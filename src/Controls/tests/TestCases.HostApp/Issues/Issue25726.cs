namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25726, "NullReferenceException in WillMoveToParentViewController When Removing Page During Navigation on iOS", PlatformAffected.iOS)]
public partial class Issue25726 : NavigationPage
{
	public Issue25726()
	{
		// Directly push Page 1
		PushAsync(CreatePage("Page 1", "Navigate to Page 2", "NavigateToPage2Button", CreatePage2));
	}

	// Common method to create pages with button and navigation functionality
	private ContentPage CreatePage(string title, string buttonText, string automationId, Func<ContentPage> nextPageCreator)
	{
		return new ContentPage
		{
			Title = title,
			Content = new Button
			{
				Text = buttonText,
				AutomationId = automationId,
				Command = new Command(async () =>
				{
					var nextPage = nextPageCreator();
					await PushAsync(nextPage);
				})
			}
		};
	}

	private ContentPage CreatePage2()
	{
		var page2 = CreatePage("Page 2", "Navigate to Page 3", "NavigateToPage3Button", CreatePage3);


		page2.Content = new Button
		{
			Text = "Navigate to Page 3",
			AutomationId = "NavigateToPage3Button",
			Command = new Command(() =>
			{
				var page3 = CreatePage3();
				PushAsync(page3);

				// Manipulate the stack after navigating to Page 3
				Navigation.RemovePage(Navigation.NavigationStack[^2]);
			})
		};

		return page2;
	}

	private ContentPage CreatePage3()
	{
		return new ContentPage
		{
			Title = "Page 3",
			Content = new Label { Text = "This is Page 3", AutomationId = "Page3Label" }
		};
	}
}
