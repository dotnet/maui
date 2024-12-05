namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25502, "Gray Line Appears on the Right Side of GraphicsView with Decimal WidthRequest on iOS Platform", PlatformAffected.iOS)]
	public class Issue25502 : ContentPage
	{
		public Issue25502()
		{
			StackLayout stackLayout = new StackLayout
			{
				Children =
				{
					new GraphicsView
					{
						AutomationId = "GraphicsView",
						HeightRequest = 200.25,
						WidthRequest = 248.25,
						BackgroundColor = Colors.White,
						Margin = new Thickness(20),
						Drawable = new GraphicsDrawable()
					}
				}
			};

			Content = stackLayout;
		}


		public class GraphicsDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.StrokeColor = Colors.Red;
				canvas.DrawRectangle(new RectF(dirtyRect.X, dirtyRect.Y + 40, dirtyRect.Width - 40, dirtyRect.Height - 40));
			}
		}
	}
}
