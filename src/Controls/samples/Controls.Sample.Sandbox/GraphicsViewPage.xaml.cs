namespace Maui.Controls.Sample;

public partial class GraphicsViewPage : ContentPage
{
	private readonly SimpleDrawable _drawable = new SimpleDrawable();

	public GraphicsViewPage()
	{
		InitializeComponent();

		_drawable.OnDrawn = rect =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				SizeLabel.Text =
					$"GraphicsView : {DrawView.Width:F1} x {DrawView.Height:F1}\n" +
					$"Drawable rect: {rect.Width:F1} x {rect.Height:F1}";

				// Highlight mismatch in red
				bool mismatch =
					Math.Abs(DrawView.Width - rect.Width) > 1 ||
					Math.Abs(DrawView.Height - rect.Height) > 1;
				SizeLabel.TextColor = mismatch ? Colors.Red : Colors.Green;

				Console.WriteLine($"SANDBOX: GraphicsView={DrawView.Width:F1}x{DrawView.Height:F1}  Drawable={rect.Width:F1}x{rect.Height:F1}  Mismatch={mismatch}");
			});
		};

		DrawView.Drawable = _drawable;

		// Re-invalidate whenever the GraphicsView itself resizes
		DrawView.SizeChanged += (_, __) =>
		{
			DrawView.Invalidate();
		};
	}
}

/// <summary>
/// Minimal drawable that reports its dirtyRect on every draw call.
/// After an Android display-size change while the app is running,
/// dirtyRect.Width/Height will differ from GraphicsView.Width/Height
/// even though the parent layout has updated — reproducing issue #34211.
/// </summary>
public class SimpleDrawable : IDrawable
{
	public Action<RectF>? OnDrawn { get; set; }

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		// Draw a filled rectangle so the mismatch is also visible
		canvas.FillColor = Colors.CornflowerBlue;
		canvas.FillRectangle(dirtyRect);

		// Draw a border so the drawable bounds are clearly visible
		canvas.StrokeColor = Colors.DarkBlue;
		canvas.StrokeSize = 3;
		canvas.DrawRectangle(dirtyRect);

		OnDrawn?.Invoke(dirtyRect);
	}
}
