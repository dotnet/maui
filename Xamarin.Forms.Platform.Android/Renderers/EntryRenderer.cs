using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public class EntryRenderer : ViewRenderer<Entry, FormsEditText>, ITextWatcher, TextView.IOnEditorActionListener
	{
		TextColorSwitcher _hintColorSwitcher;
		TextColorSwitcher _textColorSwitcher;
		bool _disposed;

		public EntryRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use EntryRenderer(Context) instead.")]
		public EntryRenderer()
		{
			AutoPackage = false;
		}

		bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			// Fire Completed and dismiss keyboard for hardware / physical keyboards
			if (actionId == ImeAction.Done || (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Up))
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

		protected override FormsEditText CreateNativeControl()
		{
			return new FormsEditText(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			HandleKeyboardOnFocus = true;

			if (e.OldElement == null)
			{
				var textView = CreateNativeControl();
				textView.ImeOptions = ImeAction.Done;
				textView.AddTextChangedListener(this);
				textView.SetOnEditorActionListener(this);
				textView.OnKeyboardBackPressed += OnKeyboardBackPressed;

				var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();

				_textColorSwitcher = new TextColorSwitcher(textView.TextColors, useLegacyColorManagement);
				_hintColorSwitcher = new TextColorSwitcher(textView.HintTextColors, useLegacyColorManagement);

				

				SetNativeControl(textView);
			}

			Control.Hint = Element.Placeholder;
			Control.Text = Element.Text;
			UpdateInputType();

			UpdateColor();
			UpdateAlignment();
			UpdateFont();
			UpdatePlaceholderColor();
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
				if (Control != null)
				{
					Control.OnKeyboardBackPressed -= OnKeyboardBackPressed;
				}
			}

			base.Dispose(disposing);
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
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();

			base.OnElementPropertyChanged(sender, e);
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener 
			// or to filter out input types you don't want to allow 
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		void UpdateAlignment()
		{
			Control.UpdateHorizontalAlignment(Element.HorizontalTextAlignment);
		}

		void UpdateColor()
		{
			_textColorSwitcher.UpdateTextColor(Control, Element.TextColor);
		}

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateInputType()
		{
			Entry model = Element;
			var keyboard = model.Keyboard;

			Control.InputType = keyboard.ToInputType();

			if (keyboard == Keyboard.Numeric)
			{
				Control.KeyListener = GetDigitsKeyListener(Control.InputType);
			}

			if (model.IsPassword && ((Control.InputType & InputTypes.ClassText) == InputTypes.ClassText))
				Control.InputType = Control.InputType | InputTypes.TextVariationPassword;
			if (model.IsPassword && ((Control.InputType & InputTypes.ClassNumber) == InputTypes.ClassNumber))
				Control.InputType = Control.InputType | InputTypes.NumberVariationPassword;
		}

		void UpdatePlaceholderColor()
		{
			_hintColorSwitcher.UpdateTextColor(Control, Element.PlaceholderColor, Control.SetHintTextColor);
		}

		void OnKeyboardBackPressed(object sender, EventArgs eventArgs)
		{
			Control?.ClearFocus();
		}
	}
}