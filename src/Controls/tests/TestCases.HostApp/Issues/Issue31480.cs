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
			}
		};
	}
}
