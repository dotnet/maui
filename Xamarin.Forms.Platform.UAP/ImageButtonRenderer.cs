using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.Internals;
using WThickness = Windows.UI.Xaml.Thickness;
using WImage = Windows.UI.Xaml.Controls.Image;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Xamarin.Forms.Platform.UWP
{
	public class ImageButtonRenderer : ViewRenderer<ImageButton, FormsButton>, IImageVisualElementRenderer
	{
		bool _measured;
		bool _disposed;
		WImage _image;
		FormsButton _formsButton;

		public ImageButtonRenderer() : base()
		{
			ImageElementManager.Init(this);
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
					_image.ImageOpened -= OnImageOpened;
					_image.ImageFailed -= OnImageFailed;
				}
			}

			base.Dispose(disposing);
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (_image?.Source == null)
				return new SizeRequest();

			_measured = true;


			var result = new Size
			{
				Width = ((BitmapSource)_image.Source).PixelWidth,
				Height = ((BitmapSource)_image.Source).PixelHeight
			};

			return new SizeRequest(result);
		}


		protected async override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_formsButton = new FormsButton();
					_formsButton.Padding = new WThickness(0);
					_formsButton.BorderThickness = new WThickness(0);
					_formsButton.Background = null;

					_image = new Windows.UI.Xaml.Controls.Image()
					{
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
						Stretch = Stretch.Uniform,
					};

					_image.ImageOpened += OnImageOpened;
					_image.ImageFailed += OnImageFailed;
					_formsButton.Content = _image;

					_formsButton.Click += OnButtonClick;
					_formsButton.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
					_formsButton.Loaded += ButtonOnLoaded;

					SetNativeControl(_formsButton);

				}
				else
				{
					WireUpFormsVsm();
				}

				//TODO: We may want to revisit this strategy later. If a user wants to reset any of these to the default, the UI won't update.
				if (Element.IsSet(VisualElement.BackgroundColorProperty) && Element.BackgroundColor != (Color)VisualElement.BackgroundColorProperty.DefaultValue)
					UpdateBackground();

				if (Element.IsSet(ImageButton.BorderColorProperty) && Element.BorderColor != (Color)ImageButton.BorderColorProperty.DefaultValue)
					UpdateBorderColor();

				if (Element.IsSet(ImageButton.BorderWidthProperty) && Element.BorderWidth != (double)ImageButton.BorderWidthProperty.DefaultValue)
					UpdateBorderWidth();

				if (Element.IsSet(ImageButton.CornerRadiusProperty) && Element.CornerRadius != (int)ImageButton.CornerRadiusProperty.DefaultValue)
					UpdateBorderRadius();
				if (Element.IsSet(Button.PaddingProperty) && Element.Padding != (Thickness)Button.PaddingProperty.DefaultValue)
					UpdatePadding();

				await TryUpdateSource().ConfigureAwait(false);

			}
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
				Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
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


		void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
		{
			if (_measured)
			{
				ImageElementManager.RefreshImage(Element);
			}

			Element?.SetIsLoading(false);
		}

		protected virtual void OnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
		{
			Log.Warning("Image Loading", $"Image failed to load: {exceptionRoutedEventArgs.ErrorMessage}");
			Element?.SetIsLoading(false);
		}



		void ButtonOnLoaded(object o, RoutedEventArgs routedEventArgs)
		{
			WireUpFormsVsm();
		}

		void WireUpFormsVsm()
		{
			if (Element.UseFormsVsm())
			{
				InterceptVisualStateManager.Hook(Control.GetFirstDescendant<Windows.UI.Xaml.Controls.Grid>(), Control, Element);
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackground();
			}
			else if (e.PropertyName == ImageButton.BorderColorProperty.PropertyName)
			{
				UpdateBorderColor();
			}
			else if (e.PropertyName == ImageButton.BorderWidthProperty.PropertyName)
			{
				UpdateBorderWidth();
			}
			else if (e.PropertyName == ImageButton.CornerRadiusProperty.PropertyName)
			{
				UpdateBorderRadius();
			}
			else if (e.PropertyName == ImageButton.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
			else if (e.PropertyName == ImageButton.SourceProperty.PropertyName)
				await TryUpdateSource().ConfigureAwait(false);
		}
		void UpdatePadding()
		{
			_image.Margin = new WThickness(0);

			Control.Padding = new WThickness(
					Element.Padding.Left,
					Element.Padding.Top,
					Element.Padding.Right,
					Element.Padding.Bottom
				);
		}
		protected override void UpdateBackgroundColor()
		{
			// Button is a special case; we don't want to set the Control's background
			// because it goes outside the bounds of the Border/ContentPresenter, 
			// which is where we might change the BorderRadius to create a rounded shape.
			return;
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		bool IImageVisualElementRenderer.IsDisposed => _disposed;

		void OnButtonClick(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void OnPointerPressed(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendPressed();
		}

		void UpdateBackground()
		{
			Control.BackgroundColor = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["ButtonBackgroundThemeBrush"];
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor != Color.Default ? Element.BorderColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		void UpdateBorderRadius()
		{
			Control.BorderRadius = Element.CornerRadius;
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == (double)ImageButton.BorderWidthProperty.DefaultValue ? new WThickness(3) : new WThickness(Element.BorderWidth);
		}

		void IImageVisualElementRenderer.SetImage(Windows.UI.Xaml.Media.ImageSource image)
		{
			_image.Source = image;
		}

		WImage IImageVisualElementRenderer.GetImage() =>
			Control?.Content as WImage;
	}
}
