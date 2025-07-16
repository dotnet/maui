namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20834, "[Android] GraphicsView can also be visible outside the canvas", PlatformAffected.Android)]
public class Issue20834 : TestContentPage
{
	readonly Issue20834_Drawable drawable = new();
	float oldX, oldY;
	GraphicsView graphicsView;

	protected override void Init()
	{
		var rootLayout = new Grid()
		{
			RowDefinitions =
			{
				new RowDefinition() { Height = 300 },
				new RowDefinition() { Height = 100 },
				new RowDefinition() { Height = 100 }
			}
		};

		var grid = new Grid()
		{
			BackgroundColor = Color.FromArgb("#1a2033"),
			HeightRequest = 300,
			VerticalOptions = LayoutOptions.Center
		};

		graphicsView = new GraphicsView();
		var dragTarget = new Button()
		{
			AutomationId = "DragTarget",
			Background = Colors.Transparent,
			BorderWidth = 0,
			CornerRadius = 0,
		};

		var dropTarget1 = new Button()
		{
			AutomationId = "DropTarget1",
			Text = "Drop the circle here",
			HeightRequest = 100,
			TextColor = Colors.White,
			Background = Color.FromArgb("#1a2033"),
			BorderWidth = 0,
			CornerRadius = 0,
			VerticalOptions = LayoutOptions.Start,
			InputTransparent = true,
		};

		var dropTarget2 = new Button()
		{
			AutomationId = "DropTarget2",
			Text = "Circle should be visible here",
			HeightRequest = 100,
			TextColor = Colors.White,
			Background = Colors.Transparent,
			BorderWidth = 0,
			CornerRadius = 0,
			VerticalOptions = LayoutOptions.Start,
			HorizontalOptions = LayoutOptions.Start,
			InputTransparent = true,
		};

		var box = new BoxView()
		{
			BackgroundColor = Colors.CornflowerBlue,
			WidthRequest = 100,
			HeightRequest = 100,
		};

		graphicsView.Drawable = drawable;
		graphicsView.StartInteraction += GraphicsView_StartInteraction;
		graphicsView.DragInteraction += GraphicsView_DragInteraction;

		grid.Add(dragTarget);
		grid.Add(graphicsView);

		rootLayout.Add(grid, 0, 0);
		rootLayout.Add(dropTarget2, 0, 0);
		rootLayout.Add(box, 0, 1);
		rootLayout.Add(dropTarget1, 0, 2);

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

	class Issue20834_Drawable : IDrawable
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
}
