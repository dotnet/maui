using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class ImageRenderer : ViewRenderer<Image, Microsoft.UI.Xaml.Controls.Image>, IImageVisualElementRenderer
	{
		bool _measured;
		bool _disposed;

		public ImageRenderer() : base()
		{
			ImageElementManager.Init(this);
			// TODO WINUI
			//Microsoft.UI.Xaml.Application.Current.Resuming += OnResumingAsync;
		}

		bool IImageVisualElementRenderer.IsDisposed => _disposed;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Control.Source == null)
				return new SizeRequest();

			_measured = true;

			return new SizeRequest(Control.Source.GetImageSourceSize());
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				ImageElementManager.Dispose(this);
				if (Control != null)
				{
					Control.ImageOpened -= OnImageOpened;
					Control.ImageFailed -= OnImageFailed;
					// TODO WINUI
					//Microsoft.UI.Xaml.Application.Current.Resuming -= OnResumingAsync;
				}
			}

			base.Dispose(disposing);
		}

		protected override async void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var image = new Microsoft.UI.Xaml.Controls.Image();
					image.ImageOpened += OnImageOpened;
					image.ImageFailed += OnImageFailed;
					SetNativeControl(image);
				}

				await TryUpdateSource().ConfigureAwait(false);
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				await TryUpdateSource().ConfigureAwait(false);
		}

		void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
		{
			if (_measured)
			{
				ImageElementManager.RefreshImage(this);
			}

			((IImageController)Element)?.SetIsLoading(false);
		}

		protected virtual void OnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
		{
			Application.Current?.FindMauiContext()?.CreateLogger<ImageRenderer>()?.LogWarning("Image failed to load: {exceptionRoutedEventArgs.ErrorMessage}", exceptionRoutedEventArgs.ErrorMessage);
			((IImageController)Element)?.SetIsLoading(false);
		}

		protected virtual async Task TryUpdateSource()
		{
			// By default we'll just catch and log any exceptions thrown by UpdateSource so we don't bring down
			// the application; a custom renderer can override this method and handle exceptions from
			// UpdateSource differently if it wants to

			try
			{
				await UpdateSource().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<ImageRenderer>()?.LogWarning(ex, "Error loading image");
			}
			finally
			{
				((IImageController)Element)?.SetIsLoading(false);
			}
		}

		protected async Task UpdateSource()
		{
			await ImageElementManager.UpdateSource(this).ConfigureAwait(false);
		}

		async void OnResumingAsync(object sender, object e)
		{
			try
			{
				await ImageElementManager.UpdateSource(this);
			}
			catch (Exception exception)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<ImageRenderer>()?.LogWarning(exception, "ImageSource failed to update after app resume");
			}
		}

		void IImageVisualElementRenderer.SetImage(Microsoft.UI.Xaml.Media.ImageSource image)
		{
			Control.Source = image;
		}

		Microsoft.UI.Xaml.Controls.Image IImageVisualElementRenderer.GetImage() => Control;
	}
}
