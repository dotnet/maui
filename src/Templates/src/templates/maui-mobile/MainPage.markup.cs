using CommunityToolkit.Maui.Markup;

namespace MauiApp._1;

public class MainPage : ContentPage
{
	readonly Button CounterBtn;

	int count = 0;

	public MainPage()
	{
		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 25,
				Children =
				{
					new Image()
						.Source("dotnet_bot.png")
						.Size(200)
						.CenterHorizontal()
						.SemanticDescription("Cute dot net bot waving hi to you!"),

					new Label()
						.Text("Hello, World!")
						.Font(size: 32)
						.CenterHorizontal()
						.SemanticHeadingLevel(SemanticHeadingLevel.Level1),

					new Label()
						.Text("Welcome to .NET Multi-platform App UI")
						.Font(size: 18)
						.CenterHorizontal()
						.SemanticDescription("Welcome to dot net Multi platform App U I")
						.SemanticHeadingLevel(SemanticHeadingLevel.Level2),

					new Button()
						.Text("Click me")
						.Assign(out CounterBtn)
						.CenterHorizontal()
						.SemanticHint("Counts the number of times you click"),
				}
			}
			.Paddings(30, 0, 30, 0)
			.CenterVertical()
		};
	}
}

