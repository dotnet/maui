using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class BorderElementManager
	{
		static nfloat _defaultCornerRadius = 5;

		public static void Init(IVisualPlatformElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		public static void Dispose(IVisualPlatformElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			renderer.ControlChanged -= OnControlChanged;
		}

		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IVisualPlatformElementRenderer renderer = (IVisualPlatformElementRenderer)sender;
			IBorderElement backgroundView = (IBorderElement)renderer.Element;

			if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.CornerRadiusProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder(renderer, backgroundView);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				UpdateBorder((IVisualPlatformElementRenderer)sender, (IBorderElement)e.NewElement);
			}
		}

		[PortHandler]
		public static void UpdateBorder(IVisualPlatformElementRenderer renderer, IBorderElement backgroundView)
		{
			var control = renderer.Control;
			var ImageButton = backgroundView;

			if (control == null)
			{
				return;
			}

			if (ImageButton.BorderColor != null)
				control.Layer.BorderColor = ImageButton.BorderColor.ToCGColor();

			control.Layer.BorderWidth = Math.Max(0f, (float)ImageButton.BorderWidth);

			nfloat cornerRadius = _defaultCornerRadius;

			if (ImageButton.IsCornerRadiusSet() && ImageButton.CornerRadius != ImageButton.CornerRadiusDefaultValue)
				cornerRadius = ImageButton.CornerRadius;

			control.Layer.CornerRadius = cornerRadius;
		}

		static void OnControlChanged(object sender, EventArgs e)
		{
			IVisualPlatformElementRenderer renderer = (IVisualPlatformElementRenderer)sender;
			IBorderElement backgroundView = (IBorderElement)renderer.Element;
			UpdateBorder(renderer, backgroundView);
		}
	}
}