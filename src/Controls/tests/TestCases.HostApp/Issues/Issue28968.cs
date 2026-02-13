namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28968, "[iOS] ActivityIndicator IsRunning ignores IsVisible when set to true", PlatformAffected.iOS)]
public class Issue28968 : ContentPage
{
	public Issue28968()
	{
		var activityIndicator = new ActivityIndicator
		{
			IsVisible = false,
			AutomationId = "MauiActivityIndicator"
		};

		var statusLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "StatusLabel"
		};

		var setRunningButton = new Button
		{
			Text = "Set IsRunning = true",
			AutomationId = "SetRunningButton",
			Command = new Command(() =>
			{
				activityIndicator.IsRunning = true;

				// Check the native platform hidden state after a delay to allow
				// Draw/LayoutSubviews to execute on iOS
				Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
				{
					bool nativeHidden = IsNativeViewHidden(activityIndicator);
					statusLabel.Text = nativeHidden ? "HIDDEN" : "VISIBLE";
				});
			})
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				activityIndicator,
				setRunningButton,
				statusLabel
			}
		};
	}

	static bool IsNativeViewHidden(ActivityIndicator indicator)
	{
		var handler = indicator.Handler;
		if (handler?.PlatformView is null)
			return true;

#if IOS || MACCATALYST
		if (handler.PlatformView is UIKit.UIView nativeView)
			return nativeView.Hidden;
#elif ANDROID
		if (handler.PlatformView is Android.Views.View nativeView)
			return nativeView.Visibility != Android.Views.ViewStates.Visible;
#elif WINDOWS
		if (handler.PlatformView is Microsoft.UI.Xaml.UIElement nativeView)
			return nativeView.Visibility != Microsoft.UI.Xaml.Visibility.Visible;
#endif
		return true;
	}
}
