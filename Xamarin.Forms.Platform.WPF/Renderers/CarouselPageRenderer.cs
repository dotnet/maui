using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfLightToolkit.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class CarouselPageRenderer : VisualMultiPageRenderer<CarouselPage, ContentPage, LightCarouselPage>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new LightCarouselPage() { ContentLoader = new FormsContentLoader() });
				}
			}
			
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}
}
