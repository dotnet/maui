using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms.Internals;
using static System.String;
using WControl = System.Windows.Controls.Control;

namespace Xamarin.Forms.Platform.WinPhone
{
	public static class KeyboardExtensions
	{
		public static InputScope ToInputScope(this Keyboard self)
		{
			var result = new InputScope();
			var name = new InputScopeName();
			if (self == Keyboard.Default)
				name.NameValue = InputScopeNameValue.Default;
			else if (self == Keyboard.Chat)
				name.NameValue = InputScopeNameValue.Chat;
			else if (self == Keyboard.Email)
				name.NameValue = InputScopeNameValue.EmailNameOrAddress;
			else if (self == Keyboard.Numeric)
				name.NameValue = InputScopeNameValue.Number;
			else if (self == Keyboard.Telephone)
				name.NameValue = InputScopeNameValue.TelephoneNumber;
			else if (self == Keyboard.Text)
				name.NameValue = InputScopeNameValue.Text;
			else if (self == Keyboard.Url)
				name.NameValue = InputScopeNameValue.Url;
			else if (self is CustomKeyboard)
			{
				var custom = (CustomKeyboard)self;
				bool capitalizedSentenceEnabled = (custom.Flags & KeyboardFlags.CapitalizeSentence) == KeyboardFlags.CapitalizeSentence;
				bool spellcheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) == KeyboardFlags.Spellcheck;
				bool suggestionsEnabled = (custom.Flags & KeyboardFlags.Suggestions) == KeyboardFlags.Suggestions;

				if (!capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Default;
				if (!capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Search;
				if (!capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if spellcheck is enabled");
					name.NameValue = InputScopeNameValue.Search;
				}
				if (!capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Search;
				if (capitalizedSentenceEnabled && !spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if auto Capitalization is enabled");
					name.NameValue = InputScopeNameValue.Chat;
				}
				if (capitalizedSentenceEnabled && !spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Chat;
				if (capitalizedSentenceEnabled && spellcheckEnabled && !suggestionsEnabled)
				{
					Debug.WriteLine("Keyboard: Suggestions cannot be disabled in Windows Phone if spellcheck is enabled");
					name.NameValue = InputScopeNameValue.Text;
				}
				if (capitalizedSentenceEnabled && spellcheckEnabled && suggestionsEnabled)
					name.NameValue = InputScopeNameValue.Text;
			}
			else
			{
				// Should never happens
				name.NameValue = InputScopeNameValue.Default;
			}
			result.Names.Add(name);
			return result;
		}
	}

	public class EntryRenderer : ViewRenderer<Entry, FormsPhoneTextBox>
	{
		bool _fontApplied;
		bool _ignoreTextChange;
		Brush _placeholderDefaultBrush;
		Brush _textDefaultBrush;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Children.Count == 0)
				return new SizeRequest();

			var constraint = new System.Windows.Size(widthConstraint, heightConstraint);

			FormsPhoneTextBox child = Control;

			double oldWidth = child.Width;
			double oldHeight = child.Height;

			child.Height = double.NaN;
			child.Width = double.NaN;

			child.Measure(constraint);
			var result = new Size(Math.Ceiling(child.DesiredSize.Width), Math.Ceiling(child.DesiredSize.Height));

			child.Width = oldWidth;
			child.Height = oldHeight;

			return new SizeRequest(result);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			var textBox = new FormsPhoneTextBox();

			SetNativeControl(textBox);

			UpdateInputScope();
			UpdateIsPassword();
			UpdateText();
			UpdatePlaceholder();
			UpdateColor();
			UpdateFont();
			UpdateAlignment();
			UpdatePlaceholderColor();
			UpdateIsEnabled();

			Control.LostFocus += OnTextBoxUnfocused;
			Control.TextChanged += TextBoxOnTextChanged;
			Control.KeyUp += TextBoxOnKeyUp;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Entry.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateIsPassword();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
		}

		protected override void UpdateBackgroundColor()
		{
			Control.Background = Element.BackgroundColor.IsDefault ? (Brush)System.Windows.Application.Current.Resources["PhoneTextBoxBrush"] : Element.BackgroundColor.ToBrush();
		}

		internal override void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			if (args.Focus)
				args.Result = Control.Focus();
			else
			{
				UnfocusControl(Control);
				args.Result = true;
			}
		}

		void OnTextBoxUnfocused(object sender, RoutedEventArgs e)
		{
			if (Element.TextColor.IsDefault)
				return;

			if (!IsNullOrEmpty(Element.Text))
				Control.Foreground = Element.TextColor.ToBrush();
		}

		void TextBoxOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Enter)
				((IEntryController)Element).SendCompleted();
		}

		void TextBoxOnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs textChangedEventArgs)
		{
			// Signal to the UpdateText method that the change to TextProperty doesn't need to update the control
			// This prevents the cursor position from getting lost
			_ignoreTextChange = true;
			((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, Control.Text);

			// If an Entry.TextChanged handler modified the value of the Entry's text, the values could now be 
			// out-of-sync; re-sync them and force the TextBox cursor to the end of the text
			string entryText = Element.Text;
			if (Control.Text != entryText)
			{
				Control.Text = entryText;
				Control.SelectionStart = Control.Text.Length;
			}

			_ignoreTextChange = false;
		}

		void UpdateAlignment()
		{
			if (Control == null)
				return;

			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateColor()
		{
			if (Control == null)
				return;

			BrushHelpers.UpdateColor(Element.TextColor, ref _textDefaultBrush,
				() => Control.Foreground, brush => Control.Foreground = brush);

			// Force the PhoneTextBox control to do some internal bookkeeping
			// so the colors change immediately and remain changed when the control gets focus
			Control.OnApplyTemplate();
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
				Control.ClearValue(WControl.FontStyleProperty);
				Control.ClearValue(WControl.FontSizeProperty);
				Control.ClearValue(WControl.FontFamilyProperty);
				Control.ClearValue(WControl.FontWeightProperty);
				Control.ClearValue(WControl.FontStretchProperty);
			}
			else
				Control.ApplyFont(entry);

			_fontApplied = true;
		}

		void UpdateInputScope()
		{
			Control.InputScope = Element.Keyboard.ToInputScope();
		}

		void UpdateIsEnabled()
		{
			Control.IsEnabled = Element.IsEnabled;
		}

		void UpdateIsPassword()
		{
			Control.IsPassword = Element.IsPassword;
		}

		void UpdatePlaceholder()
		{
			Control.Hint = Element.Placeholder ?? "";
		}

		void UpdatePlaceholderColor()
		{
			BrushHelpers.UpdateColor(Element.PlaceholderColor, ref _placeholderDefaultBrush, 
				() => Control.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);
		}

		void UpdateText()
		{
			// If the text property has changed because TextBoxOnTextChanged called SetValueFromRenderer,
			// we don't want to re-update the text and reset the cursor position
			if (_ignoreTextChange)
				return;

			if (Control.Text == Element.Text)
				return;

			Control.Text = Element.Text ?? "";
			Control.Select(Control.Text.Length, 0);
		}
	}
}