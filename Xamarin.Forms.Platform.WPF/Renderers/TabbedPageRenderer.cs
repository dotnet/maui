using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfLightToolkit.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class TabbedPageRenderer : VisualMultiPageRenderer<TabbedPage, Page, LightTabbedPage>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new LightTabbedPage() { ContentLoader = new FormsContentLoader() });
				}

				UpdateBarBackgroundColor();
				UpdateBarTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			
			if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
		}

		void UpdateBarBackgroundColor()
		{
			Control.UpdateDependencyColor(LightTabbedPage.BarBackgroundColorProperty, Element.BarBackgroundColor);
		}

		void UpdateBarTextColor()
		{
			Control.UpdateDependencyColor(LightTabbedPage.BarTextColorProperty, Element.BarTextColor);
		}
	}
}
