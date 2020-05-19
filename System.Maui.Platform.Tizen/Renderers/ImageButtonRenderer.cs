using System;
using ElmSharp;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ImageButtonRenderer : ViewRenderer<ImageButton, Box>
	{
		public ImageButtonRenderer()
		{
			RegisterPropertyHandler(ImageButton.SourceProperty, UpdateSource);
			RegisterPropertyHandler(ImageButton.PaddingProperty, UpdatePadding);
			RegisterPropertyHandler(ImageButton.CornerRadiusProperty, UpdateRadius);
			RegisterPropertyHandler(ImageButton.BorderWidthProperty, UpdateBorderWidth);
			RegisterPropertyHandler(ImageButton.BorderColorProperty, UpdateBorderColor);
			RegisterPropertyHandler(ImageButton.AspectProperty, UpdateAspect);
		}

		Native.Image _image;
		EButton _button;
		Native.RoundRectangle _round;
		Native.BorderRectangle _border;
		protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Box(Forms.NativeParent));
				Control.SetLayoutCallback(OnLayout);
				_round = new Native.RoundRectangle(Forms.NativeParent);
				_round.Show();
				_border = new Native.BorderRectangle(Forms.NativeParent);
				_border.Show();
				_image = new Native.Image(Forms.NativeParent);
				_image.Show();
				_button = new EButton(Forms.NativeParent)
				{
					Style = "transparent"
				};
				_button.Clicked += OnClicked;
				_button.Pressed += OnPressed;
				_button.Released += OnReleased;
				_button.Show();
				Control.PackEnd(_round);
				Control.PackEnd(_image);
				Control.PackEnd(_border);
				Control.PackEnd(_button);
			}
			base.OnElementChanged(e);
		}

		protected virtual void UpdateAfterLoading()
		{
			_image.IsOpaque = Element.IsOpaque;
		}

		protected override ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var size = _image.Measure(availableHeight, availableHeight);
			size.Width += Forms.ConvertToScaledPixel(Element.Padding.HorizontalThickness);
			size.Height += Forms.ConvertToScaledPixel(Element.Padding.VerticalThickness);
			return size;
		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			_round.Color = Element.BackgroundColor.ToNative();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_button != null)
				{
					_button.Clicked -= OnClicked;
					_button.Pressed -= OnPressed;
					_button.Released -= OnReleased;
					_button = null;
				}
			}
			base.Dispose(disposing);
		}

		void OnReleased(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendReleased();
		}

		void OnPressed(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendPressed();
		}

		void OnClicked(object sender, EventArgs e)
		{
			(Element as IButtonController)?.SendClicked();
		}

		void OnLayout()
		{
			var outter = Control.Geometry;
			var width = outter.Width - Forms.ConvertToScaledPixel(Element.Padding.HorizontalThickness);
			var height = outter.Height - Forms.ConvertToScaledPixel(Element.Padding.VerticalThickness);
			var left = outter.Left + Forms.ConvertToScaledPixel(Element.Padding.Left);
			var top = outter.Top + Forms.ConvertToScaledPixel(Element.Padding.Top);
			var imageBound = new Rect(left, top, width, height);

			_image.Geometry = imageBound;
			_button.Geometry = outter;
			_round.Draw(outter);
			_border.Draw(outter);
		}

		void UpdatePadding()
		{
			Control.MarkChanged();
		}

		async void UpdateSource()
		{
			ImageSource source = Element.Source;
			(Element as IImageController)?.SetIsLoading(true);

			if (Control != null)
			{
				bool success = await _image.LoadFromImageSourceAsync(source);
				if (!IsDisposed && success)
				{
					(Element as IVisualElementController)?.NativeSizeChanged();
					UpdateAfterLoading();
				}
			}

			if (!IsDisposed)
				((IImageController)Element).SetIsLoading(false);
		}

		void UpdateRadius(bool init)
		{
			if (Element.CornerRadius > 0)
			{
				_round.SetRadius(Forms.ConvertToScaledPixel(Element.CornerRadius));
				_border.SetRadius(Forms.ConvertToScaledPixel(Element.CornerRadius));
			}
			else
			{
				_round.SetRadius(0);
				_border.SetRadius(0);
			}
			if (!init)
			{
				_round.Draw();
				_border.Draw();
			}
		}

		void UpdateBorderWidth(bool init)
		{
			if (Element.BorderWidth > 0)
			{
				_border.BorderWidth = Forms.ConvertToScaledPixel(Element.BorderWidth);
			}
			else
			{
				_border.BorderWidth = 0;
			}
			if (!init)
			{
				_border.Draw();
			}
		}

		void UpdateBorderColor()
		{
			_border.Color = Element.BorderColor.ToNative();
		}

		void UpdateAspect()
		{
			_image.ApplyAspect(Element.Aspect);
		}
	}
}
