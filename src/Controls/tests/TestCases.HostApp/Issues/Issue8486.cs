namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 8486, "GraphicsView DrawString not rendering in iOS", PlatformAffected.iOS)]
	public class Issue8486 : ContentPage
	{
		public Issue8486()
		{
			var graphicsView = new GraphicsView
			{
				AutomationId = "GraphicsView",
				HeightRequest = 200,
				BackgroundColor = Colors.White,
				Drawable = new GraphicsDrawable()
			};

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Children = { graphicsView }
			};
		}

		public class GraphicsDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				float centerX = dirtyRect.Center.X;

				canvas.FontColor = Colors.Black;
				canvas.FontSize = 18;
				canvas.Font = Microsoft.Maui.Graphics.Font.Default;

				canvas.DrawString("LeftAligned", centerX, 30, HorizontalAlignment.Left);
				canvas.DrawString("CenterAligned", centerX, 90, HorizontalAlignment.Center);
				canvas.DrawString("RightAligned", centerX, 150, HorizontalAlignment.Right);
			}
		}
	}
}