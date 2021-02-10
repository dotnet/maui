using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls.GalleryPages
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
