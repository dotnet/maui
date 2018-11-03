using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{

	internal static class ImageElementManager
	{
		public static SizeRequest Measure(
			IImageController ImageElementManager,
			SizeRequest desiredSize,
			double widthConstraint,
			double heightConstraint)
		{
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
				switch (ImageElementManager.Aspect)
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
				switch (ImageElementManager.Aspect)
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

		internal static void OnBindingContextChanged(IImageController image, VisualElement visualElement)
		{
			if (image.Source != null)
				BindableObject.SetInheritedBindingContext(image.Source, visualElement?.BindingContext);
		}


		public static async void ImageSourceChanging(ImageSource oldImageSource)
		{
			if (oldImageSource == null) return;
			try
			{
				await oldImageSource.Cancel().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				// Workaround bugzilla 37792 https://bugzilla.xamarin.com/show_bug.cgi?id=37792
			}
		}

		public static void ImageSourceChanged(BindableObject bindable, ImageSource newSource)
		{
			var visualElement = (VisualElement)bindable;
			if (newSource != null)
				BindableObject.SetInheritedBindingContext(newSource, visualElement?.BindingContext);

			visualElement?.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public static void ImageSourcesSourceChanged(object sender, EventArgs e)
		{
			((IImageController)sender).RaiseImageSourcePropertyChanged();
			((VisualElement)sender).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}
	}
}