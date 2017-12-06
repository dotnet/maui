using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfLightToolkit.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class PageRenderer : VisualPageRenderer<Page, LightContentPage>
	{
		VisualElement _currentView;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new LightContentPage());
				}

				// Update control property 
				UpdateContent();
			}

			base.OnElementChanged(e);
		}
		
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if(e.PropertyName == ContentPage.ContentProperty.PropertyName)
				UpdateContent();
		}

		void UpdateContent()
		{
			ContentPage page = Element as ContentPage;
			if (page != null)
			{
				if (_currentView != null)
				{
					_currentView.Cleanup(); // cleanup old view
				}

				_currentView = page.Content;
				Control.Content = _currentView != null ? Platform.GetOrCreateRenderer(_currentView).GetNativeElement() : null;
			}
		}
	}
}
