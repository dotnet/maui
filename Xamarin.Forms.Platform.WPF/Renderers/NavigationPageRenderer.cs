using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfLightToolkit.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class FormsLightNavigationPage : LightNavigationPage
	{
		NavigationPage NavigationPage;

		public FormsLightNavigationPage(NavigationPage navigationPage)
		{
			ContentLoader = new FormsContentLoader();
			NavigationPage = navigationPage;
		}

		public override void OnBackButtonPressed()
		{
			NavigationPage.PopAsync();
		}
	}
	
	public class NavigationPageRenderer : VisualPageRenderer<NavigationPage, FormsLightNavigationPage>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				e.OldElement.PushRequested -= Element_PushRequested;
				e.OldElement.PopRequested -= Element_PopRequested;
				e.OldElement.PopToRootRequested -= Element_PopToRootRequested;
				e.OldElement.RemovePageRequested -= Element_RemovePageRequested;
				e.OldElement.InsertPageBeforeRequested -= Element_InsertPageBeforeRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsLightNavigationPage(Element));
				}

				// Update control property 
				UpdateBarBackgroundColor();
				UpdateBarTextColor();

				// Suscribe element event
				Element.PushRequested += Element_PushRequested;
				Element.PopRequested += Element_PopRequested;
				Element.PopToRootRequested += Element_PopToRootRequested;
				Element.RemovePageRequested += Element_RemovePageRequested;
				Element.InsertPageBeforeRequested += Element_InsertPageBeforeRequested;
			}
			
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NavigationPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == NavigationPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
		}

		protected override void Appearing()
		{
			base.Appearing();
			PushExistingNavigationStack();
		}
		
	    void Element_InsertPageBeforeRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			this.Control?.InsertPageBefore(e.Page, e.BeforePage);
		}

	    void Element_RemovePageRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			this.Control?.RemovePage(e.Page); 
		}

		void Element_PopToRootRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			this.Control?.PopToRoot(e.Animated);
		}

		void Element_PopRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			this.Control?.Pop(e.Animated);
		}

		void Element_PushRequested(object sender, Internals.NavigationRequestedEventArgs e)
		{
			this.Control?.Push(e.Page, e.Animated);
		}
		
		void PushExistingNavigationStack()
		{
			foreach (var page in Element.Pages)
			{
				Control.InternalChildren.Add(page);
			}
			this.Control.CurrentPage = this.Control.InternalChildren.Last();
		}
		
		void UpdateBarBackgroundColor()
		{
			Control.UpdateDependencyColor(LightNavigationPage.TitleBarBackgroundColorProperty, Element.BarBackgroundColor);
		}

		void UpdateBarTextColor()
		{
			Control.UpdateDependencyColor(LightNavigationPage.TitleBarTextColorProperty, Element.BarTextColor);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Element != null)
				{
					Element.PushRequested -= Element_PushRequested;
					Element.PopRequested -= Element_PopRequested;
					Element.PopToRootRequested -= Element_PopToRootRequested;
					Element.RemovePageRequested -= Element_RemovePageRequested;
					Element.InsertPageBeforeRequested -= Element_InsertPageBeforeRequested;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
