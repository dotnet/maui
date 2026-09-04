using Microsoft.Maui.Controls.Shapes;

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

	readonly ContentView _reproRoot;
	bool _loopDetected;
	int _sizeChangeCount;

	public Issue37892ReproPage()
	{
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

		var wrappingLabel = new Label
		{
			Margin = new Thickness(0, 0, 0, 12),
			FontFamily = "OpenSansRegular",
			FontSize = 16,
			Text = "This text does need to be two lines at least or else the looping will not begin 12345678901112131415"
		};

		var scrollView = new ScrollView
		{
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			VerticalScrollBarVisibility = ScrollBarVisibility.Never,
			Content = wrappingLabel
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

		_reproRoot = new ContentView
		{
			AutomationId = "Issue37892ReproRoot",
			VerticalOptions = LayoutOptions.End,
			Content = new Border
			{
				BackgroundColor = Colors.Green,
				Margin = new Thickness(16, 8),
				StrokeShape = new RoundRectangle { CornerRadius = 16 },
				Content = reproGrid
			}
		};

		Content = _reproRoot;

		_reproRoot.SizeChanged += OnReproRootSizeChanged;
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	async void OnReproRootSizeChanged(object sender, EventArgs e)
	{
		_sizeChangeCount++;

		if (_sizeChangeCount == LoopDetectionThreshold)
		{
			_loopDetected = true;
			SemanticProperties.SetDescription(_reproRoot, "Issue37892LayoutLoopDetected");
			_reproRoot.HeightRequest = 100;
			await DisplayAlertAsync(
				"Layout Loop Detected",
				$"The reproduction reached {LoopDetectionThreshold} root size changes.",
				"OK");
		}
	}

	async void OnLoaded(object sender, EventArgs e)
	{
		int stableCheckCount = 0;

		for (int i = 0; i < MaxSettleChecks; i++)
		{
			int countBeforeDelay = _sizeChangeCount;
			await Task.Delay(SettleCheckInterval);

			if (_loopDetected)
			{
				return;
			}

			if (_sizeChangeCount > 0 && countBeforeDelay == _sizeChangeCount)
			{
				stableCheckCount++;
				if (stableCheckCount == RequiredStableChecks)
				{
					await DisplayAlertAsync(
						"Layout Settled",
						"The ScrollView layout settled.",
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
}
