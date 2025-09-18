namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 8486, "GraphicsView DrawString not rendering in iOS", PlatformAffected.iOS)]
	public class Issue8486 : ContentPage
	{
		public Issue8486()
		{
			var topLabel = new Label { Text = "LabelText", AutomationId = "Label", FontSize = 50 };
			var graphic = new GraphicsView();
			var graphicsDrawable = new GraphicsDrawable();
			graphic.Drawable = graphicsDrawable;

			var topGrid = new Grid();
			topGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			topGrid.Add(topLabel, 0, 0);
			topGrid.Add(graphic, 0, 0);

			var bottomLabel = new Label { Text = "LabelText", FontSize = 30 };
			var bottomGrid = new Grid();
			bottomGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			bottomGrid.Add(bottomLabel, 0, 0);
			Content = new VerticalStackLayout { Children = { topGrid, bottomGrid } };;
		}


		public class GraphicsDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				PathF path = new PathF();
				path.MoveTo(dirtyRect.Left, dirtyRect.Top + dirtyRect.Height / 2);
				path.LineTo(dirtyRect.Left + dirtyRect.Width / 2, dirtyRect.Top + dirtyRect.Height / 2);
				canvas.StrokeColor = Colors.Blue.MultiplyAlpha(0.5f);
				canvas.FillColor = Colors.LightBlue.MultiplyAlpha(0.5f);
				canvas.StrokeSize = 2;
				canvas.DrawPath(path);

				canvas.FontColor = Colors.Black;
				canvas.FontSize = 10;
				canvas.Font = Microsoft.Maui.Graphics.Font.Default;
				canvas.DrawString("GraphicsTextLeft", dirtyRect.Left + dirtyRect.Width / 2, dirtyRect.Top + dirtyRect.Height / 2, HorizontalAlignment.Left);
				canvas.DrawString("GraphicsTextCenter", dirtyRect.Left + dirtyRect.Width / 2, dirtyRect.Bottom - +dirtyRect.Height / 4, HorizontalAlignment.Center);
				canvas.DrawString("GraphicsTextRight", dirtyRect.Left + dirtyRect.Width / 2, dirtyRect.Top + dirtyRect.Height, HorizontalAlignment.Right);
			}
		}
	}
}