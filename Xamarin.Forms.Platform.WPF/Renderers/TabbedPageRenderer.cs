using System.ComponentModel;
using Xamarin.Forms.Platform.WPF.Controls;
using Xamarin.Forms.Platform.WPF.Extensions;

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
				UpdateBarBackground();
				UpdateBarTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			
			if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarBackgroundProperty.PropertyName)
				UpdateBarBackground();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
		}

		void UpdateBarBackgroundColor()
		{
			Control.UpdateDependencyColor(FormsTabbedPage.BarBackgroundColorProperty, Element.BarBackgroundColor);
		}

		void UpdateBarBackground()
		{
			Control.BarBackgroundColor = Element.BarBackground.ToBrush();
		}

		void UpdateBarTextColor()
		{
			Control.UpdateDependencyColor(FormsTabbedPage.BarTextColorProperty, Element.BarTextColor);
		}
	}
}
