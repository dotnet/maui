using CommunityToolkit.Maui.Markup;

namespace MauiApp._1;

public class MainPage : ContentPage
{
	readonly Button _counterButton;
	int _count;

	public MainPage()
	{
		Title = "Home";

		var logo = new Image()
			.Source("dotnet_bot.png")
			.Height(185)
			.Aspect(Aspect.AspectFit)
			.CenterHorizontal();
		SemanticProperties.SetDescription(logo, "dot net bot in a submarine number ten");

		var headline = new Label()
			.Text("Hello, World!")
			.Font(size: 32, bold: true)
			.TextCenter();
		SemanticProperties.SetHeadingLevel(headline, SemanticHeadingLevel.Level1);

		var subtitle = new Label()
			.Text("Welcome to \n.NET Multi-platform App UI")
			.FontSize(18)
			.TextCenter();
		SemanticProperties.SetHeadingLevel(subtitle, SemanticHeadingLevel.Level2);
		SemanticProperties.SetDescription(subtitle, "Welcome to dot net Multi platform App U I");

		_counterButton = new Button()
			.Text("Click me")
			.Fill();
		SemanticProperties.SetHint(_counterButton, "Counts the number of times you click");
		_counterButton.Clicked += OnCounterClicked;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 25,
				Children =
				{
					logo,
					headline,
					subtitle,
					_counterButton
				}
			}
			.Padding(30, 0)
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
