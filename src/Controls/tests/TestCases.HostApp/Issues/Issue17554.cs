using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17554, "[Android] DragGestureRecognizer.DropCompleted event not firing", PlatformAffected.iOS)]
public class Issue17554 : TestContentPage
{
	Label eventText;
	protected override void Init()
	{
		var rootGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = 50 }
			},
			Margin = new Thickness(30)
		};

		var outerBorder = new Border
		{
			Stroke = Colors.Gray,
			StrokeThickness = 1.5,
			StrokeShape = new RoundRectangle() { CornerRadius = 4 },
			VerticalOptions = LayoutOptions.Fill
		};

		var innerStack = new StackLayout();

		var innerBorder = new Border
		{
			Stroke = Colors.Gray,
			StrokeThickness = 1,
			BackgroundColor = Colors.WhiteSmoke
		};

		var label = new Label
		{
			Text = "A",
			AutomationId = "DragTarget",
			FontSize = 32,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var dragGestureRecognizer = new DragGestureRecognizer();
		dragGestureRecognizer.DragStarting += DragStart;
		dragGestureRecognizer.DropCompleted += DropCompleted;

		innerBorder.GestureRecognizers.Add(dragGestureRecognizer);
		innerBorder.Content = label;
		innerStack.Children.Add(innerBorder);
		outerBorder.Content = innerStack;

		rootGrid.Add(outerBorder, 0, 0);

		var eventStack = new HorizontalStackLayout();

		var eventLabel = new Label
		{
			Text = "Event:",
			AutomationId = "DropTarget",
			Padding = new Thickness(4)
		};

		eventText = new Label
		{
			AutomationId = "eventText",
			Text = "Start dragging the 'A' label and drop it here",
			Padding = new Thickness(4),
			HorizontalOptions = LayoutOptions.FillAndExpand
		};

		eventStack.Children.Add(eventLabel);
		eventStack.Children.Add(eventText);
		rootGrid.Add(eventStack, 0, 1);

		Content = rootGrid;
	}

	private void DragStart(object sender, DragStartingEventArgs e)
	{
		eventText.Text = $"DragStarting";
	}

	private void DropCompleted(object sender, DropCompletedEventArgs e)
	{
		eventText.Text = $"DropCompleted";
	}
}