using System;
using System.ComponentModel;
using Gtk;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;
using GtkImageButton = Xamarin.Forms.Platform.GTK.Controls.ImageButton;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class ButtonRenderer : ViewRenderer<Button, GtkImageButton>
	{
		private const uint DefaultBorderWidth = 1;

		protected override bool PreventGestureBubbling { get; set; } = true;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var req = Control.SizeRequest();

			var widthFits = widthConstraint >= req.Width;
			var heightFits = heightConstraint >= req.Height;

			var size = new Size(widthFits ? req.Width : (int)widthConstraint,
				heightFits ? req.Height : (int)heightConstraint);

			return new SizeRequest(size);
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
			{
				Control.Clicked -= OnButtonClicked;
				Control.ButtonPressEvent -= OnButtonPressEvent;
				Control.ButtonReleaseEvent -= OnButtonReleaseEvent;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					// To allow all available options in Xamarin.Forms, a custom control has been created.
					// Can set text, text color, border, image, etc.
					var btn = new GtkImageButton();
					SetNativeControl(btn);

					Control.Clicked += OnButtonClicked;
					Control.ButtonPressEvent += OnButtonPressEvent;
					Control.ButtonReleaseEvent += OnButtonReleaseEvent;
				}

				UpdateBackgroundColor();
				UpdateTextColor();
				UpdateText();
				UpdateBorder();
				UpdateContent();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Button.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Button.ImageSourceProperty.PropertyName || e.PropertyName == Button.ContentLayoutProperty.PropertyName)
				UpdateContent();
		}

		protected override void UpdateBackgroundColor()
		{
			if (Element == null)
				return;

			if (Element.BackgroundColor.IsDefault)
			{
				Control.ResetBackgroundColor();
			}
			else if (Element.BackgroundColor != Color.Transparent)
			{
				Control.SetBackgroundColor(Element.BackgroundColor.ToGtkColor());
			}
			else
			{
				Control.SetBackgroundColor(null);
			}
		}

		protected override void SetAccessibilityLabel()
		{
			var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);

			if (string.IsNullOrWhiteSpace(elemValue)
				&& Control?.Accessible.Description == Control?.LabelWidget.Text)
				return;

			base.SetAccessibilityLabel();
		}

		private void OnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			((IButtonController)Element)?.SendPressed();
		}

		private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			((IButtonController)Element)?.SendReleased();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			((IButtonController)Element)?.SendClicked();
		}

		private void UpdateText()
		{
			var span = new Span()
			{
				FontAttributes = Element.FontAttributes,
				FontFamily = Element.FontFamily,
				FontSize = Element.FontSize,
				Text = GLib.Markup.EscapeText(Element.UpdateFormsText(Element.Text ?? string.Empty, Element.TextTransform)) ?? string.Empty
			};

			Control.LabelWidget.SetTextFromSpan(span);
		}

		private void UpdateTextColor()
		{
			if (!Element.TextColor.IsDefaultOrTransparent())
			{
				Control.SetForegroundColor(Element.TextColor.ToGtkColor());
			}
		}

		private void UpdateBorder()
		{
			var borderWidth = Element.BorderWidth < 0
					   ? DefaultBorderWidth
					   : (uint)Element.BorderWidth;

			Control.SetBorderWidth(borderWidth);

			if (Element.BorderColor.IsDefault)
			{
				Control.ResetBorderColor();
			}
			else if (Element.BorderColor != Color.Transparent)
			{
				Control.SetBorderColor(Element.BorderColor.ToGtkColor());
			}
			else
			{
				Control.SetBorderColor(null);
			}
		}

		private void UpdateContent()
		{
			this.ApplyNativeImageAsync(Button.ImageSourceProperty, image =>
			{
				if (image != null)
				{
					Control.ImageWidget.Pixbuf = image;
					Control.ImageSpacing = (uint)Element.ContentLayout.Spacing;
					Control.SetImagePosition(Element.ContentLayout.Position.AsPositionType());
				}

				Control.ImageWidget.Visible = image != null;
			});
		}
	}
}