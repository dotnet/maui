namespace Maui.Controls.Sample
{
	public partial class PointerGestureRecognizerEvents : ContentView
	{
		public PointerGestureRecognizerEvents()
		{
			InitializeComponent();
		}

		void PointerGestureRecognizer_PointerEntered(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			if (e.PlatformArgs is not null)
				secondaryLabel.Text += $"Pointer Entered: {e.PlatformArgs.ToString()}\n";
		}

		bool pointerAlreadyMoved = false;

		void PointerGestureRecognizer_PointerMoved(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			if (e.PlatformArgs is not null && !pointerAlreadyMoved)
			{
				secondaryLabel.Text += $"Pointer Moved: {e.PlatformArgs.ToString()}\n";
				pointerAlreadyMoved = true;
			}
		}

		void PointerGestureRecognizer_PointerExited(System.Object sender, Microsoft.Maui.Controls.PointerEventArgs e)
		{
			if (e.PlatformArgs is not null)
				secondaryLabel.Text += $"Pointer Exited: {e.PlatformArgs.ToString()}\n";
		}
	}
}

