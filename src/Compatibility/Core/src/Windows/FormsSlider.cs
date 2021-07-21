using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class FormsSlider : Microsoft.UI.Xaml.Controls.Slider
	{
		internal Thumb Thumb { get; set; }
		internal Thumb ImageThumb { get; set; }

		public static readonly DependencyProperty ThumbImageSourceProperty =
			DependencyProperty.Register(nameof(ThumbImageSource), typeof(WImageSource),
			typeof(FormsSlider), new PropertyMetadata(null, PropertyChangedCallback));

		static void PropertyChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var slider = (FormsSlider)dependencyObject;
			SwapThumbs(slider);
		}

		static void SwapThumbs(FormsSlider slider)
		{
			if (slider.Thumb == null || slider.ImageThumb == null)
			{
				return;
			}

			if (slider.ThumbImageSource != null)
			{
				slider.Thumb.Visibility = WVisibility.Collapsed;
				slider.ImageThumb.Visibility = WVisibility.Visible;
			}
			else
			{
				slider.Thumb.Visibility = WVisibility.Visible;
				slider.ImageThumb.Visibility = WVisibility.Collapsed;
			}
		}

		public WImageSource ThumbImageSource
		{
			get { return (WImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		internal event EventHandler Ready;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Thumb = GetTemplateChild("HorizontalThumb") as Thumb;
			ImageThumb = GetTemplateChild("HorizontalImageThumb") as Thumb;

			SwapThumbs(this);

			OnReady();
		}

		protected virtual void OnReady()
		{
			Ready?.Invoke(this, EventArgs.Empty);
		}
	}
}