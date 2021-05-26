using System;
using System.ComponentModel;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class BorderElementManager
	{
		static nfloat _defaultCornerRadius = 5;

		public static void Init(IVisualNativeElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		public static void Dispose(IVisualNativeElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			renderer.ControlChanged -= OnControlChanged;
		}

		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IVisualNativeElementRenderer renderer = (IVisualNativeElementRenderer)sender;
			IBorderElement backgroundView = (IBorderElement)renderer.Element;

			if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.CornerRadiusProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder(renderer, backgroundView);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				UpdateBorder((IVisualNativeElementRenderer)sender, (IBorderElement)e.NewElement);
			}
		}

		public static void UpdateBorder(IVisualNativeElementRenderer renderer, IBorderElement backgroundView)
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
			IVisualNativeElementRenderer renderer = (IVisualNativeElementRenderer)sender;
			IBorderElement backgroundView = (IBorderElement)renderer.Element;
			UpdateBorder(renderer, backgroundView);
		}
	}
}