using System;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	public partial class SwipeItemPositionGallery : ContentPage
	{
		public SwipeItemPositionGallery()
		{
			InitializeComponent();

			ModePicker.SelectedIndex = 0;
		}

		void OnModePickerSelectedIndexChanged(object sender, EventArgs e)
		{
			LeftSwipeItems.Mode = TopSwipeItems.Mode = RightSwipeItems.Mode = BottomSwipeItems.Mode = ModePicker.SelectedIndex == 0 ? SwipeMode.Reveal : SwipeMode.Execute;
		}
	}
}