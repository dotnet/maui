namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
{
	public partial class RefreshLayoutMarginGallery : ContentPage
	{
		public RefreshLayoutMarginGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}

		void OnMarginValueChanged(object sender, ValueChangedEventArgs e)
		{
			var margin = e.NewValue;
			RefreshScroll.Margin = new Thickness(margin);
		}
	}
}
