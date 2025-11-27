namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32886, "[Android, iOS, Mac] Entry ClearButton not visible on dark theme", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32886 : TestContentPage
{
	protected override void Init()
	{
		Title = "Issue32886";

		// Create the UITestEntry with ClearButtonVisibility
		var entry = new UITestEntry
		{
			Text = "Entry Text",
			IsCursorVisible = false,
			AutomationId = "TestEntry",
			ClearButtonVisibility = ClearButtonVisibility.WhileEditing
		};

		var button = new Button
		{
			Text = "Change theme",
			AutomationId = "ThemeButton"
		};
		button.Clicked += Button_Clicked;

		var layout = new VerticalStackLayout();
		layout.Children.Add(entry);
		layout.Children.Add(button);

		Content = layout;

		// Set background color based on app theme
		this.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		if (Application.Current is not null)
		{
			Application.Current.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
		}
	}
}