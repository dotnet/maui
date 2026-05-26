using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class BackgroundManager
	{
		public static void Init(IVisualElementRenderer renderer)
		{
			_ = renderer ?? throw new ArgumentNullException($"{nameof(BackgroundManager)}.{nameof(Init)} {nameof(renderer)} cannot be null");

			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
		}

		public static void Dispose(IVisualElementRenderer renderer)
		{
			_ = renderer ?? throw new ArgumentNullException($"{nameof(BackgroundManager)}.{nameof(Init)} {nameof(renderer)} cannot be null");

			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;

			if (renderer.Element != null)
			{
				renderer.Element.PropertyChanged -= OnElementPropertyChanged;
			}
		}

		static void UpdateBackgroundColor(AView Control, VisualElement Element, Color color = null)
		{
			if (Element == null || Control == null)
				return;

			var finalColor = color ?? Element.BackgroundColor;
			if (finalColor == null)
				Control.SetBackground(null);
			else
				Control.SetBackgroundColor(finalColor.ToAndroid());
		}

		static void UpdateBackground(AView Control, VisualElement Element)
		{
			if (Element == null || Control == null)
				return;

			Brush background = Element.Background;

			Control.UpdateBackground(background);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				var renderer = sender as IVisualElementRenderer;
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateBackgroundColor(renderer?.View, renderer?.Element);
				UpdateBackground(renderer?.View, renderer?.Element);
			}
		}


		static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				var renderer = (sender as IVisualElementRenderer);
				UpdateBackgroundColor(renderer?.View, renderer?.Element);
			}
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
			{
				var renderer = (sender as IVisualElementRenderer);
				UpdateBackground(renderer?.View, renderer?.Element);
			}
		}
	}
}