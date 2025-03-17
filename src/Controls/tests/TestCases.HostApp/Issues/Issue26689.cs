namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26689, "Radiobutton not visible in .Net Maui", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue26689 : ContentPage
	{
		public Issue26689()
		{
			var stackLayout = new StackLayout
			{
				Spacing = 10,
				Children =
				{
					new RadioButton
					{
						AutomationId = "radioButton",
						Content = "Margin + Height",
						BackgroundColor = Colors.Red,
						HeightRequest = 50,
						Margin = new Thickness(0, 70, 0, 0),
						TextColor = Colors.Black
					},
					new RadioButton
					{
						Content = "Scale",
						BackgroundColor = Colors.Red,
						Scale = 0.5,
						TextColor = Colors.Black
					},
					new RadioButton
					{
						Content = "Width",
						BackgroundColor = Colors.Red,
						WidthRequest = 100,
						TextColor = Colors.Black
					},
					new RadioButton
					{
						Content = "Height + Vertical and Horizontal Options",
						BackgroundColor = Colors.Red,
						HeightRequest = 80,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.End,
						TextColor = Colors.Black
					},
					new RadioButton
					{
						Content = "TranslationY + TranslationX",
						BackgroundColor = Colors.Red,
						TranslationY = 40,
						TextColor = Colors.Black
					}
				}
			};

			Content = stackLayout;
		}
	}
}


