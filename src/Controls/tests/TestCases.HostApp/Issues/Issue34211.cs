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

		_graphicsView.SizeChanged += (_, _) => _graphicsView.Invalidate();

		_drawable.OnDrawn = rect =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				double viewW = _graphicsView.Width;
				double viewH = _graphicsView.Height;
				bool widthMatch = Math.Abs(viewW - rect.Width) <= 1.0;
				bool heightMatch = Math.Abs(viewH - rect.Height) <= 1.0;
				_statusLabel.Text = widthMatch && heightMatch
					? "PASS: sizes match"
					: $"FAIL: GraphicsView={viewW:F1}x{viewH:F1} Drawable={rect.Width:F1}x{rect.Height:F1}";
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
