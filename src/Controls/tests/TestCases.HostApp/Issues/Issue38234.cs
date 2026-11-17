namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38234, "Android Material 3 Shell TabBar selection color does not update with AppTheme", PlatformAffected.Android, issueTestNumber: 0)]
public class Issue38234 : Shell
{
	readonly AppTheme _originalUserAppTheme;

	public Issue38234()
	{
		var application = Application.Current!;
		_originalUserAppTheme = application.UserAppTheme;
		application.UserAppTheme = AppTheme.Light;

		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Material 3 baseline roles; the selected icon intentionally uses the TabbedPage test colors.
		// https://m3.material.io/components/navigation-bar/specs
		var onSurfaceLight = Color.FromArgb("#1D1B20");
		var onSurfaceDark = Color.FromArgb("#E6E0E9");
		this.SetAppThemeColor(Shell.BackgroundColorProperty,
			Color.FromArgb("#FEF7FF"), Color.FromArgb("#141218"));
		this.SetAppThemeColor(Shell.TitleColorProperty, onSurfaceLight, onSurfaceDark);
		this.SetAppThemeColor(Shell.TabBarBackgroundColorProperty,
			Color.FromArgb("#F3EDF7"), Color.FromArgb("#211F26"));
		this.SetAppThemeColor(Shell.TabBarTitleColorProperty, Color.FromArgb("#D600AA"), onSurfaceDark);
		this.SetAppThemeColor(Shell.TabBarForegroundColorProperty,
			Color.FromArgb("#D600AA"), Color.FromArgb("#FFFFFF"));
		this.SetAppThemeColor(Shell.TabBarUnselectedColorProperty,
			Color.FromArgb("#49454F"), Color.FromArgb("#CAC4D0"));

		Label statusLabel = new Label
		{
			Text = "Light theme active",
			AutomationId = "ThemeStatusLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		Label descriptionLabel = new Label
		{
			Text = "The test passes if the Tab selection color updates when the AppTheme changes.",
			AutomationId = "ThemeStatusDescriptionLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		Button changeThemeButton = new Button
		{
			Text = "Switch to dark theme",
			AutomationId = "SwitchToDarkThemeButton",
		};

		changeThemeButton.Clicked += (sender, args) =>
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
			statusLabel.Text = "Dark theme active";
		};

		ContentPage homePage = new ContentPage
		{
			Title = "Home",
			Content = new Grid
			{
				Padding = 24,
				Children =
				{
					new VerticalStackLayout
					{
						Spacing = 16,
						VerticalOptions = LayoutOptions.Center,
						Children =
						{
							descriptionLabel,
							statusLabel,
							changeThemeButton,
						}
					}
				}
			}
		};

		TabBar tabBar = new TabBar
		{
			Items =
			{
				new ShellContent
				{
					Title = "Home",
					Icon = "groceries.png",
					Content = homePage,
				},
				new ShellContent
				{
					Title = "Settings",
					Icon = "groceries.png",
					Content = new ContentPage
					{
						Title = "Settings",
						Content = new Label
						{
							Text = "Settings",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
						}
					}
				}
			}
		};

		Items.Add(tabBar);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Application.Current!.UserAppTheme = _originalUserAppTheme;
	}
}