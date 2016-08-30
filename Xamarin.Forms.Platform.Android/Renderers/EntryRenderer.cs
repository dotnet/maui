using System.ComponentModel;
using Android.Content.Res;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;

namespace Xamarin.Forms.Platform.Android
{
	public class EntryRenderer : ViewRenderer<Entry, EntryEditText>, ITextWatcher, TextView.IOnEditorActionListener
	{
		ColorStateList _hintTextColorDefault;
		ColorStateList _textColorDefault;
		EntryEditText _textView;

		public EntryRenderer()
		{
			AutoPackage = false;
		}

		bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			// Fire Completed and dismiss keyboard for hardware / physical keyboards
			if (actionId == ImeAction.Done || (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter))
			{
				Control.ClearFocus();
				v.HideKeyboard();
				((IEntryController)Element).SendCompleted();
			}

			return true;
		}

		void ITextWatcher.AfterTextChanged(IEditable s)
		{
		}

		void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
		}

		void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
		{
			if (string.IsNullOrEmpty(Element.Text) && s.Length() == 0)
				return;

			((IElementController)Element).SetValueFromRenderer(Entry.TextProperty, s.ToString());
		}

		protected override EntryEditText CreateNativeControl()
		{
			return new EntryEditText(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			HandleKeyboardOnFocus = true;

			if (e.OldElement == null)
			{
				_textView = CreateNativeControl();
				_textView.ImeOptions = ImeAction.Done;
				_textView.AddTextChangedListener(this);
				_textView.SetOnEditorActionListener(this);
				_textView.OnKeyboardBackPressed += (sender, args) => _textView.ClearFocus();
				SetNativeControl(_textView);
			}

			_textView.Hint = Element.Placeholder;
			_textView.Text = Element.Text;
			UpdateInputType();

			UpdateColor();
			UpdateAlignment();
			UpdateFont();
			UpdatePlaceholderColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				Control.Hint = Element.Placeholder;
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Entry.TextProperty.PropertyName)
			{
				if (Control.Text != Element.Text)
				{
					Control.Text = Element.Text;
					if (Control.IsFocused)
					{
						Control.SetSelection(Control.Text.Length);
						Control.ShowKeyboard();
					}
				}
			}
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();

			base.OnElementPropertyChanged(sender, e);
		}

		void UpdateAlignment()
		{
			Control.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags();
		}

		void UpdateColor()
		{
			if (Element.TextColor.IsDefault)
			{
				if (_textColorDefault == null)
				{
					// This control has always had the default colors; nothing to update
					return;
				}

				// This control is being set back to the default colors
				Control.SetTextColor(_textColorDefault);
			}
			else
			{
				if (_textColorDefault == null)
				{
					// Keep track of the default colors so we can return to them later
					// and so we can preserve the default disabled color
					_textColorDefault = Control.TextColors;
				}

				Control.SetTextColor(Element.TextColor.ToAndroidPreserveDisabled(_textColorDefault));
			}
		}

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateInputType()
		{
			Entry model = Element;
			_textView.InputType = model.Keyboard.ToInputType();
			if (model.IsPassword && ((_textView.InputType & InputTypes.ClassText) == InputTypes.ClassText))
				_textView.InputType = _textView.InputType | InputTypes.TextVariationPassword;
			if (model.IsPassword && ((_textView.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
				_textView.InputType = _textView.InputType | InputTypes.NumberVariationPassword;
		}

		void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;

			if (placeholderColor.IsDefault)
			{
				if (_hintTextColorDefault == null)
				{
					// This control has always had the default colors; nothing to update
					return;
				}

				// This control is being set back to the default colors
				Control.SetHintTextColor(_hintTextColorDefault);
			}
			else
			{
				if (_hintTextColorDefault == null)
				{
					// Keep track of the default colors so we can return to them later
					// and so we can preserve the default disabled color
					_hintTextColorDefault = Control.HintTextColors;
				}

				Control.SetHintTextColor(placeholderColor.ToAndroidPreserveDisabled(_hintTextColorDefault));
			}
		}
	}
}