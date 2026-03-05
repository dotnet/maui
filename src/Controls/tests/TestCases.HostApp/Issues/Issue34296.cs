namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34296, "TapGestureRecognizer on GraphicsView causes a crash on Android devices", PlatformAffected.Android)]
public class Issue34296 : ContentPage
{
	readonly Label _statusLabel;
	int _tapCount;

	public Issue34296()
	{
		_statusLabel = new Label
		{
			AutomationId = "Issue34296StatusLabel",
			Text = "TapCount:0"
		};

		var graphicsView = new GraphicsView
		{
			AutomationId = "Issue34296GraphicsView",
			WidthRequest = 200,
			HeightRequest = 200,
			Drawable = new Issue34296Drawable(),
			BackgroundColor = Colors.Transparent
		};

		var tapGesture = new TapGestureRecognizer
		{
			NumberOfTapsRequired = 1
		};

		tapGesture.Tapped += OnGraphicsViewTapped;
		graphicsView.GestureRecognizers.Add(tapGesture);

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Children =
			{
				_statusLabel,
				graphicsView
			}
		};
	}

	void OnGraphicsViewTapped(object sender, TappedEventArgs e)
	{
#if ANDROID
		if (sender is GraphicsView graphicsView &&
			graphicsView.Handler?.PlatformView is Microsoft.Maui.Platform.PlatformTouchGraphicsView platformView)
		{
			platformView.TouchesBegan(Array.Empty<PointF>());
			platformView.TouchesMoved(new[] { new PointF(10, 10) });
		}
#endif

		_tapCount++;
		_statusLabel.Text = $"TapCount:{_tapCount}";
	}
}

class Issue34296Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
		var radius = size / 2;
		var center = new PointF(radius, radius);

		canvas.FillColor = Colors.Purple;
		canvas.FillCircle(center, radius);
	}
}
