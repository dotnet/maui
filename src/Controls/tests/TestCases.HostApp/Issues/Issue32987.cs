namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32987, "Android edge-to-edge: system bars are transparent, the 3-button navigation scrim is removed, and status bar icons follow the app theme", PlatformAffected.Android)]
public class Issue32987 : NavigationPage
{
	public Issue32987() : base(new Issue32987BarStatePage())
	{
	}
}

// Surfaces the Android Window's edge-to-edge state in labels so the Appium test can assert it,
// since Appium cannot read window flags/colors directly.
class Issue32987BarStatePage : ContentPage
{
	readonly Label _barsTransparent = new() { AutomationId = "BarTransparentValue" };
	readonly Label _navScrim = new() { AutomationId = "NavScrimValue" };
	readonly Label _lightIcons = new() { AutomationId = "LightIconsValue" };
	readonly Label _theme = new() { AutomationId = "ThemeValue" };

	// Invisible markers the test waits on; set in the same pass as the values so they are consistent
	// when a marker appears. An IsVisible=false element is absent from the tree until made visible.
	readonly Label _ready = new() { AutomationId = "BarStateReady", IsVisible = false };
	readonly Label _themeIsLight = new() { AutomationId = "ThemeIsLight", IsVisible = false };
	readonly Label _themeIsDark = new() { AutomationId = "ThemeIsDark", IsVisible = false };

	public Issue32987BarStatePage()
	{
		Title = "Issue 32987";

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				LabeledRow("Bars transparent:", _barsTransparent),
				LabeledRow("Nav scrim enforced:", _navScrim),
				LabeledRow("Status bar light-appearance icons:", _lightIcons),
				LabeledRow("App theme:", _theme),
				_ready,
				_themeIsLight,
				_themeIsDark,
			},
		};

		Application.Current!.RequestedThemeChanged += OnRequestedThemeChanged;
	}

	static View LabeledRow(string caption, Label value) =>
		new HorizontalStackLayout
		{
			Spacing = 6,
			Children = { new Label { Text = caption }, value },
		};

	void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e) => ScheduleRefresh();

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Follow the device theme so the SetLightTheme/SetDarkTheme UI-test helpers drive the app.
		Application.Current!.UserAppTheme = AppTheme.Unspecified;
		ScheduleRefresh();
	}

	// Read after the current frame so the activity's OnConfigurationChanged has re-applied the bar
	// appearance for the new theme first.
	void ScheduleRefresh() => Dispatcher.Dispatch(RefreshBarState);

	void RefreshBarState()
	{
#if ANDROID
		var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
		var window = activity?.Window;
		if (window is null)
			return;

#pragma warning disable CA1422 // StatusBarColor/NavigationBarColor getters are deprecated on API 35+ but still report the current value
		var statusArgb = (uint)window.StatusBarColor;
		var navArgb = (uint)window.NavigationBarColor;
#pragma warning restore CA1422
		bool barsTransparent = (statusArgb >> 24) == 0 && (navArgb >> 24) == 0; // alpha channel == 0

		bool navScrim = OperatingSystem.IsAndroidVersionAtLeast(29) && window.NavigationBarContrastEnforced;

		bool lightIcons = false;
		if (window.DecorView is global::Android.Views.View decorView)
		{
			var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, decorView);
			lightIcons = controller?.AppearanceLightStatusBars ?? false;
		}

		var configuration = activity?.Resources?.Configuration;
		bool isLightTheme = configuration is null ||
			(configuration.UiMode & Android.Content.Res.UiMode.NightMask) != Android.Content.Res.UiMode.NightYes;

		_barsTransparent.Text = barsTransparent.ToString();
		_navScrim.Text = navScrim.ToString();
		_lightIcons.Text = lightIcons.ToString();
		_theme.Text = isLightTheme ? "Light" : "Dark";

		// Set markers in the same pass as the values: _ready latches after the first read; the theme
		// markers signal which theme this read reflected.
		_themeIsLight.IsVisible = isLightTheme;
		_themeIsDark.IsVisible = !isLightTheme;
		_ready.IsVisible = true;
#endif
	}
}
