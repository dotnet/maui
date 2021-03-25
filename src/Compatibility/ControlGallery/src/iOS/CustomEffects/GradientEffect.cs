using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportEffect(typeof(GradientEffect), Issue6334.EffectName)]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class GradientEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			InsertGradient();
		}

		CAGradientLayer layer;
		void InsertGradient()
		{
			System.Diagnostics.Debug.WriteLine("InsertGradient : " + Container.Bounds);
			var page = Element as ContentPage;
			var childLabel = page?.Content as Label;
			if (childLabel != null && Container.Bounds.Width == 0)
			{
				return;
			}

			if (layer == null)
			{

				childLabel.Text = Issue6334.Success;

				var eColor = page.BackgroundColor.ToCGColor();
				var sColor = page.BackgroundColor.AddLuminosity(0.5).ToCGColor();
				layer = new CAGradientLayer
				{
					Frame = Container.Bounds,
					Colors = new CGColor[] { sColor, eColor }
				};
				Container.Layer.InsertSublayer(layer, 0);
			}
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(args);
			if (args.PropertyName == Page.WidthProperty.PropertyName ||
				args.PropertyName == Page.HeightProperty.PropertyName)
			{
				layer.Frame = Container.Bounds;
				// or (Element as VisualElement).Bounds.ToRectangleF();
			}
		}

		protected override void OnDetached()
		{
		}
	}
}
