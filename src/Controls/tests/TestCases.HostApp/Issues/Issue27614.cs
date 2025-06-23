namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27614, "Label not sized correctly on Android", PlatformAffected.Android)]
	public class Issue27614 : ContentPage
	{
		public Issue27614()
		{

			var singleLineLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Start,
				AutomationId = "Label",
				Margin = new Thickness(5, 5),
				Text = "Hello World",
				BackgroundColor = Colors.Blue,
				TextColor = Colors.Black,
				FontSize = 20
			};

			var multiLineLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Start,
				Margin = new Thickness(5, 5),
				Text = "NET MAUI is a framework used to build native, cross-platform desktop and mobile apps from a single C# codebase for Android, iOS, Mac, and Windows",
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
				singleLineLabel.HorizontalOptions = LayoutOptions.Center;
				multiLineLabel.HorizontalOptions = LayoutOptions.Center;
			};
			var button1 = new Button
			{
				Text = "Change Label HorizontalOptions to End",
				AutomationId = "EndButton",
				Margin = new Thickness(5, 5)
			};
			button1.Clicked += (s, e) =>
			{
				singleLineLabel.HorizontalOptions = LayoutOptions.End;
				multiLineLabel.HorizontalOptions = LayoutOptions.End;
			};
			var layout = new VerticalStackLayout
			{
				Children = { singleLineLabel, multiLineLabel, button, button1 }
			};

			Content = layout;
		}
	}
}