using Gdk;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class ImageRenderer : ViewRenderer<Image, Controls.ImageControl>
    {
        private bool _isDisposed;

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
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            Control.SetSizeRequest(allocation.Width, allocation.Height);
        }

        private async void SetImage(Image oldElement = null)
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

            IImageSourceHandler handler;

            ((IImageController)Element).SetIsLoading(true);

            if (source != null
                && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
            {
                Pixbuf image;

                try
                {
                    image = await handler.LoadImageAsync(source);
                }
                catch (OperationCanceledException)
                {
                    image = null;
                    Internals.Log.Warning("Image loading", "Image load cancelled");
                }
                catch(Exception ex)
                {
                    image = null;
                    Internals.Log.Warning("Image loading", $"Image load failed: {ex}");
                }

                var imageView = Control;
                if (imageView != null)
                    imageView.Pixbuf = image;

                if (!_isDisposed)
                    ((IVisualElementController)Element).NativeSizeChanged();
            }
            else
                Control.Pixbuf = null;

            if (!_isDisposed)
                ((IImageController)Element).SetIsLoading(false);
        }

        private void SetAspect()
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

        private void SetOpacity()
        {
            var opacity = Element.Opacity;

            Control.SetAlpha(opacity);
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
                    var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

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
            using (
                var streamImage = await((IStreamImageSource)streamsource)
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
}