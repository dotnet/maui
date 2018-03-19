using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Forms.Platform.UWP
{
	public class FormsSlider : Windows.UI.Xaml.Controls.Slider
	{
		internal Thumb Thumb { get; set; }
		internal Thumb ImageThumb { get; set; }

		public static readonly DependencyProperty ThumbImageProperty = 
			DependencyProperty.Register(nameof(ThumbImage), typeof(BitmapImage), 
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

			if (slider.ThumbImage != null)
			{
				slider.Thumb.Visibility = Visibility.Collapsed;
				slider.ImageThumb.Visibility = Visibility.Visible;
			}
			else
			{
				slider.Thumb.Visibility = Visibility.Visible;
				slider.ImageThumb.Visibility = Visibility.Collapsed;
			}
		}

		public BitmapImage ThumbImage
		{
			get { return (BitmapImage)GetValue(ThumbImageProperty); }
			set { SetValue(ThumbImageProperty, value); }
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