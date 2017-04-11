using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageRenderer))]
	public class Image : View, IImageController, IElementConfiguration<Image>
	{
		public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(ImageSource), typeof(Image), default(ImageSource), 
			propertyChanging: OnSourcePropertyChanging, propertyChanged: OnSourcePropertyChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create("Aspect", typeof(Aspect), typeof(Image), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create("IsOpaque", typeof(bool), typeof(Image), false);

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly("IsLoading", typeof(bool), typeof(Image), default(bool));

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
			if (Source != null)
				SetInheritedBindingContext(Source, BindingContext);

			base.OnBindingContextChanged();
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			double desiredAspect = desiredSize.Request.Width / desiredSize.Request.Height;
			double constraintAspect = widthConstraint / heightConstraint;

			double desiredWidth = desiredSize.Request.Width;
			double desiredHeight = desiredSize.Request.Height;

			if (desiredWidth == 0 || desiredHeight == 0)
				return new SizeRequest(new Size(0, 0));

			double width = desiredWidth;
			double height = desiredHeight;
			if (constraintAspect > desiredAspect)
			{
				// constraint area is proportionally wider than image
				switch (Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.Fill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
				}
			}
			else if (constraintAspect < desiredAspect)
			{
				// constraint area is proportionally taller than image
				switch (Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.Fill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
				}
			}
			else
			{
				// constraint area is same aspect as image
				width = Math.Min(desiredWidth, widthConstraint);
				height = desiredHeight * (width / desiredWidth);
			}

			return new SizeRequest(new Size(width, height));
		}

		void OnSourceChanged(object sender, EventArgs eventArgs)
		{
			OnPropertyChanged(SourceProperty.PropertyName);
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((Image)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue);
		}

		void OnSourcePropertyChanged(ImageSource oldvalue, ImageSource newvalue)
		{
			if (newvalue != null)
			{
				newvalue.SourceChanged += OnSourceChanged;
				SetInheritedBindingContext(newvalue, BindingContext);
			}

			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		static void OnSourcePropertyChanging(BindableObject bindable, object oldvalue, object newvalue)
		{
			((Image)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue);
		}

		async void OnSourcePropertyChanging(ImageSource oldvalue, ImageSource newvalue)
		{
			if (oldvalue == null)
				return;
			
			oldvalue.SourceChanged -= OnSourceChanged;
			try
			{
				await oldvalue.Cancel();
			}
			catch(ObjectDisposedException)
			{ 
				// Workaround bugzilla 37792 https://bugzilla.xamarin.com/show_bug.cgi?id=37792
			}
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
	}
}