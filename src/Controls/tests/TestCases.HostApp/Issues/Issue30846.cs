namespace TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30846, "Media Playback Customization in HybridWebView", PlatformAffected.All)]
public partial class Issue30846 : ContentPage
{
	public Issue30846()
	{
		Title = "HybridWebView Autoplay Test";

		var grid = new Grid
		{
			Padding = new Thickness(24),
			RowSpacing = 8,
			ColumnSpacing = 8,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto), // First row: switch and label
				new RowDefinition(GridLength.Star)  // Second row: webview container fills remaining height
			},
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Auto), // First column: switch
				new ColumnDefinition(GridLength.Star)  // Second column: label
			}
		};

		var webViewContainer = new ContentView
		{
			AutomationId = "WebViewContainer",
			Content = CreateHybridWebView(true),
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
		};
		grid.Add(webViewContainer);
		Grid.SetRow(webViewContainer, 1);
		Grid.SetColumnSpan(webViewContainer, 2);

		var switchControl = new Switch
		{
			AutomationId = "AutoPlaybackSwitch",
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			IsToggled = false,
		};
		switchControl.Toggled += (sender, e) => webViewContainer.Content = CreateHybridWebView(!e.Value);
		Grid.SetRow(switchControl, 0);
		Grid.SetColumn(switchControl, 0);
		grid.Add(switchControl);

		var switchLabel = new Label
		{
			Text = "Auto Playback",
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
		};
		Grid.SetRow(switchLabel, 0);
		Grid.SetColumn(switchLabel, 1);
		grid.Add(switchLabel);

		Content = grid;
	}

	static HybridWebView CreateHybridWebView(bool requireUserGesture)
	{
		var hybridWebView = new HybridWebView
		{
			AutomationId = "HybridWebView",
			DefaultFile = "issues/issue-30846.html",
			HybridRoot = "hybridroot",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
		};

		hybridWebView.WebViewInitializing += (s, e) =>
		{
#if IOS || MACCATALYST
			// Make things look decent
			e.PlatformArgs.Configuration.AllowsInlineMediaPlayback = true;
			e.PlatformArgs.Configuration.AllowsAirPlayForMediaPlayback = true;
			e.PlatformArgs.Configuration.AllowsPictureInPictureMediaPlayback = true;

			// Set media playback requirements based on the switch state
			e.PlatformArgs.Configuration.MediaTypesRequiringUserActionForPlayback = requireUserGesture
				? WebKit.WKAudiovisualMediaTypes.All
				: WebKit.WKAudiovisualMediaTypes.None;
#elif ANDROID
			// Set media playback requirements based on the switch state
			e.PlatformArgs.Settings.MediaPlaybackRequiresUserGesture = requireUserGesture;
#elif WINDOWS
			// Set media playback requirements based on the switch state
			e.PlatformArgs.EnvironmentOptions.AdditionalBrowserArguments = requireUserGesture
				? "--autoplay-policy=no-user-gesture-required"
				: "";
#endif
		};

		return hybridWebView;
	}
}
