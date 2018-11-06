using System;
using System.ComponentModel;
using NativeView = UIKit.UIView;

namespace Xamarin.Forms.Platform.iOS
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
			IBorderController backgroundView = (IBorderController)renderer.Element;

			if (e.PropertyName == backgroundView.BorderWidthProperty.PropertyName || e.PropertyName == backgroundView.CornerRadiusProperty.PropertyName || e.PropertyName == backgroundView.BorderColorProperty.PropertyName)
				UpdateBorder(renderer, backgroundView);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				UpdateBorder((IVisualNativeElementRenderer)sender, (IBorderController)e.NewElement);
			}
		}

		public static void UpdateBorder(IVisualNativeElementRenderer renderer, IBorderController backgroundView)
		{
			var control = renderer.Control;
			var ImageButton = backgroundView;

			if (control == null)
			{
				return;
			}

			if (ImageButton.BorderColor != Color.Default)
				control.Layer.BorderColor = ImageButton.BorderColor.ToCGColor();

			control.Layer.BorderWidth = Math.Max(0f, (float)ImageButton.BorderWidth);

			nfloat cornerRadius = _defaultCornerRadius;

			if (ImageButton.IsSet(ImageButton.CornerRadiusProperty) && ImageButton.CornerRadius != (int)ImageButton.CornerRadiusProperty.DefaultValue)
				cornerRadius = ImageButton.CornerRadius;

			control.Layer.CornerRadius = cornerRadius;
		}

		static void OnControlChanged(object sender, EventArgs e)
		{
			IVisualNativeElementRenderer renderer = (IVisualNativeElementRenderer)sender;
			IBorderController backgroundView = (IBorderController)renderer.Element;
			UpdateBorder(renderer, backgroundView);
		}
	}
}