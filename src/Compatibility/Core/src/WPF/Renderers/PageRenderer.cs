using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Platform.WPF.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public class PageRenderer : VisualPageRenderer<Page, FormsContentPage>
	{
		VisualElement _currentView;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsContentPage());
				}

				// Update control property 
				UpdateContent();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ContentPage.ContentProperty.PropertyName)
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
