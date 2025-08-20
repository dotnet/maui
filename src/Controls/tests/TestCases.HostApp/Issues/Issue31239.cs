namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31239, "[iOS, Mac, Windows] GraphicsView does not change the Background/BackgroundColor", PlatformAffected.iOS | PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue31239 : TestContentPage
{
	public Issue31239()
	{

	}

	protected override void Init()
	{
		var graphicsView = new GraphicsView()
		{
			HeightRequest = 300,
			WidthRequest = 300,
			AutomationId = "graphicsView",
			BackgroundColor = Colors.Red,
			Drawable = new Issue31239_Drawable()
		};

		Content = new VerticalStackLayout()
		{
			Children =
			{
				new Label() { Text = "The GraphicsView should have a blue background color" },
				graphicsView,
				new Button
				{
					Text = "Change BackgroundColor & opacity",
							AutomationId = "ChangeBackgroundColorButton",
							Command = new Command(() =>
							{
								graphicsView.BackgroundColor = Colors.Blue;
							})
				}
			}

		};
	}
}

class Issue31239_Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect) { }
}