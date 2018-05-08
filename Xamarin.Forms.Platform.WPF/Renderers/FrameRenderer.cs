using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WPF
{
	public class FrameRenderer : ViewRenderer<Frame, Border>
	{
		VisualElement _currentView;

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new Border());
				}

				// Update control property 
				UpdateContent();
				UpdateBorder();
				UpdateCornerRadius();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Frame.ContentProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName || e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
		}

		protected override void UpdateBackground()
		{
			Control.UpdateDependencyColor(Border.BackgroundProperty, Element.BackgroundColor);
		}

		void UpdateContent()
		{
			if (_currentView != null)
			{
				_currentView.Cleanup(); // cleanup old view
			}

			_currentView = Element.Content;
			Control.Child = _currentView != null ? Platform.GetOrCreateRenderer(_currentView).GetNativeElement() : null;
		}

		void UpdateBorder()
		{
			if (Element.BorderColor != Color.Default)
			{
				Control.UpdateDependencyColor(Border.BorderBrushProperty, Element.BorderColor);
				Control.BorderThickness = new System.Windows.Thickness(1);
			}
			else
			{
				Control.UpdateDependencyColor(Border.BorderBrushProperty, new Color(0, 0, 0, 0));
				Control.BorderThickness = new System.Windows.Thickness(0);
			}
		}

		void UpdateCornerRadius()
		{
			Control.CornerRadius = new System.Windows.CornerRadius(Element.CornerRadius >= 0 ? Element.CornerRadius : 0);
		}
	}
}

