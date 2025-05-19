namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29562, "Maui.Graphics GetStringSize Inverts Width and Height", PlatformAffected.UWP)]
public class Issue29562 : ContentPage
{
	public Issue29562()
	{
		var stack = new StackLayout();
		var graphicsView = new GraphicsView
		{
			Drawable = new Issue29562_drawable(),
		};
		var label = new Label
		{
			Text = "Text should displayed in single line.",
			AutomationId = "Label"
		};

		stack.Children.Add(graphicsView);
		stack.Children.Add(label);
		
		Content = stack;
	}
}

class Issue29562_drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		const string text = "Hello World";
		var fontSize = 20;
		var font = new Microsoft.Maui.Graphics.Font("Arial");

		// Measure text
		var textSize = canvas.GetStringSize(text, font, fontSize);

		float rectX = (dirtyRect.Width - textSize.Width) / 2;
		float rectY = (dirtyRect.Height - textSize.Height) / 2;

		var boxRect = new RectF(rectX, rectY, textSize.Width, textSize.Height);
		canvas.DrawString(text, boxRect, HorizontalAlignment.Left, VerticalAlignment.Top);
	}
}