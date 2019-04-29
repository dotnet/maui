using System.ComponentModel;
using System.Threading.Tasks;
using Android.Widget;
using AScaleType = Android.Widget.ImageView.ScaleType;
using ARect = Android.Graphics.Rect;
using System;
using Xamarin.Forms.Internals;
using AViewCompat = Android.Support.V4.View.ViewCompat;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public static class ImageElementManager
	{
		public static void Init(IVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;

			if(renderer is ILayoutChanges layoutChanges)
				layoutChanges.LayoutChange += OnLayoutChange;
		}

		static void OnLayoutChange(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
		{
			if(sender is IVisualElementRenderer renderer && renderer.View is ImageView imageView)
				AViewCompat.SetClipBounds(imageView, imageView.GetScaleType() == AScaleType.CenterCrop ? new ARect(0, 0, e.Right - e.Left, e.Bottom - e.Top) : null);
		}

		public static void Dispose(IVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			if (renderer is ILayoutChanges layoutChanges)
				layoutChanges.LayoutChange -= OnLayoutChange;

			if (renderer.View is ImageView imageView)
				imageView.SetImageDrawable(null);
		}

		async static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			var renderer = (sender as IVisualElementRenderer);
			var view = renderer.View as ImageView;
			var newImageElementManager = e.NewElement as IImageElement;
			var oldImageElementManager = e.OldElement as IImageElement;
			var rendererController = renderer as IImageRendererController;

			await TryUpdateBitmap(rendererController, view, newImageElementManager, oldImageElementManager);
			UpdateAspect(rendererController, view, newImageElementManager, oldImageElementManager);

			if (!rendererController.IsDisposed)
			{
				ElevationHelper.SetElevation(view, renderer.Element);
			}
		}

		async static void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var renderer = (sender as IVisualElementRenderer);
			var ImageElementManager = (IImageElement)renderer.Element;
			var imageController = (IImageController)renderer.Element;

			if (e.PropertyName == Image.SourceProperty.PropertyName ||
				e.PropertyName == Button.ImageSourceProperty.PropertyName)
			{
				try
				{
					await TryUpdateBitmap(renderer as IImageRendererController, (ImageView)renderer.View, (IImageElement)renderer.Element).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Log.Warning(renderer.GetType().Name, "Error loading image: {0}", ex);
				}
				finally
				{
					if(imageController != null)
						imageController?.SetIsLoading(false);
				}
			}
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
			{
				UpdateAspect(renderer as IImageRendererController, (ImageView)renderer.View, (IImageElement)renderer.Element);
			}
		}


		async static Task TryUpdateBitmap(IImageRendererController rendererController, ImageView Control, IImageElement newImage, IImageElement previous = null)
		{
			if (newImage == null || rendererController.IsDisposed)
			{
				return;
			}

			await Control.UpdateBitmap(newImage, previous).ConfigureAwait(false);
		}

		static void UpdateAspect(IImageRendererController rendererController, ImageView Control, IImageElement newImage, IImageElement previous = null)
		{
			if (newImage == null || rendererController.IsDisposed)
			{
				return;
			}

			ImageView.ScaleType type = newImage.Aspect.ToScaleType();
			Control.SetScaleType(type);
		}
	}
}