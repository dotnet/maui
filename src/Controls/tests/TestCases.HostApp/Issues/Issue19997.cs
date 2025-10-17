namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19997, "[Android, iOS, MacOS] Entry ClearButton Color Not Updating on AppThemeBinding Change", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue19997 : TestContentPage
{
	protected override void Init()
	{
		Title = "Issue19997";

		// Create the UITestEntry with app theme binding for TextColor
		var entry = new UITestEntry
		{
			Text = "Hii",
			IsCursorVisible = false,
			AutomationId = "EntryWithAppThemeBinding",
			ClearButtonVisibility = ClearButtonVisibility.WhileEditing
		};

		// Set up app theme binding for TextColor (Light=Blue, Dark=Red)
		entry.SetAppThemeColor(Entry.TextColorProperty, Colors.Blue, Colors.Red);

		// Create the button
		var button = new Button
		{
			Text = "Change theme",
			AutomationId = "ThemeButton"
		};
		button.Clicked += Button_Clicked;

		// Create the layout and add controls
		var layout = new VerticalStackLayout();
		layout.Children.Add(entry);
		layout.Children.Add(button);

		Content = layout;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		Application.Current?.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
	}
}
