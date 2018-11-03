using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageRenderer))]
	public class Image : View, IImageController, IElementConfiguration<Image>, IViewController
	{
		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(Image), default(ImageSource),
			propertyChanging: OnImageSourceChanging, propertyChanged: OnImageSourceChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(Image), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create(nameof(IsOpaque), typeof(bool), typeof(Image), false);

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(Image), default(bool));

		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

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

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			ImageElementManager.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElementManager.Measure(this, desiredSize, widthConstraint, heightConstraint);
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

		BindableProperty IImageController.SourceProperty => SourceProperty;
		BindableProperty IImageController.AspectProperty => AspectProperty;
		BindableProperty IImageController.IsOpaqueProperty => IsOpaqueProperty;

		void OnImageSourcesSourceChanged(object sender, EventArgs e) =>
			ImageElementManager.ImageSourcesSourceChanged(this, EventArgs.Empty);

		static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource newSource = (ImageSource)newValue;
			Image image = (Image)bindable;
			if (newSource != null)
			{
				newSource.SourceChanged += image.OnImageSourcesSourceChanged;
			}
			ImageElementManager.ImageSourceChanged(bindable, newSource);
		}

		static void OnImageSourceChanging(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource oldSource = (ImageSource)oldValue;
			Image image = (Image)bindable;

			if (oldSource != null)
			{
				oldSource.SourceChanged -= image.OnImageSourcesSourceChanged;
			}
			ImageElementManager.ImageSourceChanging(oldSource);
		}

		void IImageController.RaiseImageSourcePropertyChanged() => OnPropertyChanged(nameof(Source));
	}
}