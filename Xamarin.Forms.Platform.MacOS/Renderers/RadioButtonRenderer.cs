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
			if (Control != null)
				Control.Activated -= OnButtonActivated;

			var formsButton = Control as FormsNSButton;
			if (formsButton != null)
			{
				formsButton.Pressed -= HandleButtonPressed;
				formsButton.Released -= HandleButtonReleased;
			}

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
					btn.Pressed += HandleButtonPressed;
					btn.Released += HandleButtonReleased;
					SetNativeControl(btn);
					ObserveStateChange(true);

					Control.Activated += OnButtonActivated;
				}

				UpdateText();
				UpdateFont();
				UpdateBorder();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == RadioButton.TextProperty.PropertyName || e.PropertyName == RadioButton.TextColorProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == RadioButton.FontProperty.PropertyName)
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

		void OnButtonActivated(object sender, EventArgs eventArgs)
		{
			((IButtonController)Element)?.SendClicked();
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
			Control.Font = Element.Font.ToNSFont();
		}

		void UpdateText()
		{
			var color = Element.TextColor;
			if (color == Color.Default)
			{
				Control.Title = Element.Text ?? "";
			}
			else
			{
				var textWithColor = new NSAttributedString(Element.Text ?? "", font: Element.Font.ToNSFont(), foregroundColor: color.ToNSColor(), paragraphStyle: new NSMutableParagraphStyle() { Alignment = NSTextAlignment.Center });
				Control.AttributedTitle = textWithColor;
			}
		}

		void UpdateCheck()
		{
			Control.State = Element.IsChecked ? NSCellStateValue.On : NSCellStateValue.Off;
		}

		void HandleButtonPressed()
		{
			Element?.SendPressed();

			if (!Element.IsChecked)
				Element.IsChecked = !Element.IsChecked;
		}

		void HandleButtonReleased()
		{
			Element?.SendReleased();
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
