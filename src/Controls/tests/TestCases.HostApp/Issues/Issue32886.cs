namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32886, "[Android, iOS, Mac] Entry ClearButton not visible on dark theme", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32886 : TestContentPage
{
	Label _themeLabel;

	protected override void Init()
	{
		Title = "Issue32886";

		// Create the UITestEntry with ClearButtonVisibility
		var entry = new UITestEntry
		{
			Text = "Entry Text",
			IsCursorVisible = false,
			IsSpellCheckEnabled = false,
			IsTextPredictionEnabled = false,
			AutomationId = "TestEntry",
			ClearButtonVisibility = ClearButtonVisibility.WhileEditing
		};

		var button = new Button
		{
			Text = "Change theme",
			AutomationId = "ThemeButton"
		};
		button.Clicked += Button_Clicked;

		_themeLabel = new Label
		{
			Text = "Light",
			AutomationId = "ThemeLabel",
			HeightRequest = 0,
			Opacity = 0
		};

		var layout = new VerticalStackLayout();
		layout.Children.Add(entry);
		layout.Children.Add(button);
		layout.Children.Add(_themeLabel);

		Content = layout;

		// Set background color based on app theme
		this.SetAppThemeColor(BackgroundColorProperty, Colors.White, Colors.Black);
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		if (Application.Current is not null)
		{
			var newTheme = Application.Current.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
			Application.Current.UserAppTheme = newTheme;
			_themeLabel.Text = newTheme == AppTheme.Dark ? "Dark" : "Light";
		}
	}
}