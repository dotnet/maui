using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class PointerGestureGalleryPage
	{
		public PointerGestureGalleryPage()
		{
			InitializeComponent();
		}

		void HoverBegan(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Thanks for hovering me!";
		}

		void HoverEnded(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Hover me again!";
			positionLabel.Text = "Hover above label to reveal pointer position again";
		}

		void HoverMoved(object sender, PointerEventArgs e)
		{
			positionLabel.Text = $"Pointer position is at: {e.GetPosition((View)sender)}";
			positionToWindow.Text = $"Pointer position inside window: {e.GetPosition(null)}";
			positionToThisLabel.Text = $"Pointer position relative to this label: {e.GetPosition(positionToThisLabel)}";
		}
	}
}