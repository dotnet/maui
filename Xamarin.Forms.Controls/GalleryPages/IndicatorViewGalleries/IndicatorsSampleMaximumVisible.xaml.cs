using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public partial class IndicatorsSampleMaximumVisible : ContentPage
	{
		int maxVisible = 3;

		public IndicatorsSampleMaximumVisible()
		{
			Device.SetFlags(new[] { ExperimentalFlags.CarouselViewExperimental, ExperimentalFlags.IndicatorViewExperimental });
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);
		}

		public void MaximumVisibleClicked(object sender, EventArgs e)
		{
			indicators.MaximumVisible = indicatorsForms.MaximumVisible = maxVisible;
			maxVisible--;
		}
	}
}
