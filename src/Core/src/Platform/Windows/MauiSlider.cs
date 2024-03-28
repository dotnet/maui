#nullable disable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	internal class MauiSlider : Slider
	{
		Thumb _thumb;
		Style _originalThumbStyle;

		static Style ImageThumbStyle => (Style)Application.Current.Resources["MauiSliderImageThumbStyle"];

		public static readonly DependencyProperty ThumbImageSourceProperty =
			DependencyProperty.Register(nameof(ThumbImageSource), typeof(WImageSource),
				typeof(MauiSlider), new PropertyMetadata(null, ThumbImageSourceChanged));

		static void ThumbImageSourceChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var slider = (MauiSlider)dependencyObject;
			slider.UpdateThumbStyle();
		}

		void UpdateThumbStyle()
		{
			if (_thumb == null)
			{
				return;
			}

			WImageSource imageSource = MauiSlider.ThumbImageSource;
			if (imageSource != null)
			{
				_thumb.Style = ImageThumbStyle;
				_thumb.Tag = imageSource;
			}
			else
			{
				_thumb.Style = _originalThumbStyle;
				_thumb.Tag = null;
			}
		}

		public static WImageSource ThumbImageSource
		{
			get { return (WImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_thumb = (Thumb)GetTemplateChild("HorizontalThumb");
			_originalThumbStyle = _thumb.Style;

			UpdateThumbStyle();
		}
	}
}