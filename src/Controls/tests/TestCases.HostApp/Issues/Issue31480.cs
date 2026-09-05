namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31480, "Label FormattedText does not respect FlowDirection RightToLeft", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31480 : ContentPage
{
	public Issue31480()
	{
		// The reported scenario is inherited RTL: the label uses the default
		// FlowDirection (MatchParent) and inherits RightToLeft from its parent
		// page/app. This exercises the EffectiveFlowDirection / MatchParent path
		// the fix targets, rather than an explicit FlowDirection on the label.
		FlowDirection = FlowDirection.RightToLeft;

		var rtlLabel = new Label
		{
			AutomationId = "RTLFormattedLabel",
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span { Text = "Welcome to " },
					new Span { Text = ".NET MAUI", FontAttributes = FontAttributes.Bold },
					new Span { Text = " – " },
					new Span { Text = "Formatted", TextDecorations = TextDecorations.Underline },
					new Span { Text = " Text", TextColor = Colors.DarkOrange }
				}
			}
		};

		var ltrLabel = new Label
		{
			AutomationId = "LTRFormattedLabel",
			FlowDirection = FlowDirection.LeftToRight,
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span { Text = "Welcome to " },
					new Span { Text = ".NET MAUI", FontAttributes = FontAttributes.Bold },
					new Span { Text = " – " },
					new Span { Text = "Formatted", TextDecorations = TextDecorations.Underline },
					new Span { Text = " Text", TextColor = Colors.DarkOrange }
				}
			}
		};

		// This label starts LeftToRight and is flipped to RightToLeft at runtime by the
		// toggle button below. It exercises the dynamic-switch path where the FlowDirection
		// changes *after* the handler is connected, forcing MapFlowDirection to rebuild the
		// formatted text so the stale paragraph style is refreshed.
		var dynamicLabel = new Label
		{
			AutomationId = "DynamicFormattedLabel",
			FlowDirection = FlowDirection.LeftToRight,
			FormattedText = new FormattedString
			{
				Spans =
				{
					new Span { Text = "Welcome to " },
					new Span { Text = ".NET MAUI", FontAttributes = FontAttributes.Bold },
					new Span { Text = " – " },
					new Span { Text = "Formatted", TextDecorations = TextDecorations.Underline },
					new Span { Text = " Text", TextColor = Colors.DarkOrange }
				}
			}
		};

		var toggleButton = new Button
		{
			AutomationId = "ToggleFlowDirectionButton",
			Text = "Toggle FlowDirection",
			FlowDirection = FlowDirection.LeftToRight
		};
		toggleButton.Clicked += (_, _) =>
			dynamicLabel.FlowDirection = dynamicLabel.FlowDirection == FlowDirection.LeftToRight
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label { Text = "RTL FormattedText (should be right-aligned):", AutomationId = "RTLHeaderLabel", FlowDirection = FlowDirection.LeftToRight },
				rtlLabel,
				new Label { Text = "LTR FormattedText (should be left-aligned):", AutomationId = "LTRHeaderLabel", FlowDirection = FlowDirection.LeftToRight },
				ltrLabel,
				new Label { Text = "Dynamic FormattedText (toggled at runtime):", AutomationId = "DynamicHeaderLabel", FlowDirection = FlowDirection.LeftToRight },
				dynamicLabel,
				toggleButton,
			}
		};
	}
}
