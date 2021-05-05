using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Input;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using WThickness = Microsoft.UI.Xaml.Thickness;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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

			// The size needs to be the entire size needed for the button (including padding, borders, etc.)
			// Not just the size of the image.
			var btn = Control;
			btn.Measure(new Windows.Foundation.Size(widthConstraint, heightConstraint));

			var size = new Size(Math.Ceiling(btn.DesiredSize.Width), Math.Ceiling(btn.DesiredSize.Height));

			return new SizeRequest(size);
		}

		protected async override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_formsButton = new FormsButton();
					_formsButton.Padding = WinUIHelpers.CreateThickness(0);
					_formsButton.BorderThickness = WinUIHelpers.CreateThickness(0);
					_formsButton.Background = null;

					_image = new Microsoft.UI.Xaml.Controls.Image()
					{
						VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
						HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
						Stretch = WStretch.Uniform,
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
					UpdateImageButtonBackground();

				if (Element.IsSet(VisualElement.BackgroundProperty) && Element.Background != (Brush)VisualElement.BackgroundProperty.DefaultValue)
					UpdateImageButtonBackground();

				if (Element.IsSet(ImageButton.BorderColorProperty) && Element.BorderColor != (Color)ImageButton.BorderColorProperty.DefaultValue)
					UpdateBorderColor();

				if (Element.IsSet(ImageButton.BorderWidthProperty) && Element.BorderWidth != (double)ImageButton.BorderWidthProperty.DefaultValue)
					UpdateBorderWidth();

				if (Element.IsSet(ImageButton.CornerRadiusProperty) && Element.CornerRadius != (int)ImageButton.CornerRadiusProperty.DefaultValue)
					UpdateBorderRadius();

				// By default Button loads width padding 8, 4, 8 ,4
				if (Element.IsSet(Button.PaddingProperty))
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
				ImageElementManager.RefreshImage(this);
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
				InterceptVisualStateManager.Hook(Control.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>(), Control, Element);
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
			{
				UpdateImageButtonBackground();
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
			_image.Margin = WinUIHelpers.CreateThickness(0);

			// Apply the padding to the containing button, not the image
			_formsButton.Padding = WinUIHelpers.CreateThickness(
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

		protected override void UpdateBackground()
		{
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

		void UpdateImageButtonBackground()
		{
			if (Brush.IsNullOrEmpty(Element.Background))
				Control.BackgroundColor = Element.BackgroundColor.IsNotDefault() ? Maui.ColorExtensions.ToNative(Element.BackgroundColor) : (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["ButtonBackgroundThemeBrush"];
			else
				Control.BackgroundColor = Element.Background.ToBrush();
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor.IsNotDefault() ? Maui.ColorExtensions.ToNative(Element.BorderColor) : (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		void UpdateBorderRadius()
		{
			Control.BorderRadius = Element.CornerRadius;
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == (double)ImageButton.BorderWidthProperty.DefaultValue ? WinUIHelpers.CreateThickness(3) : WinUIHelpers.CreateThickness(Element.BorderWidth);
		}

		void IImageVisualElementRenderer.SetImage(Microsoft.UI.Xaml.Media.ImageSource image)
		{
			_image.Source = image;
		}

		WImage IImageVisualElementRenderer.GetImage() =>
			Control?.Content as WImage;
	}
}
