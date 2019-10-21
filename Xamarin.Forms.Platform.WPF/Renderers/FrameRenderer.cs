using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	public class FrameRenderer : ViewRenderer<Frame, Border>
	{
		VisualElement _currentView;
		readonly Border _rounding;
		readonly VisualBrush _mask;

		public FrameRenderer()
		{
			_rounding = new Border();
			_rounding.Background = Color.White.ToBrush();
			_rounding.SnapsToDevicePixels = true;
			var wb = new System.Windows.Data.Binding(nameof(Border.ActualWidth));
			wb.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
			{
				AncestorType = typeof(Border)
			};
			_rounding.SetBinding(Border.WidthProperty, wb);
			var hb = new System.Windows.Data.Binding(nameof(Border.ActualHeight));
			hb.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
			{
				AncestorType = typeof(Border)
			};
			_rounding.SetBinding(Border.HeightProperty, hb);
			_mask = new VisualBrush(_rounding);
		}

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
			Control.OpacityMask = _mask;
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
			_rounding.CornerRadius = Control.CornerRadius;
		}
	}
}

