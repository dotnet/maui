namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20834, "[Android] GraphicsView can also be visible outside the canvas", PlatformAffected.Android)]
public class Issue20834 : TestContentPage
{
	readonly Issue20834_Drawable drawable = new();
	GraphicsView graphicsView;
	Label overlayLabel;

	protected override void Init()
	{
		var rootLayout = new Grid()
		{
			RowDefinitions =
			{
				new RowDefinition() { Height = 300 }, // Canvas row
				new RowDefinition() { Height = 60 }   // Button row
			}
		};

		// First row: Canvas with overlapping label
		var canvasGrid = new Grid()
		{
			BackgroundColor = Color.FromArgb("#1a2033"),
			HeightRequest = 300,
			WidthRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		// Create the GraphicsView (canvas) with specific dimensions
		graphicsView = new GraphicsView()
		{
			HeightRequest = 300,
			WidthRequest = 300,
			BackgroundColor = Colors.LightGray,
			AutomationId = "GraphicsCanvas"
		};

		// Create an overlapping label
		overlayLabel = new Label()
		{
			Text = "Overlay Label",
			TextColor = Colors.Red,
			FontSize = 16,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Start,
			Margin = new Thickness(10, 10, 0, 0),
			AutomationId = "OverlayLabel"
		};

		graphicsView.Drawable = drawable;

		// Add both canvas and label to the same grid cell (overlapping)
		canvasGrid.Add(graphicsView);
		canvasGrid.Add(overlayLabel);

		// Button row: Reset and Move buttons
		var buttonRow = new StackLayout()
		{
			Orientation = StackOrientation.Horizontal,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 10
		};

		var resetButton = new Button()
		{
			Text = "Reset Circles",
			AutomationId = "ResetButton"
		};
		resetButton.Clicked += ResetButton_Clicked;

		var moveButton = new Button()
		{
			Text = "Move to Edge Positions",
			AutomationId = "MoveCirclesButton"
		};
		moveButton.Clicked += MoveButton_Clicked;

		buttonRow.Children.Add(resetButton);
		buttonRow.Children.Add(moveButton);

		// Add all rows to the root layout
		rootLayout.Add(canvasGrid, 0, 0);
		rootLayout.Add(buttonRow, 0, 1);

		Content = rootLayout;
	}

	void ResetButton_Clicked(object sender, EventArgs e)
	{
		// Reset all circles to initial positions
		drawable.ResetPositions();
		graphicsView.Invalidate();
	}

	void MoveButton_Clicked(object sender, EventArgs e)
	{
		// Move circles to edge positions to test clipping
		drawable.MoveToEdgePositions();
		graphicsView.Invalidate();
	}

	class Issue20834_Drawable : IDrawable
	{
		// Circle data structure
		class Issue20834_Circle
		{
			public float X { get; set; }
			public float Y { get; set; }
			public Color Color { get; set; }
			public float InitialX { get; set; }
			public float InitialY { get; set; }

			public Issue20834_Circle(float x, float y, Color color)
			{
				X = x;
				Y = y;
				InitialX = x;
				InitialY = y;
				Color = color;
			}
		}

		private readonly List<Issue20834_Circle> _circles;
		private const float CircleDiameter = 100f;

		public Issue20834_Drawable()
		{
			// Initialize 5 circles with different colors at initial positions
			_circles = new List<Issue20834_Circle>
			{
				new Issue20834_Circle(50f, 50f, Colors.Blue),
				new Issue20834_Circle(50f, 50f, Colors.Red),
				new Issue20834_Circle(50f, 50f, Colors.Green),
				new Issue20834_Circle(50f, 50f, Colors.Orange),
				new Issue20834_Circle(50f, 50f, Colors.Purple)
			};
		}


		public void ResetPositions()
		{
			foreach (var circle in _circles)
			{
				circle.X = circle.InitialX;
				circle.Y = circle.InitialY;
			}
		}

		public void MoveToEdgePositions()
		{

			if (_circles.Count >= 5)
			{

				_circles[0].X = 250f; // Right
				_circles[0].Y = 150f;

				_circles[1].Y = 250f; // Bottom
				_circles[1].X = 150f;

				_circles[2].X = -50f; //left
				_circles[2].Y = 150f;

				_circles[3].Y = -50f; // top
				_circles[3].X = 150f;

				_circles[4].X = -50f;
				_circles[4].Y = -50f;
			}
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.SaveState();

			// Draw a border around the canvas to visualize the bounds
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 2;
			canvas.DrawRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

			// Draw all circles
			foreach (var circle in _circles)
			{
				canvas.FillColor = circle.Color;
				canvas.FillEllipse(circle.X, circle.Y, CircleDiameter, CircleDiameter);
			}

			canvas.RestoreState();
		}
	}
}
