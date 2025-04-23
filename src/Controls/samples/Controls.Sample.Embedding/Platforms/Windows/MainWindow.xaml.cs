using Microsoft.UI.Xaml;

namespace Maui.Controls.Sample.WinUI;

public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
{
	EmbeddingScenarios.IScenario? _scenario;
	MyMauiContent? _mauiView;
	FrameworkElement? _nativeView;

	public MainWindow()
	{
		InitializeComponent();
	}

	private async void OnRootLayoutLoaded(object? sender, RoutedEventArgs e)
	{
		// Sometimes Loaded fires twice...
		if (_nativeView is not null)
			return;

		await Task.Yield();

		// Uncomment the scenario to test:
		//_scenario = new EmbeddingScenarios.Scenario1_Basic();
		//_scenario = new EmbeddingScenarios.Scenario2_Scoped();
		_scenario = new EmbeddingScenarios.Scenario3_Correct();

		// create the view and (maybe) the window
		(_mauiView, _nativeView) = _scenario.Embed(this);

		// add the new view to the UI
		RootLayout.Children.Insert(1, _nativeView);
	}

	private void OnWindowClosed(object? sender, WindowEventArgs args)
	{
		// Remove the view from the UI
		RootLayout.Children.Remove(_nativeView);

		// If we used a window, then clean that up
		if (_mauiView?.Window is IWindow window)
			window.Destroying();
	}

	private async void OnMagicClicked(object? sender, RoutedEventArgs e)
	{
		if (_mauiView?.DotNetBot is not Image bot)
			return;

		await bot.RotateToAsync(360, 1000);
		bot.Rotation = 0;

		bot.HeightRequest = 90;
	}

	private void OnNewWindowClicked(object? sender, RoutedEventArgs e)
	{
		var window = new MainWindow();
		window.Activate();
	}
}
