using System;
using System.ComponentModel;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls.Primitives;
using global::Windows.UI.Xaml.Media.Imaging;
using WImageSource = global::Windows.UI.Xaml.Media.ImageSource;

namespace System.Maui.Platform.UWP
{
	public class FormsSlider : global::Windows.UI.Xaml.Controls.Slider
	{
		internal Thumb Thumb { get; set; }
		internal Thumb ImageThumb { get; set; }
			   
		public static readonly DependencyProperty ThumbImageSourceProperty = 
			DependencyProperty.Register(nameof(ThumbImageSource), typeof(WImageSource), 
			typeof(FormsSlider), new PropertyMetadata(null, PropertyChangedCallback));

		[Obsolete("ThumbImageProperty is obsolete as of 4.0.0. Please use ThumbImageSourceProperty instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty ThumbImageProperty = ThumbImageSourceProperty;

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
				slider.Thumb.Visibility = Visibility.Collapsed;
				slider.ImageThumb.Visibility = Visibility.Visible;
			}
			else
			{
				slider.Thumb.Visibility = Visibility.Visible;
				slider.ImageThumb.Visibility = Visibility.Collapsed;
			}
		}

		public WImageSource ThumbImageSource
		{
			get { return (WImageSource)GetValue(ThumbImageSourceProperty); }
			set { SetValue(ThumbImageSourceProperty, value); }
		}

		[Obsolete("ThumbImage is obsolete as of 4.0.0. Please use ThumbImageSource instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public BitmapImage ThumbImage
		{
			get { return GetValue(ThumbImageSourceProperty) as BitmapImage; }
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