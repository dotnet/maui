using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public partial class IndicatorsTemplateSample : ContentPage
	{
		public IndicatorsTemplateSample()
		{
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);

		}
	}
}
