using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	public class ImageRenderer : ViewRenderer<Image, AImageView>
	{
		bool _isDisposed;

		IElementController ElementController => Element as IElementController;

		public ImageRenderer()
		{
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var view = new FormsImageView(Context);
				SetNativeControl(view);
			}

			UpdateBitmap(e.OldElement);
			UpdateAspect();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				UpdateBitmap();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect();
		}

		void UpdateAspect()
		{
			AImageView.ScaleType type = Element.Aspect.ToScaleType();
			Control.SetScaleType(type);
		}

		async void UpdateBitmap(Image previous = null)
		{
			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			Bitmap bitmap = null;

			ImageSource source = Element.Source;
			IImageSourceHandler handler;

			if (previous != null && Equals(previous.Source, Element.Source))
				return;

			((IImageController)Element).SetIsLoading(true);

			var formsImageView = Control as FormsImageView;
			if (formsImageView != null)
				formsImageView.SkipInvalidate();

			Control.SetImageResource(global::Android.Resource.Color.Transparent);

			if (source != null && (handler = Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				try
				{
					bitmap = await handler.LoadImageAsync(source, Context);
				}
				catch (TaskCanceledException)
				{
				}
				catch (IOException ex)
				{
					Log.Warning("Xamarin.Forms.Platform.Android.ImageRenderer", "Error updating bitmap: {0}", ex);
				}
			}

			if (Element == null || !Equals(Element.Source, source))
				return;

			if (!_isDisposed)
			{
				Control.SetImageBitmap(bitmap);
				bitmap?.Dispose();

				((IImageController)Element).SetIsLoading(false);
				((IVisualElementController)Element).NativeSizeChanged();
			}
		}
	}
}