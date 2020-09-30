using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using WButton = System.Windows.Controls.Button;
using WImage = System.Windows.Controls.Image;
using WThickness = System.Windows.Thickness;

namespace Xamarin.Forms.Platform.WPF
{
	public class ImageButtonRenderer : ViewRenderer<ImageButton, WButton>, IVisualElementRenderer
	{
		bool _disposed;
		WButton _button;
		WImage _image;

		public ImageButtonRenderer() : base() { }

		protected async override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_image = new WImage
					{
						VerticalAlignment = System.Windows.VerticalAlignment.Center,
						HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
						Stretch = System.Windows.Media.Stretch.Uniform
					};

					_image.ImageFailed += OnImageFailed;

					_button = new WButton
					{
						Padding = new WThickness(0),
						BorderThickness = new WThickness(0),
						Background = null,

						Content = _image
					};

					_button.Click += OnButtonClick;

					SetNativeControl(_button);
				}

				if (Element.BorderColor != Color.Default)
					UpdateBorderColor();

				if (Element.BorderWidth != 0)
					UpdateBorderWidth();

				await TryUpdateSource();
				UpdateAspect();

				if (Element.IsSet(Button.PaddingProperty))
					UpdatePadding();
			}
		}

		protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ImageButton.SourceProperty.PropertyName)
				await TryUpdateSource().ConfigureAwait(false);
			else if (e.PropertyName == ImageButton.BorderColorProperty.PropertyName)
				UpdateBorderColor();
			else if (e.PropertyName == ImageButton.BorderWidthProperty.PropertyName)
			{
				UpdateBorderWidth();
				UpdatePadding();
			}
			else if (e.PropertyName == ImageButton.AspectProperty.PropertyName)
				UpdateAspect();
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_image != null)
				{
					_image.ImageFailed -= OnImageFailed;
				}

				if (_button != null)
				{
					_button.Click -= OnButtonClick;
				}
			}

			_disposed = true;
			base.Dispose(disposing);
		}

		protected virtual async Task TryUpdateSource()
		{
			try
			{
				await UpdateSource().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Warning(nameof(ImageButtonRenderer), "Error loading image: {0}", ex);
			}
			finally
			{
				Element.SetIsLoading(false);
			}
		}

		protected async Task UpdateSource()
		{
			if (Element == null || Control == null)
				return;

			Element.SetIsLoading(true);

			ImageSource source = Element.Source;
			IImageSourceHandler handler;
			if (source != null && (handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				System.Windows.Media.ImageSource imageSource;

				try
				{
					imageSource = await handler.LoadImageAsync(source);
				}
				catch (OperationCanceledException)
				{
					imageSource = null;
				}

				// In the time it takes to await the imagesource, some zippy little app
				// might have disposed of this Image already.
				if (_image != null)
					_image.Source = imageSource;

				RefreshImage();
			}
			else
			{
				_image.Source = null;
				Element.SetIsLoading(false);
			}
		}

		void RefreshImage()
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.RendererReady);
		}

		void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void OnImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
		{
			Log.Warning("Image loading", $"Image failed to load: {e.ErrorException.Message}");
			Element?.SetIsLoading(false);
		}

		void UpdateBorderColor()
		{
			Control.UpdateDependencyColor(WButton.BorderBrushProperty, Element.BorderColor);
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness =
				Element.BorderWidth <= 0d
					? new WThickness(1)
					: new WThickness(Element.BorderWidth);
		}

		void UpdateAspect()
		{
			_image.Stretch = Element.Aspect.ToStretch();
			if (Element.Aspect == Aspect.Fill)
			{
				Control.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				Control.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			}
			else
			{
				Control.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
				Control.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			}
		}

		void UpdatePadding()
		{
			Control.Padding = new WThickness(
				Element.Padding.Left,
				Element.Padding.Top,
				Element.Padding.Right,
				Element.Padding.Bottom
			);
		}
	}
}
