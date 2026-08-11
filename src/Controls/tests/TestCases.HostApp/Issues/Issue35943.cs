namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35943, "[iOS, MacCatalyst] GetPosition Truncates Fractional Coordinates to Integers on TappedEvent", PlatformAffected.iOS)]
public class Issue35943 : ContentPage
{
	public Issue35943()
	{
		// A half-point left/top margin places this view at a fractional UIKit position (X=0.5, Y=0.5
		// relative to the container). Any tap at an integer screen coordinate will therefore produce
		// fractional coordinates when expressed in this view's local coordinate system.
		var referenceBox = new BoxView
		{
			Color = Colors.CornflowerBlue,
			WidthRequest = 10,
			HeightRequest = 10,
			HorizontalOptions = LayoutOptions.Start,
			Margin = new Thickness(0.5, 0.5, 0, 0),
			AutomationId = "ReferenceBox"
		};

		var instructionLabel = new Label
		{
			Text = "Tap the red box. Coordinates relative to the blue box should be fractional.",
			AutomationId = "InstructionLabel"
		};

		// resultLabel shows human-readable output; statusLabel holds the AutomationId the test waits for.
		// They are separate because AutomationId may only be set once on iOS/MacCatalyst.
		var resultLabel = new Label
		{
			Text = "Tap the red box",
			AutomationId = "ResultLabel"
		};

		var statusLabel = new Label { Text = string.Empty };

		var tapTarget = new BoxView
		{
			Color = Colors.Tomato,
			WidthRequest = 200,
			HeightRequest = 200,
			HorizontalOptions = LayoutOptions.Start,
			AutomationId = "TapTarget"
		};

		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (s, e) =>
		{
			var position = e.GetPosition(relativeTo: referenceBox);

			if (position is null)
			{
				resultLabel.Text = "Failure: position is null";
				if (string.IsNullOrEmpty(statusLabel.AutomationId))
					statusLabel.AutomationId = "Failure";
				statusLabel.Text = "Failure";
				return;
			}

			// Because referenceBox is at a fractional UIKit position (Margin = 0.5),
			// GetPosition(relativeTo: referenceBox) should return coordinates with
			// a fractional component. Before the fix, the (int) cast in CalculatePosition
			// would truncate e.g. 99.5 → 99, losing sub-pixel precision.
			double fracX = Math.Abs(position.Value.X - Math.Truncate(position.Value.X));
			double fracY = Math.Abs(position.Value.Y - Math.Truncate(position.Value.Y));
			bool hasFractionalPrecision = fracX > 0.01 || fracY > 0.01;

			if (hasFractionalPrecision)
			{
				resultLabel.Text = $"Success: X={position.Value.X:F4}, Y={position.Value.Y:F4}";
				if (string.IsNullOrEmpty(statusLabel.AutomationId))
					statusLabel.AutomationId = "Success";
				statusLabel.Text = "Success";
			}
			else
			{
				resultLabel.Text = $"Failure: X={position.Value.X}, Y={position.Value.Y} (expected fractional coordinates)";
				if (string.IsNullOrEmpty(statusLabel.AutomationId))
					statusLabel.AutomationId = "Failure";
				statusLabel.Text = "Failure";
			}
		};

		tapTarget.GestureRecognizers.Add(tapGesture);

		Content = new VerticalStackLayout
		{
			Children =
			{
				instructionLabel,
				referenceBox,
				tapTarget,
				resultLabel,
				statusLabel
			}
		};
	}
}
