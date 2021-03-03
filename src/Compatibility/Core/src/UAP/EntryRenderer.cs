using System;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.InputView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class EntryRenderer : ViewRenderer<Entry, FormsTextBox>
	{
		bool _fontApplied;
		WBrush _backgroundColorFocusedDefaultBrush;
		WBrush _placeholderDefaultBrush;
		WBrush _textDefaultBrush;
		WBrush _defaultTextColorFocusBrush;
		WBrush _defaultPlaceholderColorFocusBrush;
		bool _cursorPositionChangePending;
		bool _selectionLengthChangePending;
		bool _nativeSelectionIsUpdating;
		string _transformedText;

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var textBox = new FormsTextBox { Style = Microsoft.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Microsoft.UI.Xaml.Style };

					SetNativeControl(textBox);
					textBox.TextChanged += OnNativeTextChanged;
					textBox.KeyUp += TextBoxOnKeyUp;
					textBox.SelectionChanged += SelectionChanged;
					textBox.GotFocus += TextBoxGotFocus;
					// If the Forms VisualStateManager is in play or the user wants to disable the Forms legacy
					// color stuff, then the underlying textbox should just use the Forms VSM states
					textBox.UseFormsVsm = e.NewElement.HasVisualStateGroups()
						|| !e.NewElement.OnThisPlatform().GetIsLegacyColorModeEnabled();
				}

				// When we set the control text, it triggers the SelectionChanged event, which updates CursorPosition and SelectionLength;
				// These one-time-use variables will let us initialize a CursorPosition and SelectionLength via ctor/xaml/etc.
				_cursorPositionChangePending = Element.IsSet(Entry.CursorPositionProperty);
				_selectionLengthChangePending = Element.IsSet(Entry.SelectionLengthProperty);

				UpdateIsPassword();
				UpdateText();
				UpdatePlaceholder();
				UpdateTextColor();
				UpdateFont();
				UpdateCharacterSpacing();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
				UpdatePlaceholderColor();
				UpdateMaxLength();
				UpdateDetectReadingOrderFromContent();
				UpdateReturnType();
				UpdateIsReadOnly();
				UpdateInputScope();
				UpdateClearButtonVisibility();



				if (_cursorPositionChangePending)
					UpdateCursorPosition();

				if (_selectionLengthChangePending)
					UpdateSelectionLength();
			}
		}

		void TextBoxGotFocus(object sender, RoutedEventArgs e)
		{
			if (_cursorPositionChangePending)
				UpdateCursorPosition();

			if (_selectionLengthChangePending)
				UpdateSelectionLength();

			SetCursorPositionFromRenderer(Control.SelectionStart);
			SetSelectionLengthFromRenderer(Control.SelectionLength);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TextChanged -= OnNativeTextChanged;
				Control.KeyUp -= TextBoxOnKeyUp;
				Control.SelectionChanged -= SelectionChanged;
				Control.GotFocus -= TextBoxGotFocus;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(Entry.TextProperty, Entry.TextTransformProperty))
				UpdateText();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateIsPassword();
			else if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Entry.CharacterSpacingProperty.PropertyName)
			{
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == Entry.IsTextPredictionEnabledProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Entry.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Specifics.DetectReadingOrderFromContentProperty.PropertyName)
				UpdateDetectReadingOrderFromContent();
			else if (e.PropertyName == Entry.ReturnTypeProperty.PropertyName)
				UpdateReturnType();
			else if (e.PropertyName == Entry.CursorPositionProperty.PropertyName)
				UpdateCursorPosition();
			else if (e.PropertyName == Entry.SelectionLengthProperty.PropertyName)
				UpdateSelectionLength();
			else if (e.PropertyName == InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();
			else if (e.PropertyName == Entry.ClearButtonVisibilityProperty.PropertyName)
				UpdateClearButtonVisibility();
		}

		protected override void UpdateBackgroundColor()
		{
			base.UpdateBackgroundColor();

			if (Control == null)
			{
				return;
			}

			// By default some platforms have alternate default background colors when focused
			BrushHelpers.UpdateColor(Element.BackgroundColor, ref _backgroundColorFocusedDefaultBrush,
				() => Control.BackgroundFocusBrush, brush => Control.BackgroundFocusBrush = brush);
		}

		void OnNativeTextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs args)
		{
			if (Control.Text == _transformedText)
				return;
			_transformedText = Element.UpdateFormsText(Control.Text, Element.TextTransform);
			Element.SetValueCore(Entry.TextProperty, _transformedText);
		}

		void TextBoxOnKeyUp(object sender, KeyRoutedEventArgs args)
		{
			if (args?.Key != VirtualKey.Enter)
				return;

			if (Element.ReturnType == ReturnType.Next)
			{
				FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
			}
			else
			{
				// Hide the soft keyboard; this matches the behavior of Forms on Android/iOS
				Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
			}

			((IEntryController)Element).SendCompleted();
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalContentAlignment = Element.VerticalTextAlignment.ToNativeVerticalAlignment();
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Entry entry = Element;

			if (entry == null)
				return;

			bool entryIsDefault = entry.FontFamily == null && entry.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Entry), true) && entry.FontAttributes == FontAttributes.None;

			if (entryIsDefault && !_fontApplied)
				return;

			if (entryIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				// Resharper wants to simplify 'FormsTextBox' to 'Control', but then it'll conflict with the property 'Control'
				Control.ClearValue(FormsTextBox.FontStyleProperty);
				Control.ClearValue(FormsTextBox.FontSizeProperty);
				Control.ClearValue(FormsTextBox.FontFamilyProperty);
				Control.ClearValue(FormsTextBox.FontWeightProperty);
				Control.ClearValue(FormsTextBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(entry);
			}

			_fontApplied = true;
		}

		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		void UpdateClearButtonVisibility()
		{
			Control.ClearButtonVisible = Element.ClearButtonVisibility == ClearButtonVisibility.WhileEditing;
		}

		void UpdateInputScope()
		{
			Entry entry = Element;
			if (entry.Keyboard is CustomKeyboard custom)
			{
				Control.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				Control.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				if (entry.IsSet(Entry.IsTextPredictionEnabledProperty))
					Control.IsTextPredictionEnabled = entry.IsTextPredictionEnabled;
				else
					Control.ClearValue(TextBox.IsTextPredictionEnabledProperty);
				if (entry.IsSet(InputView.IsSpellCheckEnabledProperty))
					Control.IsSpellCheckEnabled = entry.IsSpellCheckEnabled;
				else
					Control.ClearValue(TextBox.IsSpellCheckEnabledProperty);
			}

			Control.InputScope = entry.Keyboard.ToInputScope();
		}

		void UpdateIsPassword()
		{
			Control.IsPassword = Element.IsPassword;
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? "";
		}

		void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;

			BrushHelpers.UpdateColor(placeholderColor, ref _placeholderDefaultBrush,
				() => Control.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);

			BrushHelpers.UpdateColor(placeholderColor, ref _defaultPlaceholderColorFocusBrush,
				() => Control.PlaceholderForegroundFocusBrush, brush => Control.PlaceholderForegroundFocusBrush = brush);
		}

		void UpdateText()
		{
			Control.Text = _transformedText = Element.UpdateFormsText(Element.Text, Element.TextTransform);
		}

		void UpdateTextColor()
		{
			Color textColor = Element.TextColor;

			BrushHelpers.UpdateColor(textColor, ref _textDefaultBrush,
				() => Control.Foreground, brush => Control.Foreground = brush);

			BrushHelpers.UpdateColor(textColor, ref _defaultTextColorFocusBrush,
				() => Control.ForegroundFocusBrush, brush => Control.ForegroundFocusBrush = brush);
		}

		void UpdateMaxLength()
		{
			Control.MaxLength = Element.MaxLength;

			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		void UpdateDetectReadingOrderFromContent()
		{
			if (Element.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				if (Element.OnThisPlatform().GetDetectReadingOrderFromContent())
				{
					Control.TextReadingOrder = TextReadingOrder.DetectFromContent;
				}
				else
				{
					Control.TextReadingOrder = TextReadingOrder.UseFlowDirection;
				}
			}
		}

		void UpdateReturnType()
		{
			if (Control == null || Element == null)
				return;

			Control.InputScope = Element.ReturnType.ToInputScope();
		}

		void SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (_nativeSelectionIsUpdating || Control == null || Element == null)
				return;

			int cursorPosition = Element.CursorPosition;

			if (!_cursorPositionChangePending)
			{
				var start = cursorPosition;
				int selectionStart = Control.SelectionStart;
				if (selectionStart != start)
					SetCursorPositionFromRenderer(selectionStart);
			}

			if (!_selectionLengthChangePending)
			{
				int elementSelectionLength = Math.Min(Control.Text.Length - cursorPosition, Element.SelectionLength);

				int controlSelectionLength = Control.SelectionLength;
				if (controlSelectionLength != elementSelectionLength)
					SetSelectionLengthFromRenderer(controlSelectionLength);
			}
		}

		void UpdateSelectionLength()
		{
			if (_nativeSelectionIsUpdating || Control == null || Element == null)
				return;

			if (Control.Focus(FocusState.Programmatic))
			{
				try
				{
					int selectionLength = 0;
					int elemSelectionLength = Element.SelectionLength;

					if (Element.IsSet(Entry.SelectionLengthProperty))
						selectionLength = Math.Max(0, Math.Min(Control.Text.Length - Element.CursorPosition, elemSelectionLength));

					if (elemSelectionLength != selectionLength)
						SetSelectionLengthFromRenderer(selectionLength);

					Control.SelectionLength = selectionLength;
				}
				catch (Exception ex)
				{
					Log.Warning("Entry", $"Failed to set Control.SelectionLength from SelectionLength: {ex}");
				}
				finally
				{
					_selectionLengthChangePending = false;
				}
			}
		}

		void UpdateCursorPosition()
		{
			if (_nativeSelectionIsUpdating || Control == null || Element == null)
				return;

			if (Control.Focus(FocusState.Programmatic))
			{
				try
				{
					int start = Control.Text.Length;
					int cursorPosition = Element.CursorPosition;

					if (Element.IsSet(Entry.CursorPositionProperty))
						start = Math.Min(start, cursorPosition);

					if (start != cursorPosition)
						SetCursorPositionFromRenderer(start);

					Control.SelectionStart = start;

					// Length is dependent on start, so we'll need to update it
					UpdateSelectionLength();
				}
				catch (Exception ex)
				{
					Log.Warning("Entry", $"Failed to set Control.SelectionStart from CursorPosition: {ex}");
				}
				finally
				{
					_cursorPositionChangePending = false;
				}
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
				Log.Warning("Entry", $"Failed to set CursorPosition from renderer: {ex}");
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
				Log.Warning("Entry", $"Failed to set SelectionLength from renderer: {ex}");
			}
			finally
			{
				_nativeSelectionIsUpdating = false;
			}
		}

		void UpdateIsReadOnly()
		{
			Control.IsReadOnly = Element.IsReadOnly;
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			FormsTextBox child = Control;

			if (Children.Count == 0 || child == null)
				return new SizeRequest();

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);
            child.Measure(constraint);
			var result = FormsTextBox.GetCopyOfSize(child, constraint);
			return new SizeRequest(result);
		}
	}
}