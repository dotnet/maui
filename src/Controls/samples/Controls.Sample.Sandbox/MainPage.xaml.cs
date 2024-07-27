using Microsoft.Maui.Graphics.Platform;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	private int counter;

	public MainPage()
	{
		InitializeComponent();
	}

	private void NewWindowButton_Clicked(object sender, EventArgs e)
	{
		counter++;
		TestPage page = new()
		{
			Description = $"#{counter}"
		};

		Window secondWindow = new Window(page);
		Application.Current!.OpenWindow(secondWindow);
	}

	private void ActivateWindow2_Clicked(object sender, EventArgs e)
	{
		IReadOnlyList<Window> windows = Application.Current!.Windows;

		debugInfo.Text += "#0 ";

		int windowNumber = int.Parse(windowToActivate.Text);
		int windowIndex = windowNumber - 1;

		debugInfo.Text += $"windows.count {windows.Count} ?? {windowNumber}";	

		if (windows.Count >= windowNumber)
		{
			Window windowToActivate = windows[windowIndex];
			Application.Current!.ActivateWindow(windowToActivate);
		}
	}

}
