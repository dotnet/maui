using System;
using System.ComponentModel;

using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Entry;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class EntryRenderer : EntryRendererBase<UITextField>
	{
		[Preserve(Conditional = true)]
		public EntryRenderer()
		{
			Frame = new RectangleF(0, 20, 320, 40);
		}

		protected override UITextField CreateNativeControl()
		{
			var textField = new UITextField(RectangleF.Empty);
			textField.BorderStyle = UITextBorderStyle.RoundedRect;
			textField.ClipsToBounds = true;
			return textField;
		}
	}

	public abstract class EntryRendererBase<TControl> : ViewRenderer<Entry, TControl>
		where TControl : UITextField
	{
		UIColor _defaultTextColor;

		// Placeholder default color is 70% gray
		// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder
		readonly Color _defaultPlaceholderColor = ColorExtensions.SeventyPercentGrey.ToColor();
		UIColor _defaultCursorColor;
		bool _useLegacyColorManagement;

		bool _disposed;
		IDisposable _selectedTextRangeObserver;
		bool _nativeSelectionIsUpdating;

		bool _cursorPositionChangePending;
		bool _selectionLengthChangePending;

		static readonly int baseHeight = 30;
		static CGSize initialSize = CGSize.Empty;

		public EntryRendererBase()
		{
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var baseResult = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (Forms.IsiOS11OrNewer)
				return baseResult;

			NSString testString = new NSString("Tj");
			var testSize = testString.GetSizeUsingAttributes(new UIStringAttributes { Font = Control.Font });
			double height = baseHeight + testSize.Height - initialSize.Height;
			height = Math.Round(height);

			return new SizeRequest(new Size(baseResult.Request.Width, height));
		}

		IElementController ElementController => Element as IElementController;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_defaultTextColor = null;

				if (Control != null)
				{
					_defaultCursorColor = Control.TintColor;
					Control.EditingDidBegin -= OnEditingBegan;
					Control.EditingChanged -= OnEditingChanged;
					Control.EditingDidEnd -= OnEditingEnded;
					Control.ShouldChangeCharacters -= ShouldChangeCharacters;
					_selectedTextRangeObserver?.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		abstract protected override TControl CreateNativeControl();

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
			{
				var textField = CreateNativeControl();
				SetNativeControl(textField);

				// Cache the default text color
				_defaultTextColor = textField.TextColor;

				_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();


				textField.EditingChanged += OnEditingChanged;
				textField.ShouldReturn = OnShouldReturn;

				textField.EditingDidBegin += OnEditingBegan;
				textField.EditingDidEnd += OnEditingEnded;
				textField.ShouldChangeCharacters += ShouldChangeCharacters;
				_selectedTextRangeObserver = textField.AddObserver("selectedTextRange", NSKeyValueObservingOptions.New, UpdateCursorFromControl);
			}

			// When we set the control text, it triggers the UpdateCursorFromControl event, which updates CursorPosition and SelectionLength;
			// These one-time-use variables will let us initialize a CursorPosition and SelectionLength via ctor/xaml/etc.
			_cursorPositionChangePending = Element.IsSet(Entry.CursorPositionProperty);
			_selectionLengthChangePending = Element.IsSet(Entry.SelectionLengthProperty);

			// Font needs to be set before Text and Placeholder so that they layout correctly when set
			UpdateFont();
			UpdatePlaceholder();
			UpdatePassword();
			UpdateText();
			UpdateCharacterSpacing();
			UpdateColor();
			UpdateKeyboard();
			UpdateHorizontalTextAlignment();
			UpdateVerticalTextAlignment();
			UpdateAdjustsFontSizeToFitWidth();
			UpdateMaxLength();
			UpdateReturnType();

			if (_cursorPositionChangePending || _selectionLengthChangePending)
				UpdateCursorSelection();

			UpdateCursorColor();
			UpdateIsReadOnly();

			if (Element.ClearButtonVisibility != ClearButtonVisibility.Never)
				UpdateClearButtonVisibility();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName || e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdatePassword();
			else if (e.IsOneOf(Entry.TextProperty, Entry.TextTransformProperty))
			{
				UpdateText();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Entry.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Entry.IsTextPredictionEnabledProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Entry.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
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
			else if (e.PropertyName == Specifics.AdjustsFontSizeToFitWidthProperty.PropertyName)
				UpdateAdjustsFontSizeToFitWidth();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Entry.ReturnTypeProperty.PropertyName)
				UpdateReturnType();
			else if (e.PropertyName == Entry.CursorPositionProperty.PropertyName)
				UpdateCursorSelection();
			else if (e.PropertyName == Entry.SelectionLengthProperty.PropertyName)
				UpdateCursorSelection();
			else if (e.PropertyName == Specifics.CursorColorProperty.PropertyName)
				UpdateCursorColor();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();
			else if (e.PropertyName == Entry.ClearButtonVisibilityProperty.PropertyName)
				UpdateClearButtonVisibility();

			base.OnElementPropertyChanged(sender, e);
		}

		void OnEditingBegan(object sender, EventArgs e)
		{
			if (!_cursorPositionChangePending && !_selectionLengthChangePending)
				UpdateCursorFromControl(null);
			else
				UpdateCursorSelection();

			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		[PortHandler("Ported Text setter")]
		void OnEditingChanged(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(Entry.TextProperty, Control.Text);
			UpdateCursorFromControl(null);
		}

		[PortHandler("Ported Text setter")]
		void OnEditingEnded(object sender, EventArgs e)
		{
			// Typing aid changes don't always raise EditingChanged event

			// Normalizing nulls to string.Empty allows us to ensure that a change from null to "" doesn't result in a change event.
			// While technically this is a difference it serves no functional good.
			var controlText = Control.Text ?? string.Empty;
			var entryText = Element.Text ?? string.Empty;
			if (controlText != entryText)
			{
				ElementController.SetValueFromRenderer(Entry.TextProperty, controlText);
			}

			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		protected virtual bool OnShouldReturn(UITextField view)
		{
			Control.ResignFirstResponder();
			((IEntryController)Element).SendCompleted();

			if (Element != null && Element.ReturnType == ReturnType.Next)
			{
				FocusSearch(true);
			}

			return false;
		}

		[PortHandler]
		void UpdateHorizontalTextAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalAlignment = Element.VerticalTextAlignment.ToNativeTextAlignment();
		}

		[PortHandler]
		protected virtual void UpdateColor()
		{
			var textColor = Element.TextColor;

			if (_useLegacyColorManagement)
			{
				Control.TextColor = textColor.IsDefault || !Element.IsEnabled ? _defaultTextColor : textColor.ToUIColor();
			}
			else
			{
				Control.TextColor = textColor.IsDefault ? _defaultTextColor : textColor.ToUIColor();
			}
		}

		void UpdateAdjustsFontSizeToFitWidth()
		{
			Control.AdjustsFontSizeToFitWidth = Element.OnThisPlatform().AdjustsFontSizeToFitWidth();
		}

		[PortHandler]
		protected virtual void UpdateFont()
		{
			if (initialSize == CGSize.Empty)
			{
				NSString testString = new NSString("Tj");
				initialSize = testString.StringSize(Control.Font);
			}

			Control.Font = Element.ToUIFont();
		}

		void UpdateKeyboard()
		{
			var keyboard = Element.Keyboard;
			Control.ApplyKeyboard(keyboard);
			if (!(keyboard is CustomKeyboard))
			{
				if (Element.IsSet(Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty))
				{
					if (!Element.IsSpellCheckEnabled)
					{
						Control.SpellCheckingType = UITextSpellCheckingType.No;
					}
				}
				if (Element.IsSet(Microsoft.Maui.Controls.Entry.IsTextPredictionEnabledProperty))
				{
					if (!Element.IsTextPredictionEnabled)
					{
						Control.AutocorrectionType = UITextAutocorrectionType.No;
					}
				}
			}
			Control.ReloadInputViews();
		}

		[PortHandler]
		void UpdatePassword()
		{
			if (Element.IsPassword && Control.IsFirstResponder)
			{
				Control.Enabled = false;
				Control.SecureTextEntry = true;
				Control.Enabled = Element.IsEnabled;
				Control.BecomeFirstResponder();
			}
			else
				Control.SecureTextEntry = Element.IsPassword;
		}

		[PortHandler]
		protected virtual void UpdatePlaceholder()
		{
			var formatted = (FormattedString)Element.Placeholder;

			if (formatted == null)
				return;

			var targetColor = Element.PlaceholderColor;

			if (_useLegacyColorManagement)
			{
				var color = targetColor.IsDefault || !Element.IsEnabled ? _defaultPlaceholderColor : targetColor;
				UpdateAttributedPlaceholder(formatted.ToAttributed(Element, color));
			}
			else
			{
				// Using VSM color management; take whatever is in Element.PlaceholderColor
				var color = targetColor.IsDefault ? _defaultPlaceholderColor : targetColor;
				UpdateAttributedPlaceholder(formatted.ToAttributed(Element, color));
			}

			UpdateAttributedPlaceholder(Control.AttributedPlaceholder.AddCharacterSpacing(Element.Placeholder, Element.CharacterSpacing));
		}

		protected virtual void UpdateAttributedPlaceholder(NSAttributedString nsAttributedString) =>
			Control.AttributedPlaceholder = nsAttributedString;

		[PortHandler]
		void UpdateText()
		{
			var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			// ReSharper disable once RedundantCheckBeforeAssignment
			if (Control.Text != text)
				Control.Text = text;
		}

		[PortHandler ("Partially ported ...")]
		void UpdateCharacterSpacing()
		{
			var textAttr = Control.AttributedText.AddCharacterSpacing(Element.Text, Element.CharacterSpacing);

			if (textAttr != null)
				Control.AttributedText = textAttr;

			var placeHolder = Control.AttributedPlaceholder.AddCharacterSpacing(Element.Placeholder, Element.CharacterSpacing);

			if (placeHolder != null)
				UpdateAttributedPlaceholder(placeHolder);
		}

		[PortHandler]
		void UpdateMaxLength()
		{
			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		[PortHandler]
		bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
		{
			var newLength = textField?.Text?.Length + replacementString?.Length - range.Length;
			return newLength <= Element?.MaxLength;
		}

		[PortHandler]
		void UpdateReturnType()
		{
			if (Control == null || Element == null)
				return;
			Control.ReturnKeyType = Element.ReturnType.ToUIReturnKeyType();
		}

		void UpdateCursorFromControl(NSObservedChange obj)
		{
			if (_nativeSelectionIsUpdating || Control == null || Element == null)
				return;

			var currentSelection = Control.SelectedTextRange;
			if (currentSelection != null)
			{
				if (!_cursorPositionChangePending)
				{
					int newCursorPosition = (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, currentSelection.Start);
					if (newCursorPosition != Element.CursorPosition)
						SetCursorPositionFromRenderer(newCursorPosition);
				}

				if (!_selectionLengthChangePending)
				{
					int selectionLength = (int)Control.GetOffsetFromPosition(currentSelection.Start, currentSelection.End);

					if (selectionLength != Element.SelectionLength)
						SetSelectionLengthFromRenderer(selectionLength);
				}
			}
		}

		void UpdateCursorSelection()
		{
			if (_nativeSelectionIsUpdating || Control == null || Element == null)
				return;

			_cursorPositionChangePending = _selectionLengthChangePending = true;

			// If this is run from the ctor, the control is likely too early in its lifecycle to be first responder yet. 
			// Anything done here will have no effect, so we'll skip this work until later.
			// We'll try again when the control does become first responder later OnEditingBegan
			if (Control.BecomeFirstResponder())
			{
				try
				{
					int cursorPosition = Element.CursorPosition;

					UITextPosition start = GetSelectionStart(cursorPosition, out int startOffset);
					UITextPosition end = GetSelectionEnd(cursorPosition, start, startOffset);

					Control.SelectedTextRange = Control.GetTextRange(start, end);
				}
				catch (Exception ex)
				{
					Microsoft.Maui.Controls.Internals.Log.Warning("Entry", $"Failed to set Control.SelectedTextRange from CursorPosition/SelectionLength: {ex}");
				}
				finally
				{
					_cursorPositionChangePending = _selectionLengthChangePending = false;
				}
			}
		}

		UITextPosition GetSelectionEnd(int cursorPosition, UITextPosition start, int startOffset)
		{
			UITextPosition end = start;
			int endOffset = startOffset;
			int selectionLength = Element.SelectionLength;

			if (Element.IsSet(Entry.SelectionLengthProperty))
			{
				end = Control.GetPosition(start, Math.Max(startOffset, Math.Min(Control.Text.Length - cursorPosition, selectionLength))) ?? start;
				endOffset = Math.Max(startOffset, (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, end));
			}

			int newSelectionLength = Math.Max(0, endOffset - startOffset);
			if (newSelectionLength != selectionLength)
				SetSelectionLengthFromRenderer(newSelectionLength);

			return end;
		}

		UITextPosition GetSelectionStart(int cursorPosition, out int startOffset)
		{
			UITextPosition start = Control.EndOfDocument;
			startOffset = Control.Text.Length;

			if (Element.IsSet(Entry.CursorPositionProperty))
			{
				start = Control.GetPosition(Control.BeginningOfDocument, cursorPosition) ?? Control.EndOfDocument;
				startOffset = Math.Max(0, (int)Control.GetOffsetFromPosition(Control.BeginningOfDocument, start));
			}

			if (startOffset != cursorPosition)
				SetCursorPositionFromRenderer(startOffset);

			return start;
		}

		void UpdateCursorColor()
		{
			var control = Control;
			if (control == null || Element == null)
				return;

			if (Element.IsSet(Specifics.CursorColorProperty))
			{
				var color = Element.OnThisPlatform().GetCursorColor();
				if (color == Color.Default)
					control.TintColor = _defaultCursorColor;
				else
					control.TintColor = color.ToUIColor();
			}
		}

		void SetCursorPositionFromRenderer(int start)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				ElementController?.SetValueFromRenderer(Entry.CursorPositionProperty, start);
			}
			catch (Exception ex)
			{
				Controls.Internals.Log.Warning("Entry", $"Failed to set CursorPosition from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		void SetSelectionLengthFromRenderer(int selectionLength)
		{
			try
			{
				_nativeSelectionIsUpdating = true;
				ElementController?.SetValueFromRenderer(Entry.SelectionLengthProperty, selectionLength);
			}
			catch (Exception ex)
			{
				Controls.Internals.Log.Warning("Entry", $"Failed to set SelectionLength from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		[PortHandler]
		void UpdateIsReadOnly()
		{
			Control.UserInteractionEnabled = !Element.IsReadOnly;
		}

		[PortHandler]
		void UpdateClearButtonVisibility()
		{
			Control.ClearButtonMode = Element.ClearButtonVisibility == ClearButtonVisibility.WhileEditing ? UITextFieldViewMode.WhileEditing : UITextFieldViewMode.Never;
		}
	}
}
