namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22769, "Background set to Transparent doesn't have the same behavior as BackgroundColor Transparent", PlatformAffected.All)]
public class Issue22769 : Shell
{
	public Issue22769()
	{
		Routing.RegisterRoute(nameof(Issue22769_ContentPage), typeof(Issue22769_ContentPage));

		ShellContent shellContent = new ShellContent
		{
			Content = new Issue22769_MainPage()
		};

		Items.Add(new ShellItem
		{
			Items = { shellContent }
		});
	}

}

internal class Issue22769_MainPage : ContentPage
{
	public Issue22769_MainPage()
	{
		VerticalStackLayout layout = new VerticalStackLayout();

		Image image = new Image
		{
			Source = "dotnet_bot.png",
			HeightRequest = 200,
		};

		Button button = new Button
		{
			AutomationId = "Issue22769Button",
			Text = "Go to Modal Page",
		};
		button.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync(nameof(Issue22769_ContentPage));
		};
		layout.Children.Add(image);
		layout.Children.Add(button);
		Content = layout;
	}
}

public class Issue22769_ContentPage : ContentPage
{
	public Issue22769_ContentPage()
	{
		// Page settings
		Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);
		Background = Brush.Transparent;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		var stackLayout = new VerticalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center
		};

		Label label = new Label
		{
			Text = "The underlying image should be visible through the transparent background.",
			FontAttributes = FontAttributes.Bold,
			FontSize = 20,
			AutomationId = "Issue22769Label"
		};

		stackLayout.Children.Add(label);
		grid.Add(stackLayout, 0, 1);
		Content = grid;
	}
}