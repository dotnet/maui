using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class EntryRenderer : ViewRenderer<Entry, NSTextField>
	{
		class FormsNSTextField : NSTextField
		{
			public EventHandler<BoolEventArgs> FocusChanged;

			public EventHandler Completed;

			bool _windowEventsSet;

			bool _disposed;

			public override bool ResignFirstResponder()
			{
				return base.ResignFirstResponder();
			}

			public override bool BecomeFirstResponder()
			{
				FocusChanged?.Invoke(this, new BoolEventArgs(true));

				var result = base.BecomeFirstResponder();

				if (!_windowEventsSet)
				{
					_windowEventsSet = true;
					Window.DidResignKey += HandleWindowDidResignKey;
					Window.DidBecomeKey += HandleWindowDidBecomeKey;
				}

				return result;
			}

			public override void DidEndEditing(NSNotification notification)
			{
				if (CurrentEditor != Window.FirstResponder)
					FocusChanged?.Invoke(this, new BoolEventArgs(false));

				base.DidEndEditing(notification);
			}

			public override void KeyUp(NSEvent theEvent)
			{
				base.KeyUp(theEvent);

				if (theEvent.KeyCode == (ushort)NSKey.Return)
					Completed?.Invoke(this, EventArgs.Empty);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !_disposed)
				{
					_disposed = true;

					if (Window != null)
					{
						Window.DidResignKey -= HandleWindowDidResignKey;
						Window.DidBecomeKey -= HandleWindowDidBecomeKey;
					}
				}

				base.Dispose(disposing);
			}

			void HandleWindowDidResignKey(object sender, EventArgs args)
			{
				if (!_disposed)
				{
					FocusChanged?.Invoke(this, new BoolEventArgs(false));
				}
			}

			void HandleWindowDidBecomeKey(object sender, EventArgs args)
			{
				if (!_disposed)
				{
					if (Window != null && CurrentEditor == Window.FirstResponder)
						FocusChanged?.Invoke(this, new BoolEventArgs(true));
				}
			}
		}

		bool _disposed;
		CGSize _previousSize;
		NSColor _defaultTextColor;

		IElementController ElementController => Element;

		IEntryController EntryController => Element;

		public override void Layout()
		{
			base.Layout();

			if (Element != null && _previousSize != Bounds.Size)
				SetBackground(Element.Background);

			_previousSize = Bounds.Size;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					CreateControl();
				}
				UpdateControl();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName ||
				e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdatePassword();
			else if (e.PropertyName == Entry.TextProperty.PropertyName ||
				e.PropertyName == Entry.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateColor();
				UpdatePlaceholder();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Xamarin.Forms.InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;
			Control.BackgroundColor = color == Color.Default ? ColorExtensions.ControlBackgroundColor : color.ToNSColor();

			base.SetBackgroundColor(color);
		}

		protected override void SetBackground(Brush brush)
		{
			if (Control == null)
				return;

			var backgroundImage = this.GetBackgroundImage(brush);
			Control.BackgroundColor = backgroundImage != null ? NSColor.FromPatternImage(backgroundImage) : NSColor.Clear;

			base.SetBackground(brush);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				ClearControl();
			}

			base.Dispose(disposing);
		}

		void CreateControl()
		{
			NSTextField textField;
			if (Element.IsPassword)
				textField = new NSSecureTextField();
			else
			{
				textField = new FormsNSTextField();
				(textField as FormsNSTextField).FocusChanged += TextFieldFocusChanged;
				(textField as FormsNSTextField).Completed += OnCompleted;
			}

			SetNativeControl(textField);

			_defaultTextColor = ColorExtensions.TextColor;

			textField.Changed += OnChanged;
			textField.EditingBegan += OnEditingBegan;
			textField.EditingEnded += OnEditingEnded;
		}

		void ClearControl()
		{
			if (Control != null)
			{
				Control.EditingBegan -= OnEditingBegan;
				Control.Changed -= OnChanged;
				Control.EditingEnded -= OnEditingEnded;
				var formsNSTextField = (Control as FormsNSTextField);
				if (formsNSTextField != null)
				{
					formsNSTextField.FocusChanged -= TextFieldFocusChanged;
					formsNSTextField.Completed -= OnCompleted;
				}
			}
		}

		void UpdateControl()
		{
			UpdatePlaceholder();
			UpdateText();
			UpdateColor();
			UpdateFont();
			UpdateAlignment();
			UpdateMaxLength();
			UpdateIsReadOnly();
		}

		void TextFieldFocusChanged(object sender, BoolEventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, e.Value);
		}

		void OnEditingBegan(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnChanged(object sender, EventArgs eventArgs)
		{
			UpdateMaxLength();

			ElementController.SetValueFromRenderer(Entry.TextProperty, Control.StringValue);
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnCompleted(object sender, EventArgs e)
		{
			EntryController?.SendCompleted();
		}

		void UpdateAlignment()
		{
			if (IsElementOrControlEmpty)
				return;

			Control.Alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateColor()
		{
			if (IsElementOrControlEmpty)
				return;

			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToNSColor();
		}

		void UpdatePassword()
		{
			ClearControl();
			CreateControl();
			UpdateControl();
			base.Layout();
		}

		void UpdateFont()
		{
			if (IsElementOrControlEmpty)
				return;

			Control.Font = Element.ToNSFont();
		}

		void UpdatePlaceholder()
		{
			if (IsElementOrControlEmpty)
				return;

			var formatted = (FormattedString)Element.Placeholder;

			if (formatted == null)
				return;

			var targetColor = Element.PlaceholderColor;

			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : ColorExtensions.PlaceholderColor.ToColor(NSColorSpace.DeviceRGBColorSpace);

			Control.PlaceholderAttributedString = formatted.ToAttributed(Element, color, Element.HorizontalTextAlignment);
		}

		protected override void SetAccessibilityLabel()
		{
			if (_disposed || IsElementOrControlEmpty)
				return;
			Control.AccessibilityLabel = (string)Element?.GetValue(AutomationProperties.NameProperty) ?? Control.PlaceholderAttributedString?.Value;
		}

		void UpdateText()
		{
			if (IsElementOrControlEmpty)
				return;

      var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);

			// ReSharper disable once RedundantCheckBeforeAssignment
			if (Control.StringValue != text)
				Control.StringValue = text;
		}

		void UpdateMaxLength()
		{
			if (IsElementOrControlEmpty)
				return;

			var currentControlText = Control?.StringValue;

			if (currentControlText.Length > Element?.MaxLength)
				Control.StringValue = currentControlText.Substring(0, Element.MaxLength);
		}


		void UpdateIsReadOnly()
		{
			if (IsElementOrControlEmpty)
				return;

			Control.Editable = !Element.IsReadOnly;
			if (Element.IsReadOnly && Control.Window?.FirstResponder == Control.CurrentEditor)
				Control.Window?.MakeFirstResponder(null);
		}
	}
}
