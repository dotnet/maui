using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class PointerGestureGalleryPage
	{
		public PointerGestureGalleryPage()
		{
			InitializeComponent();
		}

		private void HoverBegan(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Thanks for hovering me!";
		}

		private void HoverEnded(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Hover me again!";
			positionLabel.Text = "Hover above label to reveal pointer position again";
		}

		private void HoverMoved(object sender, PointerEventArgs e)
		{
			positionLabel.Text = $"Pointer position is at: {e.GetPosition((View)sender)}";
		}
	}
}