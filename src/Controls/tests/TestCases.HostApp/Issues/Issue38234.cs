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

		Items.Add(new TabBar
		{
			Items =
			{
				new ShellContent
				{
					Title = "Home",
					Icon = "groceries.png",
					Content = CreateProbePage(this, color => SetTabBarForegroundColor(this, color), () => GetTabBarForegroundColor(this)),
				},
				new ShellContent
				{
					Title = "Settings",
					Icon = "groceries.png",
					Content = new ContentPage { Title = "Settings", Content = new Label { Text = "Settings" } },
				}
			}
		});
	}

	internal static ContentPage CreateProbePage(Page owner, Action<Color> setSelectedColor, Func<Color> getSelectedColor)
	{
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
		Button nativeSelectionButton = new Button
		{
			Text = "Use native selection",
			AutomationId = "UseNativeSelectionColorButton",
		};
		nativeSelectionButton.Clicked += (sender, args) => setSelectedColor(null);
		Button explicitSelectionButton = new Button
		{
			Text = "Use orange selection",
			AutomationId = "UseExplicitSelectionColorButton",
		};
		explicitSelectionButton.Clicked += (sender, args) => setSelectedColor(Colors.Orange);
		Button readIndicatorButton = new Button
		{
			Text = "Read native selection indicator",
			AutomationId = "ReadIndicatorColorButton",
		};
		readIndicatorButton.Clicked += (sender, args) =>
		{
			var platformView = owner.Handler?.PlatformView as global::Android.Views.View;
			BottomNavigationView bottomView = null;
			while (platformView is not null && bottomView is null)
			{
				bottomView = (platformView as global::Android.Views.ViewGroup)?.GetFirstChildOfType<BottomNavigationView>();
				platformView = platformView.Parent as global::Android.Views.View;
			}
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

			var checkedState = new[] { global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StateChecked };
			var expectedIconColor = getSelectedColor()?.ToPlatform().ToArgb() ??
				expectedView.ItemIconTintList?.GetColorForState(checkedState,
					new global::Android.Graphics.Color(expectedView.ItemIconTintList.DefaultColor));

			indicatorColorLabel.Text = $"{indicatorColor.DefaultColor:X8}/{ReadSelectedIconColor(bottomView)}";
			expectedIndicatorColorLabel.Text = expectedView.ItemActiveIndicatorColor is { } expectedColor && expectedIconColor is int iconColor
				? $"{expectedColor.DefaultColor:X8}/{iconColor:X8}"
				: "Native Material3 theme indicator unavailable";
		};
#endif

		ContentPage homePage = new ContentPage
		{
			Title = "Home",
			IconImageSource = "groceries.png",
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
							new HorizontalStackLayout
							{
								Spacing = 8,
								Children = { nativeSelectionButton, explicitSelectionButton }
							},
							readIndicatorButton,
							indicatorColorLabel,
							expectedIndicatorColorLabel,
#endif
						}
					}
				}
			}
		};

		return homePage;
	}

	[Issue(IssueTracker.Github, 38234, "Android Material 3 TabbedPage native selection colors follow AppTheme", PlatformAffected.Android, issueTestNumber: 2)]
	public class Issue38234NativeTabbedPage : TabbedPage
	{
		readonly AppTheme _originalUserAppTheme;

		public Issue38234NativeTabbedPage()
		{
			_originalUserAppTheme = Application.Current!.UserAppTheme;
			Application.Current.UserAppTheme = AppTheme.Light;
			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
				this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
			BarTextColor = null;
			UnselectedTabColor = null;
			SelectedTabColor = null;
			Children.Add(Issue38234.CreateProbePage(this, color => SelectedTabColor = color, () => SelectedTabColor));
			Children.Add(new ContentPage { Title = "Settings", IconImageSource = "groceries.png", Content = new Label { Text = "Settings" } });
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Application.Current!.UserAppTheme = _originalUserAppTheme;
		}
	}

#if ANDROID
	static string ReadSelectedIconColor(BottomNavigationView bottomView)
	{
		var selectedItem = bottomView.FindViewById<global::Android.Views.View>(bottomView.SelectedItemId);
		var icon = selectedItem?.FindViewById<global::Android.Widget.ImageView>(
			Microsoft.Maui.Resource.Id.navigation_bar_item_icon_view);
		if (icon is null || icon.Width == 0 || icon.Height == 0)
			return "Native selected icon unavailable";

		using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(icon.Width, icon.Height, global::Android.Graphics.Bitmap.Config.Argb8888);
		using var canvas = new global::Android.Graphics.Canvas(bitmap);
		icon.Draw(canvas);
		var pixels = new int[icon.Width * icon.Height];
		bitmap.GetPixels(pixels, 0, icon.Width, 0, 0, icon.Width, icon.Height);
		foreach (var pixel in pixels)
		{
			if ((uint)pixel >> 24 == 255)
				return $"{pixel:X8}";
		}

		return "Native selected icon has no opaque pixels";
	}
#endif

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Application.Current!.UserAppTheme = _originalUserAppTheme;
	}
}