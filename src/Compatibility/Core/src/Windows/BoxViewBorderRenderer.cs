using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WShape = Microsoft.UI.Xaml.Shapes.Shape;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class BoxViewBorderRenderer : ViewRenderer<BoxView, WBorder>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var rect = new WBorder
					{
						DataContext = Element
					};

					SetNativeControl(rect);
				}

				SetColor(Element.Color);
				SetCornerRadius(Element.CornerRadius);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetColor(Element.Color);
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				SetCornerRadius(Element.CornerRadius);
			else if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				UpdateBackgroundColor();

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

		protected override void UpdateBackgroundColor()
		{
			// BackgroundColor change must be handled separately	
			// because the background would protrude through the border if the corners are rounded
			// as the background would be applied to the renderer's FrameworkElement
			if (Control == null)
				return;
			Color backgroundColor = Element.Color;
			if (backgroundColor.IsDefault())
			{
				backgroundColor = Element.BackgroundColor;
			}

			Control.Background = backgroundColor.IsDefault() ? null : backgroundColor.ToPlatform();
		}

		protected override void UpdateBackground()
		{
			if (Control == null)
				return;

			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
			{
				Color backgroundColor = Element.BackgroundColor;

				if (!backgroundColor.IsDefault())
					Control.Background = backgroundColor.ToPlatform();
				else
				{
					if (Element.Color.IsDefault())
						Control.Background = null;
				}
			}
			else
				Control.Background = background.ToBrush();
		}

		void SetColor(Color color)
		{
			if (color.IsDefault())
				UpdateBackground();
			else
				Control.Background = color.ToPlatform();
		}

		void SetCornerRadius(CornerRadius cornerRadius)
		{
			Control.CornerRadius = WinUIHelpers.CreateCornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
		}

		static Size DefaultSize = new(40, 40);

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			// Creating a custom override for measuring the BoxView on Windows; this reports the same default size that's 
			// specified in the old OnMeasure method. Normally we'd just do this centrally in the xplat code or override
			// GetDesiredSize in a BoxViewHandler. But BoxView is a legacy control (replaced by Shapes), so we don't want
			// to bring that into the new stuff. 

			if (Element != null)
			{
				var heightRequest = Element.HeightRequest;
				var widthRequest = Element.WidthRequest;

				heightRequest = heightRequest >= 0 ? heightRequest : 40;
				widthRequest = widthRequest >= 0 ? widthRequest : 40;

				return new Size(widthRequest, heightRequest);
			}

			return DefaultSize;
		}
	}
}