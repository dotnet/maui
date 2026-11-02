namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 37180, "Label background remains visible after being set to null",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue37180 : ContentPage
	{
		public Issue37180()
		{
			var label = new Label
			{
				Text = "Label Background Test",
				Background = new SolidColorBrush(Colors.Red),
				AutomationId = "BackgroundLabel",
				HorizontalOptions = LayoutOptions.Fill,
				Padding = new Thickness(10),
			};

			var setButton = new Button
			{
				Text = "Set Background to null",
				AutomationId = "SetBackgroundButton",
			};
			setButton.Clicked += (s, e) =>
			{
				label.Background = null;
			};

			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Margin = new Thickness(20),
				Children = { label, setButton }
			};
		}
	}
}
