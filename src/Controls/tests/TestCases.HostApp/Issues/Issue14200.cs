using Microsoft.Maui.Layouts;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14200, "Vertical Stack Layout compressed in IOS and MacCatalyst", PlatformAffected.iOS)]
public class Issue14200 : ContentPage
{
	public Issue14200()
	{
		// --- Vertical Stack Layout ---
		var verticalStack = new VerticalStackLayout
		{
			WidthRequest = 200,
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Label
				{
					AutomationId = "Issue14200Label",
					LineBreakMode = LineBreakMode.WordWrap,
					Text = "This is a test to see if the text is fully visible in iOS and MacCatalyst. The text should wrap and be fully visible.",
				}
			}
		};

		// --- Horizontal Stack Layout ---
		var horizontalStack = new HorizontalStackLayout
		{
			HeightRequest = 300,
			BackgroundColor = Colors.LightBlue,
			HorizontalOptions = LayoutOptions.Start
		};

		var flexLayout = new FlexLayout
		{
			Direction = FlexDirection.Column,
			MinimumWidthRequest = 100,
			BackgroundColor = Colors.Grey,
			Wrap = FlexWrap.Wrap
		};

		for (int i = 0; i < 4; i++)
		{
			flexLayout.Children.Add(new BoxView
			{
				HeightRequest = 80,
				WidthRequest = 100,
				BackgroundColor = Colors.Red,
				Margin = new Thickness(10)
			});
		}

		horizontalStack.Children.Add(flexLayout);

		// --- Combine both in a parent layout ---
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label { Text = "Vertical Stack Layout Test", FontAttributes = FontAttributes.Bold },
				verticalStack,
				new Label { Text = "Horizontal Stack Layout Test", FontAttributes = FontAttributes.Bold },
				horizontalStack
			}
		};
	}
}
