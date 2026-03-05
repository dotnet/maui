namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34211, "Android display-size change causes parent and drawable children mismatch in .NET MAUI", PlatformAffected.Android)]
public class Issue34211 : ContentPage
{
	readonly Issue34211_Drawable _drawable = new();
	GraphicsView _graphicsView;
	Label _statusLabel;

	public Issue34211()
	{
		_graphicsView = new GraphicsView
		{
			AutomationId = "Issue34211_GraphicsView",
			BackgroundColor = Color.FromArgb("#F0F0F0"),
			Drawable = _drawable,
		};

		_statusLabel = new Label
		{
			AutomationId = "Issue34211_StatusLabel",
			Text = "Waiting for first draw...",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 0, 0, 10),
		};

		var checkButton = new Button
		{
			AutomationId = "Issue34211_CheckButton",
			Text = "Check Size Match",
		};
		checkButton.Clicked += (_, _) => _graphicsView.Invalidate();

		// Mirror the original sample's layout:
		// Label in Auto row, GraphicsView in * row so it fills remaining space
		// and always has a non-zero size — no HeightRequest needed.
		Content = new Grid
		{
			Padding = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			},
			Children =
			{
				_statusLabel,
				checkButton,
				_graphicsView,
			}
		};

		Grid.SetRow(_statusLabel, 0);
		Grid.SetRow(checkButton, 1);
		Grid.SetRow(_graphicsView, 2);

		// Mirror the original: re-invalidate whenever the view resizes
		_graphicsView.SizeChanged += (_, _) =>
		{
#if ANDROID
			Android.Util.Log.Debug("ISSUE34211", $"[Issue34211.SizeChanged] GraphicsView={_graphicsView.Width:F1}x{_graphicsView.Height:F1} — calling Invalidate()");
#endif
			_graphicsView.Invalidate();
		};

		// Mirror the original: update the label on every draw automatically
		_drawable.OnDrawn = rect =>
		{
#if ANDROID
			Android.Util.Log.Debug("ISSUE34211", $"[Issue34211.OnDrawn] dirtyRect={rect.Width:F1}x{rect.Height:F1}  GraphicsView={_graphicsView.Width:F1}x{_graphicsView.Height:F1}");
#endif
			MainThread.BeginInvokeOnMainThread(() =>
			{
				double viewW = _graphicsView.Width;
				double viewH = _graphicsView.Height;
				bool widthMatch = Math.Abs(viewW - rect.Width) <= 1.0;
				bool heightMatch = Math.Abs(viewH - rect.Height) <= 1.0;
				string status = widthMatch && heightMatch
					? "PASS: sizes match"
					: $"FAIL: GraphicsView={viewW:F1}x{viewH:F1} Drawable={rect.Width:F1}x{rect.Height:F1}";

				_statusLabel.Text = status;
#if ANDROID
				Android.Util.Log.Debug("ISSUE34211", $"[Issue34211.StatusUpdate] {status}");
#endif
			});
		};
	}
}

public class Issue34211_Drawable : IDrawable
{
	public Action<RectF> OnDrawn { get; set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.FillColor = Colors.CornflowerBlue;
		canvas.FillRectangle(dirtyRect);

		canvas.StrokeColor = Colors.DarkBlue;
		canvas.StrokeSize = 3;
		canvas.DrawRectangle(dirtyRect);

		OnDrawn?.Invoke(dirtyRect);
	}
}
