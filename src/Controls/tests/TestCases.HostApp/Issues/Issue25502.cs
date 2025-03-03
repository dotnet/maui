namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25502, "Gray Line Appears on the Right Side of GraphicsView with Decimal WidthRequest on iOS Platform", PlatformAffected.iOS)]
	public class Issue25502 : ContentPage
	{
		public Issue25502()
		{
			GraphicsView graphicsView = new GraphicsView
			{
				AutomationId = "GraphicsView",
				HeightRequest = 200.25,
				WidthRequest = 248.25,
				BackgroundColor = Colors.White,
				Margin = new Thickness(20),
			};
			graphicsView.Drawable = new GraphicsDrawable(graphicsView);

			Button changeColorButton = new Button
			{
				Text = "Click to change Color",
				AutomationId = "ChangeColorButton"
			};

			changeColorButton.Clicked += ChangeColorButton_Clicked;

			Content = new StackLayout
			{
				Spacing = 10,
				Children =
				{
					graphicsView,
					changeColorButton
				}
			};

			void ChangeColorButton_Clicked(object sender, EventArgs e)
			{
				graphicsView.BackgroundColor = Colors.Yellow;
			}
		}

		public class GraphicsDrawable : IDrawable
		{
			GraphicsView _graphicsView;
			public GraphicsDrawable(GraphicsView graphicsView)
			{
				_graphicsView = graphicsView;
			}

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.FillColor = _graphicsView.BackgroundColor;
				canvas.FillRectangle(new RectF(dirtyRect.X, dirtyRect.Y + 40, dirtyRect.Width - 40, dirtyRect.Height - 40));
			}
		}
	}
}
