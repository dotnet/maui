namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18933, "ContentView Background Color Not Cleared When Set to Null", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue18933 : ContentPage
{
	ContentView contentView;
	ContentView contentViewWithBackground;

	public Issue18933()
	{
		Label contentLabel = new Label
		{
			Text = "ContentView - Test BackgroundColor removal",
			Padding = 20
		};

		Label backgroundContentLabel = new Label
		{
			Text = "ContentView - Test Background removal",
			Padding = 20
		};

		contentView = new ContentView
		{
			BackgroundColor = Colors.Purple,
			Content = contentLabel
		};

		contentViewWithBackground = new ContentView
		{
			Background = Colors.Purple,
			Content = backgroundContentLabel
		};

		Button clearBackgroundBtn = new Button
		{
			AutomationId = "clearBgBtn",
			Text = "Set ContentView Background to Null"
		};
		clearBackgroundBtn.Clicked += Button_Clicked;

		Button setBackgroundBtn = new Button
		{
			AutomationId = "setBgBtn",
			Text = "Set ContentView Background"
		};
		setBackgroundBtn.Clicked += SetBackground_Clicked;

		VerticalStackLayout mainLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 20,
			Children =
			{
				contentView,
				contentViewWithBackground,
				clearBackgroundBtn,
				setBackgroundBtn,
			}
		};

		Content = mainLayout;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		contentView.BackgroundColor = null;
		contentViewWithBackground.Background = null;
	}

	void SetBackground_Clicked(object sender, EventArgs e)
	{
		contentView.BackgroundColor = Colors.Purple;
		contentViewWithBackground.Background = Colors.Purple;
	}
}