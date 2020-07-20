using Gdk;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.GTK.Extensions;
using DrawingFont = System.Drawing.Font;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class ImageRenderer : ViewRenderer<Image, Controls.ImageControl>
	{
		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.Dispose();
					Control = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (Control == null)
			{
				var image = new Controls.ImageControl();
				SetNativeControl(image);
			}

			if (e.NewElement != null)
			{
				SetImage(e.OldElement);
				SetAspect();
				SetOpacity();
				SetScaleX();
				SetScaleY();
				SetRotation();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				SetImage();
			else if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
				SetOpacity();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect();
			else if (e.PropertyName == Image.ScaleProperty.PropertyName)
				SetScale();
			else if (e.PropertyName == Image.ScaleXProperty.PropertyName)
				SetScaleX();
			else if (e.PropertyName == Image.ScaleYProperty.PropertyName)
				SetScaleY();
			else if (e.PropertyName == Image.RotationProperty.PropertyName)
				SetRotation();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			Control.SetSizeRequest(allocation.Width, allocation.Height);
		}

		async void SetImage(Image oldElement = null)
		{
			var source = Element.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource && source is FileImageSource
					&& ((FileImageSource)oldSource).File == ((FileImageSource)source).File)
					return;

				Control.Pixbuf = null;
			}

			((IImageController)Element).SetIsLoading(true);

			var image = await source.GetNativeImageAsync();

			var imageView = Control;
			if (imageView != null)
				imageView.Pixbuf = image;

			if (!_isDisposed)
			{
				((IVisualElementController)Element).NativeSizeChanged();
				((IImageController)Element).SetIsLoading(false);
			}
		}

		void SetAspect()
		{
			switch (Element.Aspect)
			{
				case Aspect.AspectFit:
					Control.Aspect = Controls.ImageAspect.AspectFit;
					break;
				case Aspect.AspectFill:
					Control.Aspect = Controls.ImageAspect.AspectFill;
					break;
				case Aspect.Fill:
					Control.Aspect = Controls.ImageAspect.Fill;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Element.Aspect));
			}
		}

		void SetOpacity()
		{
			var opacity = Element.Opacity;

			Control.SetAlpha(opacity);
		}

		void SetScale()
		{
			Control.Scale = Element.Scale;
		}

		void SetScaleX()
		{
			Control.ScaleX = Element.ScaleX;
		}

		void SetScaleY()
		{
			Control.ScaleY = Element.ScaleY;
		}

		void SetRotation()
		{
			Control.Rotation = Element.Rotation;
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken =
			default(CancellationToken), float scale = 1);
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<Pixbuf> LoadImageAsync(
			ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken),
			float scale = 1f)
		{
			Pixbuf image = null;
			var filesource = imagesource as FileImageSource;

			if (filesource != null)
			{
				var file = filesource.File;
				if (!string.IsNullOrEmpty(file))
				{
					var imagePath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

					if (File.Exists(imagePath))
					{
						image = new Pixbuf(imagePath);
					}
				}
			}

			return Task.FromResult(image);
		}
	}

	public sealed class StreamImagesourceHandler : IImageSourceHandler
	{
		public async Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
		{
			Pixbuf image = null;

			var streamsource = imagesource as StreamImageSource;
			if (streamsource?.Stream == null) return null;
			using (var streamImage = await ((IStreamImageSource)streamsource)
				.GetStreamAsync(cancelationToken).ConfigureAwait(false))
			{
				if (streamImage != null)
					image = new Pixbuf(streamImage);
			}

			return image;
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public async Task<Pixbuf> LoadImageAsync(
			ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken),
			float scale = 1)
		{
			Pixbuf image = null;

			var imageLoader = imagesource as UriImageSource;

			if (imageLoader?.Uri == null)
				return null;

			using (Stream streamImage = await imageLoader.GetStreamAsync(cancelationToken))
			{
				if (streamImage == null || !streamImage.CanRead)
				{
					return null;
				}

				image = new Pixbuf(streamImage);
			}

			return image;
		}
	}


	public sealed class FontImageSourceHandler : IImageSourceHandler
	{
		public Task<Pixbuf> LoadImageAsync(ImageSource imageSource,
			CancellationToken cancellationToken = new CancellationToken(), float scale = 1)
		{
			if (!(imageSource is FontImageSource fontImageSource))
				return null;

			Pixbuf pixbuf;
			using (var bmp = new Bitmap((int)fontImageSource.Size, (int)fontImageSource.Size))
			{
				using (var g = Graphics.FromImage(bmp))
				{
					var fontFamily = GetFontFamily(fontImageSource);
					var font = new DrawingFont(fontFamily, (int)fontImageSource.Size * .5f);
					var fontColor = fontImageSource.Color != Color.Default
						? fontImageSource.Color
						: Color.White;
					g.DrawString(fontImageSource.Glyph, font, new SolidBrush(fontColor), 0, 0);
				}

				using (var stream = new MemoryStream())
				{
					bmp.Save(stream, ImageFormat.Jpeg);
					pixbuf = new Pixbuf(stream.ToArray());
				}
			}

			return Task.FromResult(pixbuf);
		}

		static FontFamily GetFontFamily(FontImageSource fontImageSource)
		{
			var privateFontCollection = new PrivateFontCollection();
			FontFamily fontFamily;
			if (fontImageSource.FontFamily.Contains("#"))
			{
				var fontPathAndFamily = fontImageSource.FontFamily.Split('#');
				privateFontCollection.AddFontFile(fontPathAndFamily[0]);
				fontFamily = fontPathAndFamily.Length > 1 ?
					privateFontCollection.Families.FirstOrDefault(f => f.Name.Equals(fontPathAndFamily[1], StringComparison.InvariantCultureIgnoreCase)) ?? privateFontCollection.Families[0] : 
					privateFontCollection.Families[0];
			}
			else
			{
				privateFontCollection.AddFontFile(fontImageSource.FontFamily);
				fontFamily = privateFontCollection.Families[0];
			}

			return fontFamily;
		}
	}
}

