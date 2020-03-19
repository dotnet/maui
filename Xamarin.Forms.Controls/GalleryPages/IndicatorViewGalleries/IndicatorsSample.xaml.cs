using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IndicatorsSample : ContentPage
	{
		public IndicatorsSample()
		{
			Device.SetFlags(new[] { ExperimentalFlags.CarouselViewExperimental, ExperimentalFlags.IndicatorViewExperimental });
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);
		}
	}
}