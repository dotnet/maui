using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	public static class NSImageExtensions
	{
		public static NSImage ResizeTo(this NSImage self, CoreGraphics.CGSize newSize)
		{
			if (self == null)
				return null;
			self.ResizingMode = NSImageResizingMode.Stretch;
			var resizedImage = new NSImage(newSize);
			resizedImage.LockFocus();
			self.Size = newSize;
			NSGraphicsContext.CurrentContext.ImageInterpolation = NSImageInterpolation.High;
			self.Draw(CoreGraphics.CGPoint.Empty, new CoreGraphics.CGRect(0, 0, newSize.Width, newSize.Height),
				NSCompositingOperation.Copy, 1.0f);
			resizedImage.UnlockFocus();
			return resizedImage;
		}

		internal static async Task<NSImage> GetNativeImageAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null || source.IsEmpty)
				return null;

			var handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadImageAsync(source, scale: (float)NSScreen.MainScreen.BackingScaleFactor, cancelationToken: cancellationToken);
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

		internal static Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableProperty imageSourceProperty, Action<NSImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return renderer.ApplyNativeImageAsync(null, imageSourceProperty, onSet, onLoading, cancellationToken);
		}

		internal static async Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableObject bindable, BindableProperty imageSourceProperty, Action<NSImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = renderer ?? throw new ArgumentNullException(nameof(renderer));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			// TODO: it might be good to make sure the renderer has not been disposed

			// makse sure things are good before we start
			var element = bindable ?? renderer.Element;

			var nativeRenderer = renderer as IVisualNativeElementRenderer;

			if (element == null || renderer.NativeView == null || (nativeRenderer != null && nativeRenderer.Control == null))
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
						if (element == null || renderer.NativeView == null || (nativeRenderer != null && nativeRenderer.Control == null))
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

		internal static async Task ApplyNativeImageAsync(this BindableObject bindable, BindableProperty imageSourceProperty, Action<NSImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = bindable ?? throw new ArgumentNullException(nameof(bindable));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			onLoading?.Invoke(true);
			if (bindable.GetValue(imageSourceProperty) is ImageSource initialSource)
			{
				try
				{
					using (var nsimage = await initialSource.GetNativeImageAsync(cancellationToken))
					{
						// only set if we are still on the same image
						if (bindable.GetValue(imageSourceProperty) == initialSource)
							onSet(nsimage);
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