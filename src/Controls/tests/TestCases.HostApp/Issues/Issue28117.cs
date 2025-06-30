namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28117, "Label text is cropped inside the border control with a specific padding value on certain Android devices", PlatformAffected.Android)]
public class Issue28117 : ContentPage
{
	public Issue28117()
	{
		Content = new VerticalStackLayout
		{
			WidthRequest = 350,
			Children =
			{
				new Border()
				{
					Padding =  new Thickness(70.89827027958738, 0, 0, 0),
					Margin = new Thickness(10),
					StrokeThickness = 1,
					Stroke = Colors.Black,
					Content =
					new Label
					{
						AutomationId = "Label",
						FontFamily = "OpenSansRegular",
						FontSize = 16,
						Text = "At any time, but not later than one month before the expiration date"
					}
				},
			}
		};
	}
}