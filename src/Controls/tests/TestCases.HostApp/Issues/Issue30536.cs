#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30536, "[Windows] PointerGestureRecognizer behaves incorrectly when multiple windows are open", PlatformAffected.UWP)]
public class Issue30536 : ContentPage
{
	Button newWindowButton;
	Button closeNewWindowButton;
	Button minimizeSecondWindowButton;
	Label pointerEnterCountLabel;
	Label pointerExitCountLabel;
	Border theBorder;
	int enterCount = 0;
	int exitCount = 0;
	Window secondWindow;
	public Issue30536()
	{
		InitializeUI();
	}

	private void InitializeUI()
	{
		// Create the VerticalStackLayout
		var stackLayout = new VerticalStackLayout
		{
			Spacing = 5
		};

		// Create New Window button
		newWindowButton = new Button
		{
			Text = "New Window",
			AutomationId = "NewWindowButton"
		};
		newWindowButton.Clicked += OnNewWindowButton_Clicked;

		closeNewWindowButton = new Button
		{
			Text = "Close New Window",
			AutomationId = "CloseNewWindowButton"
		};
		closeNewWindowButton.Clicked += (sender, e) =>
		{
			if (secondWindow != null)
			{
				Application.Current.CloseWindow(secondWindow);
				secondWindow = null;
			}
		};

		minimizeSecondWindowButton = new Button
		{
			Text = "Minimize Second Window",
			AutomationId = "MinimizeSecondWindowButton"
		};
		minimizeSecondWindowButton.Clicked += (sender, e) =>
		{
			if (secondWindow != null)
			{
				MinimizeWindow(secondWindow);
			}
		};

		// Create pointer enter count label
		pointerEnterCountLabel = new Label
		{
			Text = $"Pointer Enter Count: {enterCount}",
			AutomationId = "PointerEnterCountLabel"

		};

		pointerExitCountLabel = new Label
		{
			Text = $"Pointer Exit Count: {exitCount}",
			AutomationId = "PointerExitCountLabel"

		};

		// Create the border with red background
		theBorder = new Border
		{
			WidthRequest = 500,
			HeightRequest = 500,
			BackgroundColor = Colors.Red,
			Content = new Button
			{
				Text = "Tap Me",
				AutomationId = "BorderButton"
			}
		};

		// Create and add PointerGestureRecognizer
		var pointerGestureRecognizer = new PointerGestureRecognizer();
		pointerGestureRecognizer.PointerEntered += OnPointerGestureRecognizer_PointerEntered;
		pointerGestureRecognizer.PointerExited += OnPointerGestureRecognizer_PointerExited;
		theBorder.GestureRecognizers.Add(pointerGestureRecognizer);

		// Add all elements to the stack layout
		stackLayout.Children.Add(minimizeSecondWindowButton);
		stackLayout.Children.Add(newWindowButton);
		stackLayout.Children.Add(closeNewWindowButton);
		stackLayout.Children.Add(pointerEnterCountLabel);
		stackLayout.Children.Add(pointerExitCountLabel);
		stackLayout.Children.Add(theBorder);

		// Set the content
		Content = stackLayout;
	}

#if WINDOWS
	// Windows API declarations
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
 
	// ShowWindow constants
	private const int SW_MINIMIZE = 6;
#endif

	private void OnNewWindowButton_Clicked(object sender, EventArgs e)
	{
		secondWindow = new Window(new ContentPage());
		Application.Current.OpenWindow(secondWindow);
	}

	private void MinimizeWindow(Window window)
	{
#if WINDOWS
		if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window win)
		{
			// Get window handle directly from MAUI's platform view
			var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(win);
			if (hwnd != IntPtr.Zero)
			{
				ShowWindow(hwnd, SW_MINIMIZE);
			}
		}
#endif
	}
	private void OnPointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
	{
		theBorder.BackgroundColor = Colors.Lime;
		enterCount++;
		pointerEnterCountLabel.Text = $"Pointer Enter Count: {enterCount}";
	}
	private void OnPointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
	{
		theBorder.BackgroundColor = Colors.Red;
		exitCount++;
		pointerExitCountLabel.Text = $"Pointer Exit Count: {exitCount}";
	}
}