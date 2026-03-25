namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20419, "Argument Exception raised when the GetStringSize method of ICanvas called with default font", PlatformAffected.UWP)]
public class Issue20419 : ContentPage
{
	public Issue20419()
	{
		VerticalStackLayout verticalStackLayout = new VerticalStackLayout();

		GraphicsView graphicsView = new GraphicsView()
		{
			HeightRequest = 300,
			WidthRequest = 300,
		};
		graphicsView.Drawable = new MyDrawable();

		Label descriptionLabel = new Label()
		{
			AutomationId = "descriptionLabel",
			Text = "The test passes if the app runs without crashing and fails if the app crashes",
			HorizontalTextAlignment = TextAlignment.Center,
			FontSize = 20
		};

		verticalStackLayout.Children.Add(descriptionLabel);
		verticalStackLayout.Children.Add(graphicsView);

		Content = verticalStackLayout;
	}
}

public class MyDrawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		Microsoft.Maui.Graphics.Font font = new Microsoft.Maui.Graphics.Font();
		var stringSize = canvas.GetStringSize("MyString", font, 32);
		canvas.DrawString($"String Width: {stringSize.Width}, String Height: {stringSize.Height}", dirtyRect.Left + dirtyRect.Width / 2, dirtyRect.Top + dirtyRect.Height / 2, HorizontalAlignment.Center);
	}
}
