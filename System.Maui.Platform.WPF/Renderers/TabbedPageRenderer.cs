using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms.Platform.WPF.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class TabbedPageRenderer : VisualMultiPageRenderer<TabbedPage, Page, FormsTabbedPage>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsTabbedPage() { ContentLoader = new FormsContentLoader() });
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
			Control.UpdateDependencyColor(FormsTabbedPage.BarBackgroundColorProperty, Element.BarBackgroundColor);
		}

		void UpdateBarTextColor()
		{
			Control.UpdateDependencyColor(FormsTabbedPage.BarTextColorProperty, Element.BarTextColor);
		}
	}
}
