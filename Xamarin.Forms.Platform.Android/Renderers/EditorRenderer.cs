using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;

namespace Xamarin.Forms.Platform.Android
{
	public class EditorRenderer : EditorRendererBase<FormsEditText>
	{
		TextColorSwitcher _hintColorSwitcher;
		TextColorSwitcher _textColorSwitcher;

		public EditorRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use EditorRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public EditorRenderer()
		{
			AutoPackage = false;
		}

		protected override FormsEditText CreateNativeControl()
		{
			return new FormsEditText(Context)
			{
				ImeOptions = ImeAction.Done
			};
		}

		protected override EditText EditText => Control;

		protected override void UpdatePlaceholderColor()
		{
			_hintColorSwitcher = _hintColorSwitcher ?? new TextColorSwitcher(EditText.HintTextColors, Element.UseLegacyColorManagement());
			_hintColorSwitcher.UpdateTextColor(EditText, Element.PlaceholderColor, EditText.SetHintTextColor);
		}

		protected override void UpdateTextColor()
		{
			_textColorSwitcher = _textColorSwitcher ?? new TextColorSwitcher(EditText.TextColors, Element.UseLegacyColorManagement());
			_textColorSwitcher.UpdateTextColor(EditText, Element.TextColor);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (EditText.IsAlive() && EditText.Enabled)
			{
				// https://issuetracker.google.com/issues/37095917
				EditText.Enabled = false;
				EditText.Enabled = true;
			}
		}
	}

	public abstract class EditorRendererBase<TControl> : ViewRenderer<Editor, TControl>, ITextWatcher
		where TControl : global::Android.Views.View
	{
		bool _disposed;
		protected abstract EditText EditText { get; }

		public EditorRendererBase(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use EditorRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public EditorRendererBase()
		{
			AutoPackage = false;
		}

		IEditorController ElementController => Element;

		void ITextWatcher.AfterTextChanged(IEditable s)
		{
		}

		void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
		}

		void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			Internals.TextTransformUtilites.SetPlainText(Element, s?.ToString());
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			if (!e.Focus)
			{
				EditText.HideKeyboard();
			}

			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
			{
				// Post this to the main looper queue so it doesn't happen until the other focus stuff has resolved
				// Otherwise, ShowKeyboard will be called before this control is truly focused, and we will potentially
				// be displaying the wrong keyboard
				EditText?.PostShowKeyboard();
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			var edit = Control;
			if (edit == null)
			{
				edit = CreateNativeControl();

				SetNativeControl(edit);
				EditText.AddTextChangedListener(this);
				if (EditText is IFormsEditText formsEditText)
					formsEditText.OnKeyboardBackPressed += OnKeyboardBackPressed;
			}

			EditText.SetSingleLine(false);
			EditText.Gravity = GravityFlags.Top;
			if ((int)Forms.SdkInt > 16)
				EditText.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
			EditText.SetHorizontallyScrolling(false);

			UpdateText();
			UpdateInputType();
			UpdateTextColor();
			UpdateCharacterSpacing();
			UpdateFont();
			UpdateMaxLength();
			UpdatePlaceholderColor();
			UpdatePlaceholderText();
			UpdateIsReadOnly();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			if (e.PropertyName == Editor.TextProperty.PropertyName || e.PropertyName == Editor.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Editor.IsTextPredictionEnabledProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Editor.PlaceholderProperty.PropertyName)
				UpdatePlaceholderText();
			else if (e.PropertyName == Editor.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();

			base.OnElementPropertyChanged(sender, e);
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
				if (EditText != null && EditText is IFormsEditText formsEditText)
				{
					formsEditText.OnKeyboardBackPressed -= OnKeyboardBackPressed;
				}
			}

			base.Dispose(disposing);
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener 
			// or to filter out input types you don't want to allow 
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		internal override void OnNativeFocusChanged(bool hasFocus)
		{
			if (Element.IsFocused && !hasFocus) // Editor has requested an unfocus, fire completed event
				ElementController.SendCompleted();
		}

		protected virtual void UpdateFont()
		{
			EditText.Typeface = Element.ToTypeface();
			EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateInputType()
		{
			Editor model = Element;
			var edit = EditText;
			var keyboard = model.Keyboard;

			edit.InputType = keyboard.ToInputType() | InputTypes.TextFlagMultiLine;
			if (!(keyboard is Internals.CustomKeyboard))
			{
				if (model.IsSet(InputView.IsSpellCheckEnabledProperty))
				{
					if (!model.IsSpellCheckEnabled)
						edit.InputType = edit.InputType | InputTypes.TextFlagNoSuggestions;
				}
				if (model.IsSet(Editor.IsTextPredictionEnabledProperty))
				{
					if (!model.IsTextPredictionEnabled)
						edit.InputType = edit.InputType | InputTypes.TextFlagNoSuggestions;
				}
			}

			if (keyboard == Keyboard.Numeric)
			{
				edit.KeyListener = GetDigitsKeyListener(edit.InputType);
			}
		}

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				EditText.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		void UpdateText()
		{
			string newText = Element.UpdateFormsText(Element.Text, Element.TextTransform);

			if (EditText.Text == newText)
				return;

			newText = TrimToMaxLength(newText);
			EditText.Text = newText;
			EditText.SetSelection(newText.Length);
		}

		abstract protected void UpdateTextColor();

		protected virtual void UpdatePlaceholderText()
		{
			if (EditText.Hint == Element.Placeholder)
				return;

			EditText.Hint = Element.Placeholder;
		}

		abstract protected void UpdatePlaceholderColor();

		void OnKeyboardBackPressed(object sender, EventArgs eventArgs)
		{
			ElementController?.SendCompleted();
			EditText?.ClearFocus();
		}

		void UpdateMaxLength()
		{
			var currentFilters = new List<IInputFilter>(EditText?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(Element.MaxLength));

			if (EditText == null)
				return;

			EditText.SetFilters(currentFilters.ToArray());
			EditText.Text = TrimToMaxLength(EditText.Text);
		}

		string TrimToMaxLength(string currentText)
		{
			if (currentText == null || currentText.Length <= Element.MaxLength)
				return currentText;

			return currentText.Substring(0, Element.MaxLength);
		}

		void UpdateIsReadOnly()
		{
			bool isReadOnly = !Element.IsReadOnly;

			EditText.FocusableInTouchMode = isReadOnly;
			EditText.Focusable = isReadOnly;
			EditText.SetCursorVisible(isReadOnly);
		}
	}
}