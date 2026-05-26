namespace TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30846, "Media Playback Customization in HybridWebView", PlatformAffected.All)]
public partial class Issue30846 : ContentPage
{
	Label _videoStatusLabel;
	Label _audioStatusLabel;

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
				new RowDefinition(GridLength.Auto), // Second row: video status
				new RowDefinition(GridLength.Auto), // Third row: audio status
				new RowDefinition(GridLength.Star)  // Forth row: webview container fills remaining height
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
		Grid.SetRow(webViewContainer, 3);
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

		_videoStatusLabel = new Label
		{
			AutomationId = "VideoStatusLabel",
			Text = "Loading...",
		};
		Grid.SetRow(_videoStatusLabel, 1);
		Grid.SetColumnSpan(_videoStatusLabel, 2);
		grid.Add(_videoStatusLabel);

		_audioStatusLabel = new Label
		{
			AutomationId = "AudioStatusLabel",
			Text = "Loading...",
		};
		Grid.SetRow(_audioStatusLabel, 2);
		Grid.SetColumnSpan(_audioStatusLabel, 2);
		grid.Add(_audioStatusLabel);

		Content = grid;
	}

	HybridWebView CreateHybridWebView(bool requireUserGesture)
	{
		var hybridWebView = new HybridWebView
		{
			AutomationId = "HybridWebView",
			DefaultFile = "issues/issue-30846.html",
			HybridRoot = "hybridroot",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
		};

		hybridWebView.RawMessageReceived += (s, e) =>
		{
			Dispatcher.Dispatch(() =>
			{
				if (e.Message.StartsWith("Video"))
					_videoStatusLabel.Text = e.Message;
				else if (e.Message.StartsWith("Audio"))
					_audioStatusLabel.Text = e.Message;
			});
		};

		hybridWebView.WebViewInitializing += (s, e) =>
		{
#if IOS || MACCATALYST
			// Make things look better
			e.PlatformArgs.Configuration.AllowsInlineMediaPlayback = true;

			// Set media playback requirements based on the switch state
			e.PlatformArgs.Configuration.MediaTypesRequiringUserActionForPlayback = requireUserGesture
				? WebKit.WKAudiovisualMediaTypes.All
				: WebKit.WKAudiovisualMediaTypes.None;
#elif ANDROID
			// Set media playback requirements based on the switch state
			e.PlatformArgs.Settings.MediaPlaybackRequiresUserGesture = requireUserGesture;
#elif WINDOWS
			// WebView2 requires that different environments have different UDF to support multiple simultaneous instances.
			// Technically we are swapping out the instances, but the first instance takes time to shut down.
			var lad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			e.PlatformArgs.UserDataFolder = Path.Combine(lad, "Controls.TestCases.HostApp", $"UserDataFolder-{requireUserGesture}");

			// Set media playback requirements based on the switch state
			e.PlatformArgs.EnvironmentOptions = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions
			{
				AdditionalBrowserArguments = requireUserGesture
					? ""
					: "--autoplay-policy=no-user-gesture-required"
			};
#endif
		};

		return hybridWebView;
	}
}
