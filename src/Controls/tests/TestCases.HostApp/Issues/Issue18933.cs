namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18933, "ContentView Background Color Not Cleared When Set to Null", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue18933 : ContentPage
{
	ContentView contentViewWithBackgroundColor;

	public Issue18933()
	{
		Label contentLabel = new Label
		{
			Text = "ContentView - Test background removal",
			Padding = 20
		};

		contentViewWithBackgroundColor = new ContentView
		{
			BackgroundColor = Colors.Purple,
			Content = contentLabel
		};

		Button contentButton = new Button
		{
			AutomationId = "Issue18933Btn",
			Text = "Set ContentView BackgroundColor to Null"
		};
		contentButton.Clicked += Button_Clicked;

		VerticalStackLayout mainLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 20,
			Children =
			{
				contentViewWithBackgroundColor,
				contentButton,
			}
		};

		Content = mainLayout;
	}
		
    private void Button_Clicked(object sender, EventArgs e)
	{
		contentViewWithBackgroundColor.BackgroundColor = null;
	}
}