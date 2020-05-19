using System;
using System.ComponentModel;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Automation.Peers;
using global::Windows.UI.Xaml.Shapes;

namespace System.Maui.Platform.UWP
{
	[Obsolete("BoxViewRenderer is obsolete as of version 4.6.0. Please use BoxViewBorderRenderer instead.", false)]
	public class BoxViewRenderer : ViewRenderer<BoxView, global::Windows.UI.Xaml.Shapes.Rectangle>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var rect = new global::Windows.UI.Xaml.Shapes.Rectangle
					{
						DataContext = Element
					};

					rect.SetBinding(Shape.FillProperty, new global::Windows.UI.Xaml.Data.Binding { Converter = new ColorConverter(), Path = new PropertyPath("Color") });

					SetNativeControl(rect);
				}

				SetCornerRadius(Element.CornerRadius);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				SetCornerRadius(Element.CornerRadius);
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			// We need an automation peer so we can interact with this in automated tests
			if (Control == null)
			{
				return new FrameworkElementAutomationPeer(this);
			}

			return new FrameworkElementAutomationPeer(Control);
		}

		private void SetCornerRadius(CornerRadius cornerRadius)
		{
			Control.RadiusX = cornerRadius.TopLeft;
			Control.RadiusY = cornerRadius.BottomRight;
		}
	}
}
