using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32042, "Rectangle appears blurred on iOS and macOS when its bounds are changed at runtime within an AbsoluteLayout", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32042 : ContentPage
{
	AbsoluteLayout absoluteLayout;
	Rectangle rectangle;
	Button changeBoundsButton;

	public Issue32042()
	{
		// Create the AbsoluteLayout
		absoluteLayout = new AbsoluteLayout
		{
			BackgroundColor = Colors.LightGray
		};

		// Create the Rectangle
		rectangle = new Rectangle
		{
			BackgroundColor = Colors.Green,
		};

		// Create the description label
		Label label = new Label
		{
			Text = "The green square must remain sharp after its bounds are updated at runtime; otherwise test failed.",
			LineBreakMode = LineBreakMode.WordWrap,
			MaximumWidthRequest = 300,
		};

		// Position the label at the top
		AbsoluteLayout.SetLayoutBounds(label, new Rect(0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);

		// Initially set Rectangle position below the label
		AbsoluteLayout.SetLayoutBounds(rectangle, new Rect(0.5, 0.3, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(rectangle, AbsoluteLayoutFlags.PositionProportional);

		// Create the Button
		changeBoundsButton = new Button
		{
			Text = "Change Bounds",
			AutomationId = "Issue32042Button"
		};

		AbsoluteLayout.SetLayoutBounds(changeBoundsButton, new Rect(0.5, 0.9, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(changeBoundsButton, AbsoluteLayoutFlags.PositionProportional);

		// Attach click event
		changeBoundsButton.Clicked += OnChangeBoundsClicked;

		// Add children to layout - order matters for z-index
		absoluteLayout.Children.Add(label);
		absoluteLayout.Children.Add(rectangle);
		absoluteLayout.Children.Add(changeBoundsButton);

		// Set the Content of the page
		Content = absoluteLayout;
	}

	void OnChangeBoundsClicked(object sender, EventArgs e)
	{
		AbsoluteLayout.SetLayoutBounds(rectangle, new Rect(0.5, 0.5, 100, 100));
	}
}