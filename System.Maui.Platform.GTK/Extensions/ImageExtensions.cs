using Gdk;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Maui.Internals;
using System.Maui.Platform.GTK.Renderers;

namespace System.Maui.Platform.GTK.Extensions
{
	public static class ImageExtensions
	{
		public static Pixbuf ToPixbuf(this ImageSource imagesource)
		{
			return ToPixbufAux(imagesource, null);
		}

		public static Pixbuf ToPixbuf(this ImageSource imagesource, Size size)
		{
			return ToPixbufAux(imagesource, size);
		}

		private static Pixbuf ToPixbufAux(this ImageSource imagesource, Size? size)
		{
			try
			{
				Pixbuf image = null;

				var filesource = imagesource as FileImageSource;

				if (filesource != null)
				{
					var file = filesource.File;

					if (!string.IsNullOrEmpty(file))
					{
						var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

						image = size.HasValue
							? new Pixbuf(imagePath, (int)size.Value.Width, (int)size.Value.Height)
							: new Pixbuf(imagePath);
					}
				}

				return image;
			}
			catch
			{
				return null;
			}
		}

		internal static async Task<Pixbuf> GetNativeImageAsync(this ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (imageSource == null || imageSource.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadImageAsync(imageSource, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				Log.Warning("Image loading", "Image load cancelled");
			}
			catch (Exception ex)
			{
				Log.Warning("Image loading", $"Image load failed: {ex}");
			}

			return null;
		}

		internal static Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableProperty imageSourceProperty, Action<Pixbuf> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return renderer.ApplyNativeImageAsync(null, imageSourceProperty, onSet, onLoading, cancellationToken);
		}

		internal static async Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableObject bindable, BindableProperty imageSourceProperty, Action<Pixbuf> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = renderer ?? throw new ArgumentNullException(nameof(renderer));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			// TODO: it might be good to make sure the renderer has not been disposed

			// makse sure things are good before we start
			var element = bindable ?? renderer.Element;

			var nativeRenderer = renderer as IVisualNativeElementRenderer;

			if (element == null || renderer.Container == null || (nativeRenderer != null && nativeRenderer.Control == null))
				return;

			onLoading?.Invoke(true);
			if (element.GetValue(imageSourceProperty) is ImageSource initialSource && !initialSource.IsEmpty)
			{
				try
				{
					using (var drawable = await initialSource.GetNativeImageAsync(cancellationToken))
					{
						// TODO: it might be good to make sure the renderer has not been disposed

						// we are back, so update the working element
						element = bindable ?? renderer.Element;

						// makse sure things are good now that we are back
						if (element == null || renderer.Container == null || (nativeRenderer != null && nativeRenderer.Control == null))
							return;

						// only set if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
							onSet(drawable);
					}
				}
				finally
				{
					if (element != null && onLoading != null)
					{
						// only mark as finished if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
							onLoading.Invoke(false);
					}
				}
			}
			else
			{
				onSet(null);
				onLoading?.Invoke(false);
			}
		}

		internal static async Task ApplyNativeImageAsync(this BindableObject bindable, BindableProperty imageSourceProperty, Action<Pixbuf> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = bindable ?? throw new ArgumentNullException(nameof(bindable));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			onLoading?.Invoke(true);
			if (bindable.GetValue(imageSourceProperty) is ImageSource initialSource)
			{
				try
				{
					using (var drawable = await initialSource.GetNativeImageAsync(cancellationToken))
					{
						// only set if we are still on the same image
						if (bindable.GetValue(imageSourceProperty) == initialSource)
							onSet(drawable);
					}
				}
				finally
				{
					if (onLoading != null)
					{
						// only mark as finished if we are still on the same image
						if (bindable.GetValue(imageSourceProperty) == initialSource)
							onLoading.Invoke(false);
					}
				}
			}
			else
			{
				onSet(null);
				onLoading?.Invoke(false);
			}
		}
	}
}
