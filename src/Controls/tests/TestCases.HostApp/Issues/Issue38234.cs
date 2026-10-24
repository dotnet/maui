#if ANDROID
using Android.Content.Res;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Platform;
#endif

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
			var theme = Application.Current!.UserAppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
			Application.Current.UserAppTheme = theme;
			statusLabel.Text = $"{theme} theme active";
			changeThemeButton.Text = theme == AppTheme.Dark ? "Switch to light theme" : "Switch to dark theme";
		};

#if ANDROID
		Label indicatorColorLabel = new Label { AutomationId = "IndicatorColorLabel" };
		Label expectedIndicatorColorLabel = new Label { AutomationId = "ExpectedIndicatorColorLabel" };
		Button readIndicatorButton = new Button
		{
			Text = "Read native selection indicator",
			AutomationId = "ReadIndicatorColorButton",
		};
		readIndicatorButton.Clicked += (sender, args) =>
		{
			var bottomView = (Handler?.PlatformView as global::Android.Views.ViewGroup)?
				.GetFirstChildOfType<BottomNavigationView>();
			if (bottomView?.ItemActiveIndicatorColor is not { } indicatorColor)
			{
				indicatorColorLabel.Text = "Native Material3 selection indicator unavailable";
				expectedIndicatorColorLabel.Text = "Native Material3 selection indicator unavailable";
				return;
			}

			using var configuration = new Configuration(bottomView.Context.Resources.Configuration);
			configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
				(Application.Current!.RequestedTheme == AppTheme.Dark ? UiMode.NightYes : UiMode.NightNo);

			using var context = bottomView.Context.CreateConfigurationContext(configuration);
			context.SetTheme(Microsoft.Maui.Resource.Style.Maui_Material3_Theme_NoActionBar);
			using var expectedView = new BottomNavigationView(context);

			indicatorColorLabel.Text = $"{indicatorColor.DefaultColor:X8}";
			expectedIndicatorColorLabel.Text = expectedView.ItemActiveIndicatorColor is { } expectedColor
				? $"{expectedColor.DefaultColor:X8}"
				: "Native Material3 theme indicator unavailable";
		};
#endif

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
#if ANDROID
							readIndicatorButton,
							indicatorColorLabel,
							expectedIndicatorColorLabel,
#endif
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