namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31182, "GraphicsView draws at half size after canvas.ResetState()", PlatformAffected.Android)]
public class Issue31182 : TestContentPage
{
	protected override void Init()
	{
		var someView = new SomeView
		{
			WidthRequest = 200,
			HeightRequest = 200,
			AutomationId = "GraphicsView31182"
		};

		var label = new Label
		{
			Text = "Waiting",
			AutomationId = "StatusLabel31182"
		};

		someView.Loaded += (s, e) =>
		{
			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					someView.Invalidate();
					label.Text = "Invalidated";
				});
			});
		};

		Content = new VerticalStackLayout
		{
			WidthRequest = 200,
			Background = Colors.Red,
			Children =
			{
				someView,
				label
			}
		};
	}

	class SomeView : GraphicsView, IDrawable
	{
		public SomeView() { Drawable = this; }

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.ResetState();
			canvas.FillColor = Colors.Yellow;
			canvas.FillRectangle(dirtyRect);
		}
	}
}