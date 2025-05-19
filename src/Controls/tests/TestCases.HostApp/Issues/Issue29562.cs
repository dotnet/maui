namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29562, "Maui.Graphics GetStringSize Inverts Width and Height", PlatformAffected.UWP)]
public class Issue29562 : ContentPage
{
	public Issue29562()
	{
		Content = new GraphicsView
		{
			Drawable = new StringSizeDrawable(),
			HeightRequest = 300,
			WidthRequest = 300,
			AutomationId = "GraphicsView"
		};
	}
}

class StringSizeDrawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		string text = "Hello World";
		var font = new Microsoft.Maui.Graphics.Font("Arial");
		float fontSize = 20;

		var size = canvas.GetStringSize(text, font, fontSize);
	
		// Draw the text
		canvas.Font = font;
		canvas.FontSize = fontSize;
		canvas.DrawString(text, 20, 50, HorizontalAlignment.Left);

		// Draw the bounding box returned by GetStringSize
		canvas.StrokeColor = Colors.Red;
		canvas.DrawRectangle(20, 40, size.Width, size.Height);
	}
}