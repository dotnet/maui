using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Maui.Controls.Compatibility.Platform.WPF.Extensions;
using WThickness = System.Windows.Thickness;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
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
				UpdatePadding();
				UpdateShadow();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ContentView.ContentProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateShadow();
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();
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
				Control.UpdateDependencyColor(Border.BorderBrushProperty, Color.Transparent);
				Control.BorderThickness = new System.Windows.Thickness(0);
			}
		}

		protected virtual void UpdateShadow()
		{
			if (Element.HasShadow)
			{
				Control.Effect = new DropShadowEffect()
				{
					Color = Colors.Gray,
					Direction = 320,
					Opacity = 0.5,
					BlurRadius = 6,
					ShadowDepth = 2
				};
			}
			else if (Control.Effect is DropShadowEffect)
			{
				Control.Effect = null;
			}
		}

		protected override void UpdateBackground()
		{
			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
				Control.UpdateDependencyColor(Border.BackgroundProperty, Element.BackgroundColor);
			else
				Control.Background = background.ToBrush();
		}

		void UpdateCornerRadius()
		{
			Control.CornerRadius = new System.Windows.CornerRadius(Element.CornerRadius >= 0 ? Element.CornerRadius : 0);
			_rounding.CornerRadius = Control.CornerRadius;
		}

		void UpdatePadding()
		{
			Control.Padding = new WThickness(
				Element.Padding.Left,
				Element.Padding.Top,
				Element.Padding.Right,
				Element.Padding.Bottom);
		}
	}
}
