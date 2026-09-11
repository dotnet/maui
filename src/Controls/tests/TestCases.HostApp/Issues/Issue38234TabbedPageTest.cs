namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 1418, "Android Material 3 TabbedPage selection color does not update with AppTheme", PlatformAffected.Android)]
public class Issue38234TabbedPageTest : TabbedPage
{
	readonly AppTheme _originalUserAppTheme;
	public Issue38234TabbedPageTest()
	{
		var application = Application.Current!;

		_originalUserAppTheme = application.UserAppTheme;
		application.UserAppTheme = AppTheme.Light;

		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
			this,
			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

		// Same colors used by the XAML TabbedPage style.
		var white = Color.FromArgb("#FFFFFFFF");
		var gray950 = Color.FromArgb("#FF141414");
		var magenta = Color.FromArgb("#FFD600AA");
		var gray200 = Color.FromArgb("#FFC8C8C8");

		this.SetAppThemeColor(
			TabbedPage.BarBackgroundColorProperty,
			white,
			gray950);

		this.SetAppThemeColor(
			TabbedPage.BarTextColorProperty,
			magenta,
			white);

		this.SetAppThemeColor(
			TabbedPage.UnselectedTabColorProperty,
			gray200,
			gray950);

		this.SetAppThemeColor(
			TabbedPage.SelectedTabColorProperty,
			gray950,
			gray200);

		Label descriptionLabel = new Label
		{
			Text = "Tab 1",
			HorizontalOptions = LayoutOptions.Center
		};

		Button button = new Button
		{
			Text = "Switch to Dark Theme",
			AutomationId = "Issue38234TabbedPage_SwitchToDarkThemeButton"
		};
		button.Clicked += (s, e) =>
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			descriptionLabel.Text = "Dark theme active";
		};

		ContentPage tabOne = new ContentPage
		{
			Title = "Tab 1",
			IconImageSource = "groceries.png",
			Content = new VerticalStackLayout
			{
				Spacing = 16,
				Padding = 24,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					descriptionLabel,
					button
				}
			}
		};

		ContentPage tabTwo = new ContentPage
		{
			Title = "Tab 2",
			IconImageSource = "groceries.png",

			Content = new Label
			{
				Text = "Tab 2",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		Children.Add(tabOne);
		Children.Add(tabTwo);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Application.Current!.UserAppTheme = _originalUserAppTheme;
	}
}