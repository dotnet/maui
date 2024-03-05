using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class PanGestureEventsGallery : ContentPage
	{
		public PanGestureEventsGallery()
		{
			InitializeComponent();
		}

		void OnPanGestureRecognizerUpdated(object sender, PanUpdatedEventArgs e)
		{
			InfoLabel.Text = $"StatusType: {e.StatusType}, TotalX: {e.TotalX}, TotalY: {e.TotalY}";
		}
	}
}