namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31141, "IndicatorView template remains visible with right-to-left flow direction", PlatformAffected.All)]
public class Issue31141 : ContentPage
{
	public Issue31141()
	{
		var indicatorView = new IndicatorView
		{
			AutomationId = "TemplatedIndicatorView",
			ItemsSource = new[] { "FirstIndicator", "SecondIndicator", "ThirdIndicator" },
			FlowDirection = FlowDirection.RightToLeft,
			HorizontalOptions = LayoutOptions.Center,
			IndicatorColor = Colors.LightGray,
			SelectedIndicatorColor = Colors.Yellow,
			Position = 0,
			IndicatorTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Text = "ABC",
					WidthRequest = 40,
					HeightRequest = 40,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				};
				label.SetBinding(AutomationIdProperty, ".");
				return label;
			})
		};

		var toggleDirection = new Button
		{
			AutomationId = "ToggleFlowDirection",
			Text = "Toggle flow direction"
		};
		toggleDirection.Clicked += (_, _) =>
		{
			indicatorView.FlowDirection = indicatorView.FlowDirection == FlowDirection.RightToLeft
				? FlowDirection.LeftToRight
				: FlowDirection.RightToLeft;
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label { Text = "Indicators must stay inside the control when their order changes." },
				indicatorView,
				toggleDirection
			}
		};
	}
}
