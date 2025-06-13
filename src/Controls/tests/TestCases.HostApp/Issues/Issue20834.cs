namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20834, "[Android] GraphicsView can also be visible outside the canvas", PlatformAffected.Android)]
public class Issue20834 : TestContentPage
{
	readonly Issue20834_Drawable drawable = new();
	float oldX, oldY;
	Grid rootLayout;
	Grid grid;
	GraphicsView graphicsView;

	protected override void Init()
	{
		rootLayout = new Grid()
		{
			RowDefinitions = new RowDefinitionCollection()
		{
			new RowDefinition() { Height = 300 },
			new RowDefinition() { Height = 100 },
			new RowDefinition() { Height = 100 }
			}
		};

		grid = new Grid()
		{
			BackgroundColor = Color.FromArgb("#1a2033"),
			HeightRequest = 300,
			VerticalOptions = LayoutOptions.Center
		};
		graphicsView = new GraphicsView() { AutomationId = "graphicsView" };
		Label label = new Label()
		{
			AutomationId = "DropTarget",
			Text = "Drop the circle here",
			HeightRequest = 100,
			BackgroundColor = Color.FromArgb("#1a2033"),
			TextColor = Colors.White,
		};

		Label invalidDroplabel = new Label()
		{
			AutomationId = "DropTarget_2",
			Text = "Circle should be visible here",
			HeightRequest = 100,
			TextColor = Colors.White,
			VerticalOptions = LayoutOptions.Start,
		};

		graphicsView.Drawable = drawable;
		graphicsView.StartInteraction += GraphicsView_StartInteraction;
		graphicsView.DragInteraction += GraphicsView_DragInteraction;

		grid.Add(graphicsView);

		rootLayout.Add(grid, 0, 0);
		rootLayout.Add(invalidDroplabel, 0, 0);
		rootLayout.Add(new BoxView()
		{
			BackgroundColor = Colors.CornflowerBlue,
			WidthRequest = 100,
			HeightRequest = 100,
		}, 0, 1);
		rootLayout.Add(label, 0, 2);

		Content = rootLayout;

	}

	void GraphicsView_DragInteraction(object sender, TouchEventArgs e)
	{
		var x = e.Touches[0].X;
		var y = e.Touches[0].Y;
		x -= oldX;
		y -= oldY;
		drawable.X += x;
		drawable.Y += y;
		oldX = e.Touches[0].X;
		oldY = e.Touches[0].Y;
		graphicsView.Invalidate();
	}

	void GraphicsView_StartInteraction(object sender, TouchEventArgs e)
	{
		oldX = e.Touches[0].X;
		oldY = e.Touches[0].Y;
	}
}

internal class Issue20834_Drawable : IDrawable
{
	public float X { get; set; } = 20f;
	public float Y { get; set; } = 20f;

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();
		canvas.FillColor = Colors.Blue;
		canvas.FillEllipse(X, Y, 100, 100);
		canvas.ResetState();
	}
}