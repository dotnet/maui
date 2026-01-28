namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Issue 33731 reproduction: TabbedPage with continuous GC logs
		return new Window(new Issue33731TabbedPage());
	}
}

// Exact reproduction from https://github.com/dotnet/maui/issues/33731
public class Issue33731TabbedPage : TabbedPage
{
	private static int _sizeChangedCount = 0;
	private static Label? _statusLabel;

	public Issue33731TabbedPage()
	{
		// Status label to show size change events
		_statusLabel = new Label
		{
			Text = "SizeChanged Count: 0",
			BackgroundColor = Colors.Yellow,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Padding = new Thickness(10),
			FontSize = 18
		};

		// Create first tab with Grid (matching the exact repro from the issue)
		var tab1 = new ContentPage
		{
			Title = "Tab 1",
			Content = new Grid
			{
				Children =
				{
					new VerticalStackLayout
					{
						Spacing = 20,
						Children =
						{
							_statusLabel,
							new Label
							{
								Text = "Tab 1",
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center,
								FontSize = 24
							},
							new Label
							{
								Text = "Watch the SizeChanged count above.\nIf it keeps increasing, the bug is present.",
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center,
								FontSize = 14,
								TextColor = Colors.Gray
							}
						}
					}
				}
			}
		};

		// Create second tab with Grid (matching the exact repro from the issue)
		var tab2 = new ContentPage
		{
			Title = "Tab 2",
			Content = new Grid
			{
				Children =
				{
					new Label
					{
						Text = "Tab 2",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 24
					}
				}
			}
		};

		// Hook into SizeChanged on both tabs to detect thrashing
		tab1.SizeChanged += OnPageSizeChanged;
		tab2.SizeChanged += OnPageSizeChanged;

		// Also hook into the TabbedPage itself
		this.SizeChanged += OnPageSizeChanged;

		// Add tabs to TabbedPage
		Children.Add(tab1);
		Children.Add(tab2);
	}

	private static void OnPageSizeChanged(object? sender, EventArgs e)
	{
		_sizeChangedCount++;
		if (_statusLabel != null)
		{
			_statusLabel.Text = $"SizeChanged Count: {_sizeChangedCount}";
		}
		
		// Log to device logs for verification
		System.Diagnostics.Debug.WriteLine($"[Issue33731] SizeChanged event #{_sizeChangedCount}");
		Console.WriteLine($"SANDBOX: [Issue33731] SizeChanged event #{_sizeChangedCount}");
	}
}
