using System.Diagnostics;

namespace MauiMultiWindow;

public partial class MainPage : ContentPage
{
 // int count = 0;

  public MainPage()
  {
    InitializeComponent();
  }

  private void OnClicked(object sender, EventArgs e)
  {
    Window secondWindow = new Window(new SecondPage());
    secondWindow.Width = 300;
    secondWindow.Height = 300;
    Application.Current?.OpenWindow(secondWindow);
  }

	int i = 0;

	private void OnPointerEntered(object sender, PointerEventArgs e)
  {
    grid.BackgroundColor = Colors.Red;
		Debug.WriteLine($"OnPointerEntered: {i++}");
	}

  private void OnPointerExited(object sender, PointerEventArgs e)
  {
    grid.BackgroundColor = Colors.White;
		Debug.WriteLine($"OnPointerExited: {i--}");
	}
}