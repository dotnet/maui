namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 19568, "GraphicsView does not change the Background/BackgroundColor on Android", PlatformAffected.Android)]
	public class Issue19568 : ContentPage
	{
		public Issue19568()
		{
			Content = new GraphicsView()
			{
				HeightRequest = 300,
				WidthRequest = 300,
				AutomationId = "graphicsView",
				BackgroundColor = Colors.Red,
				Drawable = new MyDrawable()
			};
		}

		class MyDrawable : IDrawable
		{
			public void Draw(ICanvas canvas, RectF dirtyRect) { }
		}
	}
}

