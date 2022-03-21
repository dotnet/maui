#nullable disable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	public class MauiSlider : Slider
	{
		public MauiSlider()
		{
		}

		public static readonly DependencyProperty ThumbImageSourceProperty =
			DependencyProperty.Register(nameof(ThumbImageSource), typeof(WImageSource),
				typeof(MauiSlider), new PropertyMetadata(null, PropertyChangedCallback));

		static void PropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var slider = (MauiSlider)dependencyObject;
			
		}

		public WImageSource ThumbImageSource
		{
			get { return (WImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}
	}
}