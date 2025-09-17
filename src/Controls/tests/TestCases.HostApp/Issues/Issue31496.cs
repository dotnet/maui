using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31496, "BoxView in AbsoluteLayout does not return to default AutoSize", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31496 : ContentPage
{
	AbsoluteLayout absoluteLayout;
	BoxView boxView;
	Button changeBoundsButton;
	Button resetBoundsButton;
	readonly Rect defaultBounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

	public Issue31496()
	{
		absoluteLayout = new AbsoluteLayout
		{
			BackgroundColor = Colors.LightGray
		};

		var label = new Label
		{
			Text = "The test passes if a BoxView is not visible in view.",
		};
		AbsoluteLayout.SetLayoutBounds(label, new Rect(0.5, 0.80, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);

		boxView = new BoxView
		{
			Color = Colors.Green
		};
		AbsoluteLayout.SetLayoutBounds(boxView, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(boxView, AbsoluteLayoutFlags.None);

		changeBoundsButton = new Button
		{
			Text = "Change Bounds",
			AutomationId = "Issue31496ChangeBoundsButton"
		};
		AbsoluteLayout.SetLayoutBounds(changeBoundsButton, new Rect(0.5, 0.90, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(changeBoundsButton, AbsoluteLayoutFlags.PositionProportional);
		changeBoundsButton.Clicked += OnChangeBoundsClicked;

		resetBoundsButton = new Button
		{
			Text = "Reset Bounds",
			AutomationId = "Issue31496ResetButton"
		};
		AbsoluteLayout.SetLayoutBounds(resetBoundsButton, new Rect(0.5, 0.99, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
		AbsoluteLayout.SetLayoutFlags(resetBoundsButton, AbsoluteLayoutFlags.PositionProportional);
		resetBoundsButton.Clicked += OnResetBoundsClicked;

		absoluteLayout.Children.Add(boxView);
		absoluteLayout.Children.Add(label);
		absoluteLayout.Children.Add(changeBoundsButton);
		absoluteLayout.Children.Add(resetBoundsButton);

		Content = absoluteLayout;
	}

	void OnChangeBoundsClicked(object sender, EventArgs e)
	{
		AbsoluteLayout.SetLayoutBounds(boxView, new Rect(50, 50, 100, 100));
	}

	void OnResetBoundsClicked(object sender, EventArgs e)
	{
		AbsoluteLayout.SetLayoutBounds(boxView, defaultBounds);
	}
}