using System;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
{
	public partial class IsEnabledRefreshViewGallery : ContentPage
	{
		public IsEnabledRefreshViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}

		void IsEnabledBtnClicked(object sender, EventArgs e)
		{
			var button = (Button)sender;

			if (RefreshContainer.IsEnabled)
			{
				button.Text = "Enable RefreshView";
				RefreshContainer.IsEnabled = false;
				Title = "Is disabled";
			}
			else
			{
				button.Text = "Disable RefreshView";
				RefreshContainer.IsEnabled = true;
				Title = "Is enabled";
			}
		}
	}
}
