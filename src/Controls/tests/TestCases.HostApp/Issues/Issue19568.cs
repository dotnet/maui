namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 19568, "GraphicsView does not change the Background/BackgroundColor on Android", PlatformAffected.Android)]
	public class Issue19568 : ContentPage
	{
		public Issue19568()
		{
			var graphicsView = new GraphicsView()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				AutomationId = "graphicsView",
				BackgroundColor = Colors.Red,
				Drawable = new MyDrawable()
			};

			Content =
				new VerticalStackLayout()
				{
					new Grid
					{
						Children = {
							new Label() { Text = "The GraphicsView should have a blue background color" },
							graphicsView
						}
					},
					new Button
					{
						Text = "Change BackgroundColor & opacity",
						AutomationId = "ChangeBackgroundColorButton",
						Command = new Command(() =>
						{
							graphicsView.BackgroundColor = Colors.Blue;
							graphicsView.Opacity = 0.1;
						})
					}
				};
		}

		class MyDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect) { }
		}
	}
}

