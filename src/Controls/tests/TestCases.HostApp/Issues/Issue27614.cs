namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27614, "Label not sized correctly on Android", PlatformAffected.Android)]
	public class Issue27614 : ContentPage
	{
		public Issue27614()
		{
			var label = new Label
			{
				HorizontalOptions = LayoutOptions.Start,
				AutomationId = "Label",
				Margin = new Thickness(5, 5),
				Text = "Label and parent layout is behaving differently in iOS",
				BackgroundColor = Colors.Orchid,
				TextColor = Colors.Black,
				FontSize = 20
			};

			var button = new Button
			{
				Text = "Change Label HorizontalOptions to Center",
				AutomationId = "CenterButton",
				Margin = new Thickness(5, 5)

			};
			button.Clicked += (s, e) =>
			{
				label.HorizontalOptions = LayoutOptions.Center;
			};
			var button1 = new Button
			{
				Text = "Change Label HorizontalOptions to End",
				AutomationId = "EndButton",
				Margin = new Thickness(5, 5)
			};
			button1.Clicked += (s, e) =>
			{
				label.HorizontalOptions = LayoutOptions.End;
			};
			var layout = new VerticalStackLayout
			{
				Children = { label, button, button1 }
			};

			Content = layout;
		}
	}
}