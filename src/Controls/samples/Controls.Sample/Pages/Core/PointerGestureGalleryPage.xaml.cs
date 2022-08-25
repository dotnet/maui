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
			hoverLabel.Text = "Thanks for hovering me! You can hover off now";
		}

		private void HoverEnded(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Hover me again!";
		}
	}
}