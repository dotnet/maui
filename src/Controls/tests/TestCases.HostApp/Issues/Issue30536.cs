#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30536, "[Windows] PointerGestureRecognizer behaves incorrectly when multiple windows are open", PlatformAffected.UWP)]
public class Issue30536 : ContentPage
{
	Button newWindowButton;
	Button closeNewWindowButton;
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
		stackLayout.Children.Add(newWindowButton);
		stackLayout.Children.Add(closeNewWindowButton);
		stackLayout.Children.Add(pointerEnterCountLabel);
		stackLayout.Children.Add(pointerExitCountLabel);
		stackLayout.Children.Add(theBorder);

		// Set the content
		Content = stackLayout;
	}

	private async void OnNewWindowButton_Clicked(object sender, EventArgs e)
	{
		secondWindow = new Window(new ContentPage());
		Application.Current.OpenWindow(secondWindow);
		await Task.Delay(500); // Allow the new window to open
		MinimizeWindow(secondWindow);
	}

#if WINDOWS
	// Windows API declarations
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
 
	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();
 
	// ShowWindow constants
	private const int SW_MINIMIZE = 6;
#endif

	private void MinimizeWindow(Window window)
	{
#if WINDOWS
		// Get the current window handle
		IntPtr hWnd = GetActiveWindow();
		if (hWnd != IntPtr.Zero)
		{
			// Minimize the window
			ShowWindow(hWnd, SW_MINIMIZE);
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