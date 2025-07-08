namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29954, "The status bar is blank when ManualMAUITests sample project debugging on the Android API 36 emulator", PlatformAffected.Android)]
public class Issue29954 : ContentPage
{
	public Issue29954()
	{
		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "Android API 36 Status Bar Test",
					FontSize = 18,
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10, 50, 10, 10)
				},
				new Label
				{
					Text = "On Android API 36 (Android 16), the status bar should display correctly with proper light/dark appearance based on the current theme, not appear blank.",
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10),
					AutomationId = "StatusBarDescriptionLabel"
				},
				new Button
				{
					Text = "Open Modal to Test Status Bar",
					AutomationId = "OpenModalButton",
					Command = new Command(() =>
					{
						Window!.Page!.Navigation.PushModalAsync(new ContentPage
						{
							Title = "Modal Page",
							Content = new StackLayout
							{
								Children =
								{
									new Label
									{
										Text = "Modal Page - Status bar should still be visible and properly themed",
										VerticalOptions = LayoutOptions.Center,
										HorizontalOptions = LayoutOptions.Center,
										AutomationId = "ModalStatusBarTestLabel"
									},
									new Button
									{
										Text = "Close Modal",
										AutomationId = "CloseModalButton",
										Command = new Command(async () =>
										{
											await Window!.Page!.Navigation.PopModalAsync();
										})
									}
								}
							}
						}, false);
					})
				},
				new Label
				{
					Text = $"Current Android API Level: {GetAndroidApiLevel()}",
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10),
					AutomationId = "ApiLevelLabel"
				}
			}
		};
	}

	string GetAndroidApiLevel()
	{
#if ANDROID
 		return Android.OS.Build.VERSION.SdkInt.ToString();
#else
		return "N/A (Not Android)";
#endif
	}
} 