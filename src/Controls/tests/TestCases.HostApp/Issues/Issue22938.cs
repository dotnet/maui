namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22938, "Keyboard focus does not shift to a newly opened modal page", PlatformAffected.All)]
public class Issue22938 : ContentPage
{
	public Issue22938()
	{
		var clickCountLabel = new Label
		{
			Text = "0",
			AutomationId = "ClickCountLabel",
			FontSize = 24
		};

		var mainPageButton = new Button
		{
			Text = "Click Me",
			AutomationId = "MainPageButton",
			Command = new Command(() =>
			{
				int count = int.Parse(clickCountLabel.Text);
				clickCountLabel.Text = (count + 1).ToString();
			})
		};

		var openModalButton = new Button
		{
			Text = "Open Modal",
			AutomationId = "OpenModalButton",
			Command = new Command(async () =>
			{
				// Use semi-transparent background to match the reproduction scenario.
			// This causes the underlying page to remain in the visual tree
			// (ModalNavigationManager does not call RemovePage for non-default backgrounds).
			var modalPage = new ContentPage
				{
					BackgroundColor = Color.FromArgb("#40808080"),
					Content = new VerticalStackLayout
					{
						Spacing = 20,
						Padding = new Thickness(30),
						VerticalOptions = LayoutOptions.Center,
						Children =
						{
							new Label
							{
								Text = "Modal Page",
								AutomationId = "ModalPageLabel",
								FontSize = 24,
								HorizontalOptions = LayoutOptions.Center
							},
							new Entry
							{
								Placeholder = "Focus target on modal",
								AutomationId = "ModalEntry"
							},
							new Button
							{
								Text = "Close Modal",
								AutomationId = "CloseModalButton",
								Command = new Command(async () =>
								{
									await Navigation.PopModalAsync();
								})
							}
						}
					}
				};

				await Navigation.PushModalAsync(modalPage);
			})
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Main Page - Issue 22938",
					FontSize = 24
				},
				mainPageButton,
				clickCountLabel,
				openModalButton
			}
		};
	}
}
