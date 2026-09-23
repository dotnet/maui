using Microsoft.Maui.Controls.Shapes;
#if IOS
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37892, "ScrollView enters an infinite measure loop near the scrollability boundary", PlatformAffected.iOS)]
public class Issue37892 : ContentPage
{
	public Issue37892()
	{
		Title = "Issue 37892";

		var launchButton = new Button
		{
			AutomationId = "Issue37892LaunchButton",
			Text = "Launch reproduction"
		};
		launchButton.Clicked += OnLaunchClicked;

		Content = new VerticalStackLayout
		{
			Padding = 24,
			Spacing = 16,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 22,
					Text = "Infinite measure loop on iOS"
				},
				new Label
				{
					Text = "Launches the reproduction inside a nested NavigationPage."
				},
				launchButton
			}
		};
	}

	async void OnLaunchClicked(object sender, EventArgs e)
	{
		if (Window is null)
			throw new InvalidOperationException("The issue page must be attached to a window before launching the reproduction.");

		var outerPage = new ContentPage { Title = "Issue 37892" };
		var outerNavigationPage = new NavigationPage(outerPage);

		Window.Page = outerNavigationPage;
		await outerNavigationPage.PushAsync(
			new NavigationPage(new Issue37892ReproPage()),
			animated: false);
	}
}

class Issue37892ReproPage : ContentPage
{
	const int LoopDetectionThreshold = 50;
	const int MaxSettleChecks = 40;
	const int RequiredStableChecks = 8;
	static readonly TimeSpan SettleCheckInterval = TimeSpan.FromMilliseconds(250);
	static readonly ReproConfiguration IOS26BoundaryConfiguration =
		new("iOS 26 boundary configuration (Margin 12, Bottom 10, Font 15.5)", 12, 10, 15.5, "This text does need to be two lines at least or else the looping will not begin", null);

	readonly Border _border;
	readonly ReproConfiguration _configuration;
	readonly ContentView _reproRoot;
	readonly Label _wrappingLabel;
	ReproConfiguration _currentConfiguration;
	bool _loopDetected;
	int _sizeChangeCount;

	public Issue37892ReproPage()
	{
		_configuration = GetConfiguration();
		_currentConfiguration = _configuration;

		var header = new ContentView
		{
			ControlTemplate = new ControlTemplate(() =>
			{
				var headerGrid = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Auto),
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Auto)
					}
				};

				var headerBorder = new Border
				{
					Margin = new Thickness(0, 0, -4, 0),
					BackgroundColor = Color.FromArgb("#E1E1E1"),
					HeightRequest = 40,
					HorizontalOptions = LayoutOptions.End,
					StrokeShape = new RoundRectangle { CornerRadius = 20 },
					VerticalOptions = LayoutOptions.Start,
					WidthRequest = 40
				};
				headerGrid.Add(headerBorder, 2);

				return headerGrid;
			})
		};

		_wrappingLabel = new Label
		{
			FontSize = _configuration.FontSize,
			Margin = new Thickness(0, 0, 0, _configuration.LabelBottomMargin),
			Text = _configuration.LabelText
		};

		if (_configuration.FontFamily is not null)
			_wrappingLabel.FontFamily = _configuration.FontFamily;

		var scrollView = new ScrollView
		{
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			VerticalScrollBarVisibility = ScrollBarVisibility.Never,
			Content = _wrappingLabel
		};

		var reproGrid = new Grid
		{
			Padding = new Thickness(16, 12, 16, 0),
			IsClippedToBounds = true,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},
			RowSpacing = 6
		};
		reproGrid.Add(header, 0, 0);
		reproGrid.Add(scrollView, 0, 1);

		_border = new Border
		{
			Margin = new Thickness(_configuration.HorizontalMargin, 8),
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Content = reproGrid
		};

		_reproRoot = new ContentView
		{
			AutomationId = "Issue37892ReproRoot",
			VerticalOptions = LayoutOptions.End,
			Content = _border
		};

		Content = _reproRoot;
		_reproRoot.SizeChanged += OnReproRootSizeChanged;
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	async void OnReproRootSizeChanged(object sender, EventArgs e)
	{
		_sizeChangeCount++;
		if (_loopDetected || _sizeChangeCount < LoopDetectionThreshold)
			return;

		_loopDetected = true;
		_reproRoot.HeightRequest = 100;
		await DisplayAlertAsync(
			"Layout Loop Detected",
			$"{_currentConfiguration.Name} reached {LoopDetectionThreshold} root size changes.",
			"OK");
	}

	async void OnLoaded(object sender, EventArgs e)
	{
		if (IsIOS26OrHigher())
		{
			ApplyConfiguration(IOS26BoundaryConfiguration);
			await Task.Delay(TimeSpan.FromMilliseconds(100));

			if (_loopDetected)
				return;
		}

		int stableCheckCount = 0;

		for (int i = 0; i < MaxSettleChecks; i++)
		{
			int countBeforeDelay = _sizeChangeCount;
			await Task.Delay(SettleCheckInterval);

			if (_loopDetected)
				return;

			if (_sizeChangeCount > 0 && countBeforeDelay == _sizeChangeCount)
			{
				stableCheckCount++;
				if (stableCheckCount == RequiredStableChecks)
				{
					await DisplayAlertAsync(
						"Layout Settled",
						$"The ScrollView layout settled using {_currentConfiguration.Name}.",
						"OK");
					return;
				}
			}
			else
			{
				stableCheckCount = 0;
			}
		}
	}

	void OnUnloaded(object sender, EventArgs e)
	{
		_reproRoot.SizeChanged -= OnReproRootSizeChanged;
		Loaded -= OnLoaded;
		Unloaded -= OnUnloaded;
	}

	static ReproConfiguration GetConfiguration()
	{
#if IOS
		if (UIDevice.CurrentDevice.CheckSystemVersion(26, 0))
		{
			return new(
				"iOS 26 initial configuration (Margin 16, Bottom 12, Font 16)",
				16,
				12,
				16,
				"This text does need to be two lines at least or else the looping will not begin",
				null);
		}
#endif

		return new(
			"iOS 18 boundary configuration (Margin 16, Bottom 12, Font 16)",
			16,
			12,
			16,
			"This text does need to be two lines at least or else the looping will not begin 12345678901112131415",
			"OpenSansRegular");
	}

	void ApplyConfiguration(ReproConfiguration configuration)
	{
		_currentConfiguration = configuration;
		Title = configuration.Name;
		_border.Margin = new Thickness(configuration.HorizontalMargin, 8);
		_wrappingLabel.Margin = new Thickness(0, 0, 0, configuration.LabelBottomMargin);
		_wrappingLabel.FontSize = configuration.FontSize;
		_wrappingLabel.FontFamily = configuration.FontFamily;
		_wrappingLabel.Text = configuration.LabelText;
	}

	static bool IsIOS26OrHigher()
	{
#if IOS
		return UIDevice.CurrentDevice.CheckSystemVersion(26, 0);
#else
		return false;
#endif
	}

	readonly record struct ReproConfiguration(
		string Name,
		double HorizontalMargin,
		double LabelBottomMargin,
		double FontSize,
		string LabelText,
		string FontFamily);
}
