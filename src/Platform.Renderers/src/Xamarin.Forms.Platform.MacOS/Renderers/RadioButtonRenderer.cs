using System;
using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class RadioButtonRenderer : ViewRenderer<RadioButton, NSButton>
	{
		class FormsNSButton : NSButton
		{
			public event Action Pressed;

			public event Action Released;

			public override void MouseDown(NSEvent theEvent)
			{
				Pressed?.Invoke();

				base.MouseDown(theEvent);

				Released?.Invoke();
			}
		}

		static readonly IntPtr _tokenObserveState = (IntPtr)1;

		protected override void Dispose(bool disposing)
		{
			ObserveStateChange(false);

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<RadioButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var btn = new FormsNSButton();
					btn.SetButtonType(NSButtonType.Radio);
					SetNativeControl(btn);
					ObserveStateChange(true);
				}

				UpdateContent();
				UpdateFont();
				UpdateBorder();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == RadioButton.ContentProperty.PropertyName || e.PropertyName == RadioButton.TextColorProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == RadioButton.FontAttributesProperty.PropertyName
					|| e.PropertyName == RadioButton.FontFamilyProperty.PropertyName
					|| e.PropertyName == RadioButton.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == RadioButton.BorderWidthProperty.PropertyName ||
					e.PropertyName == RadioButton.CornerRadiusProperty.PropertyName ||
					e.PropertyName == RadioButton.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundVisibility();
			else if (e.PropertyName == RadioButton.IsCheckedProperty.PropertyName)
				UpdateCheck();
		}

		void UpdateBackgroundVisibility()
		{
			var model = Element;
			var shouldDrawImage = model.BackgroundColor == Color.Default;
			if (!shouldDrawImage)
				Control.Cell.BackgroundColor = model.BackgroundColor.ToNSColor();
		}

		void UpdateBorder()
		{
			var uiButton = Control;
			var button = Element;

			if (button.BorderColor != Color.Default)
				uiButton.Layer.BorderColor = button.BorderColor.ToCGColor();

			uiButton.Layer.BorderWidth = (float)button.BorderWidth;
			uiButton.Layer.CornerRadius = button.CornerRadius;

			UpdateBackgroundVisibility();
		}

		void UpdateFont()
		{
			Font font = Font.OfSize(Element.FontFamily, Element.FontSize).WithAttributes(Element.FontAttributes);
			Control.Font = font.ToNSFont();
		}

		void UpdateContent()
		{
			var text = Element.ContentAsString();

			var color = Element.TextColor;
			if (color == Color.Default)
			{
				Control.Title = text ?? "";
			}
			else
			{
				Font font = Font.OfSize(Element.FontFamily, Element.FontSize).WithAttributes(Element.FontAttributes);
				var textWithColor = new NSAttributedString(text ?? "", font: font.ToNSFont(), foregroundColor: color.ToNSColor(), paragraphStyle: new NSMutableParagraphStyle() { Alignment = NSTextAlignment.Center });
				Control.AttributedTitle = textWithColor;
			}
		}

		void UpdateCheck()
		{
			Control.State = Element.IsChecked ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		void ObserveStateChange(bool observe)
		{
			if (observe)
			{
				AddObserver(this, "state", NSKeyValueObservingOptions.New | NSKeyValueObservingOptions.Old, _tokenObserveState);
			}
			else
			{
				RemoveObserver(this, "state", _tokenObserveState);
			}
		}

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr ctx)
		{
			if (ctx == _tokenObserveState)
			{
				OnStateChanged();
			}
			else
			{
				// invoke the base implementation for unhandled events
				base.ObserveValue(keyPath, ofObject, change, ctx);
			}
		}

		void OnStateChanged()
		{
			if (Element == null || Control == null)
			{
				return;
			}

			Element.IsChecked = Control.State == NSCellStateValue.On;
		}
	}
}
