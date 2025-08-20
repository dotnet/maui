namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31239, "[iOS, Mac, Windows] GraphicsView does not change the Background/BackgroundColor", PlatformAffected.iOS | PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue31239 : TestContentPage
{
	GraphicsView _backgroundColorGraphicsView;
	GraphicsView _backgroundGraphicsView;

	public Issue31239()
	{

	}

	protected override void Init()
	{
		_backgroundColorGraphicsView = new GraphicsView()
		{
			HeightRequest = 200,
			WidthRequest = 200,
			BackgroundColor = Colors.Red,
			Drawable = new Issue31239_Drawable()
		};

		_backgroundGraphicsView = new GraphicsView()
		{
			HeightRequest = 200,
			WidthRequest = 200,
			Background = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1),
				GradientStops =
				[
					new GradientStop { Color = Colors.Blue, Offset = 0.0f },
					new GradientStop { Color = Colors.Green, Offset = 1.0f }
				]
			},
			Drawable = new Issue31239_Drawable()
		};

		var backgroundColorLabel = new Label
		{
			Text = "BackgroundColor",
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};

		var backgroundBrushLabel = new Label
		{
			Text = "Background",
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};

		Grid.SetRow(backgroundColorLabel, 0);
		Grid.SetRow(_backgroundColorGraphicsView, 1);
		Grid.SetRow(backgroundBrushLabel, 2);
		Grid.SetRow(_backgroundGraphicsView, 3);

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			Children = { backgroundColorLabel, backgroundBrushLabel, _backgroundColorGraphicsView, _backgroundGraphicsView }
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "This test verifies that GraphicsView Background and BackgroundColor properties are properly applied and can be changed dynamically.",
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				},

				grid,

				new Button
				{
					Text = "Change Background Properties",
					AutomationId = "changeBackgroundButton",
					Command = new Command(ChangeBackgroundProperties)
				}
			}
		};
	}

	void ChangeBackgroundProperties()
	{
		// Change BackgroundColor to a different solid color
		_backgroundColorGraphicsView.BackgroundColor = Colors.Purple;

		// Change Background to a different gradient
		_backgroundGraphicsView.Background = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(0, 1),
			GradientStops =
			[
				new GradientStop { Color = Colors.Orange, Offset = 0.0f },
				new GradientStop { Color = Colors.Pink, Offset = 1.0f }
			]
		};
	}
}

class Issue31239_Drawable : IDrawable
{
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
	}
}