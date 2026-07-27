namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17323, "Arabic text flows RTL on Android in MAUI, but flows LTR on Windows", PlatformAffected.Android)]
public class Issue17323 : TestContentPage
{
	protected override void Init()
	{
		VerticalStackLayout _verticalStackLayout = new VerticalStackLayout();
		GraphicsView _graphicsView = new GraphicsView
		{
			Drawable = new Issue17323_Drawable(),
			HeightRequest = 300,
			WidthRequest = 350,
		};

		Label label = new Label
		{
			AutomationId = "TextLabel",
			Text = "The Arabic text should be drawn from left to right on Android for consistent behavior with Windows, iOS, and macOS."
		};

		_verticalStackLayout.Add(_graphicsView);
		_verticalStackLayout.Add(label);
		Content = _verticalStackLayout;
	}
}


public class Issue17323_Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.StrokeColor = Colors.Red;
		canvas.StrokeSize = 5;
		canvas.FontSize = 18;
		canvas.DrawRectangle(dirtyRect);
		canvas.DrawString("Hello World", dirtyRect.X + 5, dirtyRect.Y + 5, dirtyRect.Width - 10, dirtyRect.Height - 160, HorizontalAlignment.Left, VerticalAlignment.Center);
		canvas.DrawString("مرحبا بالعالم", dirtyRect.X + 5, dirtyRect.Y + 20, dirtyRect.Width - 10, dirtyRect.Height - 160, HorizontalAlignment.Left, VerticalAlignment.Center);
	}
}
