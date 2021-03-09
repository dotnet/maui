using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls.Compatibility.Internals;
using CoreAnimation;

#if __MOBILE__
using UIKit;
using NativeImage = UIKit.UIImage;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using AppKit;
using NativeImage = AppKit.NSImage;
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class ImageElementManager
	{
		public static void Init(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged += OnElementPropertyChanged;
			renderer.ElementChanged += OnElementChanged;
			renderer.ControlChanged += OnControlChanged;
		}

		public static void Dispose(IImageVisualElementRenderer renderer)
		{
			renderer.ElementPropertyChanged -= OnElementPropertyChanged;
			renderer.ElementChanged -= OnElementChanged;
			renderer.ControlChanged -= OnControlChanged;


#if __MOBILE__
			if (renderer.GetImage() is FormsUIImageView imageView && imageView.Animation != null)
			{
				imageView.Animation.AnimationStopped -= OnAnimationStopped;
				imageView.Animation.Dispose();
				imageView.Animation = null;
			}
#endif

		}


		async static Task StartStopAnimation(IImageVisualElementRenderer renderer)
		{
#if __MOBILE__
			if (renderer.IsDisposed || renderer.Element == null || renderer.Control == null)
				return;


			if (renderer.GetImage() is FormsUIImageView imageView &&
				renderer.Element is IImageElement imageElement &&
				renderer.Element is IImageController imageController)
			{
				if (imageElement.IsLoading)
					return;

				if (imageView.Animation == null && imageElement.IsAnimationPlaying)
					await SetImage(renderer, imageElement).ConfigureAwait(false);
				if (imageView.Animation != null && imageElement.IsAnimationPlaying && !imageView.IsAnimating)
					imageView.StartAnimating();
				else if (imageView.Animation != null && !imageElement.IsAnimationPlaying && imageView.IsAnimating)
					imageView.StopAnimating();
			}
#else
			await Task.CompletedTask;
#endif

		}

		static void OnControlChanged(object sender, EventArgs e)
		{
			var renderer = sender as IImageVisualElementRenderer;
			var imageElement = renderer.Element as IImageElement;
			SetAspect(renderer, imageElement);
			SetOpacity(renderer, imageElement);
		}

		static void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.NewElement != null)
			{
				var renderer = sender as IImageVisualElementRenderer;
				var imageElement = renderer.Element as IImageElement;

				SetAspect(renderer, imageElement);
				SetOpacity(renderer, imageElement);
			}
		}

		static async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var renderer = sender as IImageVisualElementRenderer;
			var imageElement = renderer.Element as IImageElement;

			if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
				SetOpacity(renderer, renderer.Element as IImageElement);
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect(renderer, renderer.Element as IImageElement);
			else if (e.PropertyName == Image.IsAnimationPlayingProperty.PropertyName)
				await StartStopAnimation(renderer).ConfigureAwait(false);
		}



		public static void SetAspect(IImageVisualElementRenderer renderer, IImageElement imageElement)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();


			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}
#if __MOBILE__
			Control.ContentMode = imageElement.Aspect.ToUIViewContentMode();
#else
			Control.Layer.ContentsGravity = imageElement.Aspect.ToNSViewContentMode();
#endif
		}

		public static void SetOpacity(IImageVisualElementRenderer renderer, IImageElement imageElement)
		{
			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}
#if __MOBILE__
			Control.Opaque = imageElement.IsOpaque;
#else
			(Control as FormsNSImageView)?.SetIsOpaque(imageElement.IsOpaque);
#endif
		}

		public static async Task SetImage(IImageVisualElementRenderer renderer, IImageElement imageElement, Image oldElement = null)
		{
			_ = renderer ?? throw new ArgumentNullException(nameof(renderer), $"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(renderer)} cannot be null");
			_ = imageElement ?? throw new ArgumentNullException(nameof(imageElement), $"{nameof(ImageElementManager)}.{nameof(SetImage)} {nameof(imageElement)} cannot be null");

			var Element = renderer.Element;
			var Control = renderer.GetImage();

			if (renderer.IsDisposed || Element == null || Control == null)
			{
				return;
			}

			var imageController = imageElement as IImageController;

			var source = imageElement.Source;
#if __MOBILE__
			if (Control.Image?.Images != null && Control.Image.Images.Length > 1)
			{
				renderer.SetImage(null);
			}
#else
			if (Control.Image != null && Control.Image.Representations().Length > 1)
			{
				renderer.SetImage(null);
			}
#endif
			else if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource oldFile && source is FileImageSource newFile && oldFile == newFile)
					return;

#if __MOBILE__
				if (Control is FormsUIImageView imageView)
				{
					if (imageView.Animation != null)
					{
						Control.StopAnimating();
						imageView.AnimationStopped -= OnAnimationStopped;
						imageView.Animation.Dispose();
					}

					renderer.SetImage(null);
					imageView.Animation = null;
				}
#endif

			}

			imageController?.SetIsLoading(true);

			try
			{
#if __MOBILE__
				bool useAnimation = imageController.GetLoadAsAnimation();
				IAnimationSourceHandler handler = null;
				if (useAnimation && source != null)
					handler = Controls.Internals.Registrar.Registered.GetHandlerForObject<IAnimationSourceHandler>(source);

				if (handler != null)
				{
					FormsCAKeyFrameAnimation animation = await handler.LoadImageAnimationAsync(source, scale: (float)UIScreen.MainScreen.Scale).ConfigureAwait(false);

					if (animation != null && Control is FormsUIImageView imageView && imageElement.Source == source)
					{
						if (imageView.Animation != null)
							imageView.AnimationStopped -= OnAnimationStopped;

						imageView.Animation = animation;
						imageView.AnimationStopped += OnAnimationStopped;

						if ((bool)Element.GetValue(Image.IsAnimationPlayingProperty))
							imageView.StartAnimating();
					}
					else
					{
						animation?.Dispose();
					}
				}
				else
#endif
				{
					var uiimage = await source.GetNativeImageAsync();

					if (renderer.IsDisposed)
						return;

					// only set if we are still on the same image
					if (Control != null && imageElement.Source == source)
						renderer.SetImage(uiimage);
					else
						uiimage?.Dispose();
				}
			}
			finally
			{
				// only mark as finished if we are still on the same image
				if (imageElement.Source == source)
					imageController?.SetIsLoading(false);
			}

			(imageElement as IViewController)?.NativeSizeChanged();
		}

#if __MOBILE__
		static void OnAnimationStopped(object sender, CAAnimationStateEventArgs e)
		{
			if (sender is FormsUIImageView imageView &&
				e.Finished &&
				imageView.Superview is IImageVisualElementRenderer renderer &&
				renderer.Element is IElementController imageController)
			{
				imageController.SetValueFromRenderer(Image.IsAnimationPlayingProperty, false);
			}

		}
#endif

		internal static async Task<NativeImage> GetNativeImageAsync(this ImageSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null || source.IsEmpty)
				return null;

			var handler = Controls.Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source);
			if (handler == null)
				return null;

			try
			{
#if __MOBILE__
				float scale = (float)UIScreen.MainScreen.Scale;
#else
				float scale = (float)NSScreen.MainScreen.BackingScaleFactor;
#endif

				return await handler.LoadImageAsync(source, scale: scale, cancelationToken: cancellationToken);
			}
			catch (OperationCanceledException)
			{
				Controls.Internals.Log.Warning("Image loading", "Image load cancelled");
			}
			catch (Exception ex)
			{
				Controls.Internals.Log.Warning("Image loading", $"Image load failed: {ex}");
			}

			return null;
		}

#if __MOBILE__
		internal static Task ApplyNativeImageAsync(this IShellContext shellContext, BindableObject bindable, BindableProperty imageSourceProperty, Action<UIImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			var renderer = shellContext as IVisualElementRenderer ?? throw new InvalidOperationException($"The shell context {shellContext.GetType()} must be a {typeof(IVisualElementRenderer)}.");

			return renderer.ApplyNativeImageAsync(bindable, imageSourceProperty, onSet, onLoading, cancellationToken);
		}
#endif
		internal static Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableProperty imageSourceProperty, Action<NativeImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return renderer.ApplyNativeImageAsync(null, imageSourceProperty, onSet, onLoading, cancellationToken);
		}

		internal static async Task ApplyNativeImageAsync(this IVisualElementRenderer renderer, BindableObject bindable, BindableProperty imageSourceProperty, Action<NativeImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
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

		internal static async Task ApplyNativeImageAsync(this BindableObject bindable, BindableProperty imageSourceProperty, Action<NativeImage> onSet, Action<bool> onLoading = null, CancellationToken cancellationToken = default(CancellationToken))
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
