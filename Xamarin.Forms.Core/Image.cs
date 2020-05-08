using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageRenderer))]
	public class Image : View, IImageController, IElementConfiguration<Image>, IViewController, IImageElement
	{
		public static readonly BindableProperty SourceProperty = ImageElement.SourceProperty;

		public static readonly BindableProperty ErrorPlaceholderProperty =
			BindableProperty.Create(nameof(ErrorPlaceholder), typeof(ImageSource), typeof(Image), default(ImageSource), propertyChanged: (b, o, n) =>
			{
				if (!(n is FileImageSource) && n != null)
					throw new InvalidOperationException($"{nameof(ErrorPlaceholder)} needs to be a local resource.");

			});

		public ImageSource ErrorPlaceholder
		{
			get => (ImageSource)GetValue(ErrorPlaceholderProperty);
			set => SetValue(ErrorPlaceholderProperty, value);
		}

		public static readonly BindableProperty LoadingPlaceholderProperty =
			BindableProperty.Create(nameof(LoadingPlaceholder), typeof(ImageSource), typeof(Image), default(ImageSource), propertyChanged: (b, o, n) =>
			{
				if (!(n is FileImageSource) && n != null)
					throw new InvalidOperationException($"{nameof(LoadingPlaceholder)} needs to be a local resource.");

			});

		public ImageSource LoadingPlaceholder
		{
			get => (ImageSource)GetValue(LoadingPlaceholderProperty);
			set => SetValue(LoadingPlaceholderProperty, value);
		}

		public static readonly BindableProperty AspectProperty = ImageElement.AspectProperty;

		public static readonly BindableProperty IsOpaqueProperty = ImageElement.IsOpaqueProperty;

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(Image), default(bool));

		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		public static readonly BindableProperty IsAnimationPlayingProperty = ImageElement.IsAnimationPlayingProperty;

		readonly Lazy<PlatformConfigurationRegistry<Image>> _platformConfigurationRegistry;

		public Image()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Image>>(() => new PlatformConfigurationRegistry<Image>(this));
		}

		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		public bool IsLoading
		{
			get { return (bool)GetValue(IsLoadingProperty); }
		}

		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}

		public bool IsAnimationPlaying
		{
			get { return (bool)GetValue(IsAnimationPlayingProperty); }
			set { SetValue(IsAnimationPlayingProperty, value); }
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		bool IImageController.GetLoadAsAnimation() => ImageElement.GetLoadAsAnimation(this);

		protected override void OnBindingContextChanged()
		{
			ImageElement.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElement.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading)
		{
			SetValue(IsLoadingPropertyKey, isLoading);
		}

		public IPlatformElementConfiguration<T, Image> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void IImageElement.OnImageSourceSourceChanged(object sender, EventArgs e) =>
			ImageElement.ImageSourceSourceChanged(this, e);

		void IImageElement.RaiseImageSourcePropertyChanged() => OnPropertyChanged(nameof(Source));
	}
}