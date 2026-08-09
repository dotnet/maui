namespace MauiApp._1;

public class MainPage : ContentPage
{
	Button _counterButton = null!;
	int _count;

	public MainPage()
	{
		Title = "Home";
		Build();
	}

	void Build()
	{
		var logo = new Image
		{
			Source = "dotnet_bot.png",
			HeightRequest = 185,
			Aspect = Aspect.AspectFit
		};
		SemanticProperties.SetDescription(logo, "dot net bot in a submarine number ten");

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
			Text = FormatCounterText(_count),
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

		_counterButton.Text = FormatCounterText(_count);

		SemanticScreenReader.Announce(_counterButton.Text);
	}

	static string FormatCounterText(int count) => count switch
	{
		0 => "Click me",
		1 => $"Clicked {count} time",
		_ => $"Clicked {count} times"
	};
}
