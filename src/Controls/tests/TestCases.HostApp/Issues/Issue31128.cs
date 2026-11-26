namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31128, "IndicatorView does not display a dynamically assigned IndicatorTemplate", PlatformAffected.All)]
public class Issue31128 : ContentPage
{
	public Issue31128()
	{
		var indicatorView = new IndicatorView
		{
			ItemsSource = new[] { "One", "Two", "Three" },
			HorizontalOptions = LayoutOptions.Center,
			IndicatorColor = Colors.Gray,
			SelectedIndicatorColor = Colors.Red,
			HeightRequest = 80
		};

		var assignTemplateButton = new Button
		{
			Text = "Assign indicator template",
			AutomationId = "AssignIndicatorTemplate"
		};
		assignTemplateButton.Clicked += (sender, args) =>
		{
			indicatorView.IndicatorTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 18,
					WidthRequest = 80,
					HeightRequest = 40,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.AutomationIdProperty, new Binding(".", stringFormat: "CustomIndicator{0}"));
				label.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "Label {0}"));
				return label;
			});
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { indicatorView, assignTemplateButton }
		};
	}
}
