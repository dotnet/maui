using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class EntryRenderer : ViewRenderer<Entry, NSTextField>
	{
		class BoolEventArgs : EventArgs
		{
			public BoolEventArgs(bool value)
			{
				Value = value;
			}
			public bool Value
			{
				get;
				private set;
			}
		}
		class FormsNSTextField : NSTextField
		{
			public EventHandler<BoolEventArgs> FocusChanged;
			public override bool ResignFirstResponder()
			{
				FocusChanged?.Invoke(this, new BoolEventArgs(false));
				return base.ResignFirstResponder();
			}
			public override bool BecomeFirstResponder()
			{
				FocusChanged?.Invoke(this, new BoolEventArgs(true));
				return base.BecomeFirstResponder();
			}
		}

		bool _disposed;
		NSColor _defaultTextColor;

		IElementController ElementController => Element;

		IEntryController EntryController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				NSTextField textField;
				if (e.NewElement.IsPassword)
					textField = new NSSecureTextField();
				else
				{
					textField = new FormsNSTextField();
					(textField as FormsNSTextField).FocusChanged += TextFieldFocusChanged;
				}

				SetNativeControl(textField);

				_defaultTextColor = textField.TextColor;

				textField.Changed += OnChanged;
				textField.EditingBegan += OnEditingBegan;
				textField.EditingEnded += OnEditingEnded;
			}

			if (e.NewElement != null)
			{
				UpdatePlaceholder();
				UpdateText();
				UpdateColor();
				UpdateFont();
				UpdateAlignment();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName ||
				e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdatePassword();
			else if (e.PropertyName == Entry.TextProperty.PropertyName)
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

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;
			Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			base.SetBackgroundColor(color);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
				{
					Control.EditingBegan -= OnEditingBegan;
					Control.Changed -= OnChanged;
					Control.EditingEnded -= OnEditingEnded;
					var formsNSTextField = (Control as FormsNSTextField);
					if (formsNSTextField != null)
						formsNSTextField.FocusChanged -= TextFieldFocusChanged;
				}
			}

			base.Dispose(disposing);
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
			ElementController.SetValueFromRenderer(Entry.TextProperty, Control.StringValue);
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			EntryController?.SendCompleted();
		}

		void UpdateAlignment()
		{
			Control.Alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToNSColor();
		}

		void UpdatePassword()
		{
			if (Element.IsPassword && (Control is NSSecureTextField))
				return;
			if (!Element.IsPassword && !(Control is NSSecureTextField))
				return;
		}

		void UpdateFont()
		{
			Control.Font = Element.ToNSFont();
		}

		void UpdatePlaceholder()
		{
			var formatted = (FormattedString)Element.Placeholder;

			if (formatted == null)
				return;

			var targetColor = Element.PlaceholderColor;

			// Placeholder default color is 70% gray
			// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder

			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : ColorExtensions.SeventyPercentGrey.ToColor();

			Control.PlaceholderAttributedString = formatted.ToAttributed(Element, color);
		}

		void UpdateText()
		{
			// ReSharper disable once RedundantCheckBeforeAssignment
			if (Control.StringValue != Element.Text)
				Control.StringValue = Element.Text ?? string.Empty;
		}
	}
}