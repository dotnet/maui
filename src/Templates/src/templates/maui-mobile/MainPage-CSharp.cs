namespace MauiApp._1;

public class MainPage : ContentPage
{
	readonly Button _counterButton;
	int _count;

	public MainPage()
	{
		Title = "Home";

		var logo = new Image
		{
			AutomationId = "BotImage",
			Source = "dotnet_bot.png",
			HeightRequest = 185,
			MaximumWidthRequest = 190,
			Margin = new Thickness(0, 20, 0, 0),
			HorizontalOptions = LayoutOptions.Center,
			Aspect = Aspect.AspectFit
		};
		SemanticProperties.SetDescription(logo, "Two dot net bots with a rocket marked eleven.");

		var headline = new Label
		{
			Text = "Hello, World!"
		};
		headline.SetDynamicResource(VisualElement.StyleProperty, "Headline");
		SemanticProperties.SetHeadingLevel(headline, SemanticHeadingLevel.Level1);

		var subtitle = new Label
		{
			Text = "Welcome to \n.NET Multi-platform App UI"
		};
		subtitle.SetDynamicResource(VisualElement.StyleProperty, "SubHeadline");
		SemanticProperties.SetHeadingLevel(subtitle, SemanticHeadingLevel.Level2);
		SemanticProperties.SetDescription(subtitle, "Welcome to dot net Multi platform App U I");

		_counterButton = new Button
		{
			AutomationId = "CounterBtn",
			Text = "Click me",
			HorizontalOptions = LayoutOptions.Fill
		};
		SemanticProperties.SetHint(_counterButton, "Counts the number of times you click");
		_counterButton.Clicked += OnCounterClicked;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					logo,
					headline,
					subtitle,
					_counterButton
				}
			}
		};
	}

	void OnCounterClicked(object? sender, EventArgs e)
	{
		_count++;

		_counterButton.Text = _count == 1
			? $"Clicked {_count} time"
			: $"Clicked {_count} times";

		SemanticScreenReader.Announce(_counterButton.Text);
	}
}
