using Microsoft.UI.Xaml;

namespace Maui.Controls.Sample.WinUI;

public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void OnRootLayoutLoaded(object? sender, RoutedEventArgs e)
	{
	}

	private void OnWindowClosed(object? sender, WindowEventArgs args)
	{
	}

	private void OnMagicClicked(object? sender, RoutedEventArgs e)
	{
	}

	private void OnNewWindowClicked(object? sender, RoutedEventArgs e)
	{
		var window = new MainWindow();
		window.Activate();
	}
}
