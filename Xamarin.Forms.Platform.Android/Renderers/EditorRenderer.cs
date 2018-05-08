using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Java.Lang;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public class EditorRenderer : ViewRenderer<Editor, FormsEditText>, ITextWatcher
	{
		bool _disposed;
		TextColorSwitcher _textColorSwitcher;
		ColorStateList defaultPlaceholdercolor;

		public EditorRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use EditorRenderer(Context) instead.")]
		public EditorRenderer()
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
			if (string.IsNullOrEmpty(Element.Text) && s.Length() == 0)
				return;

			if (Element.Text != s.ToString())
				((IElementController)Element).SetValueFromRenderer(Editor.TextProperty, s.ToString());
		}

		protected override FormsEditText CreateNativeControl()
		{
			return new FormsEditText(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			HandleKeyboardOnFocus = true;

			var edit = Control;
			if (edit == null)
			{
				edit = CreateNativeControl();

				SetNativeControl(edit);
				edit.AddTextChangedListener(this);
				edit.OnKeyboardBackPressed += OnKeyboardBackPressed;

				var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
				_textColorSwitcher = new TextColorSwitcher(edit.TextColors, useLegacyColorManagement);

				defaultPlaceholdercolor = Control.HintTextColors;
			}

			edit.SetSingleLine(false);
			edit.Gravity = GravityFlags.Top;
			if ((int)Build.VERSION.SdkInt > 16)
				edit.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
			edit.SetHorizontallyScrolling(false);

			UpdateText();
			UpdateInputType();
			UpdateTextColor();
			UpdateFont();
			UpdateMaxLength();
			UpdatePlaceholderColor();
			UpdatePlaceholderText();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Editor.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
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
				if (Control != null)
				{
					Control.OnKeyboardBackPressed -= OnKeyboardBackPressed;
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

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateInputType()
		{
			Editor model = Element;
			FormsEditText edit = Control;
			var keyboard = model.Keyboard;

			edit.InputType = keyboard.ToInputType() | InputTypes.TextFlagMultiLine;
			if (!(keyboard is Internals.CustomKeyboard) && model.IsSet(InputView.IsSpellCheckEnabledProperty))
			{
				if ((edit.InputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
				{
					if (!model.IsSpellCheckEnabled)
						edit.InputType = edit.InputType | InputTypes.TextFlagNoSuggestions;
				}
			}

			if (keyboard == Keyboard.Numeric)
			{
				edit.KeyListener = GetDigitsKeyListener(edit.InputType);
			}
		}

		void UpdateText()
		{
			string newText = Element.Text ?? "";

			if (Control.Text == newText)
				return;

			Control.Text = newText;
			Control.SetSelection(newText.Length);
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}

		void UpdatePlaceholderText()
		{
			if (Control.Hint == Element.Placeholder)
				return;

			Control.Hint = Element.Placeholder;
		}

		void UpdatePlaceholderColor()
		{
			if (Element.PlaceholderColor == Color.Default)
				Control.SetHintTextColor(defaultPlaceholdercolor);
			else
				Control.SetHintTextColor(Element.PlaceholderColor.ToAndroid());
		}

		void OnKeyboardBackPressed(object sender, EventArgs eventArgs)
		{
			ElementController?.SendCompleted();
			Control?.ClearFocus();
		}

		void UpdateMaxLength()
		{
			var currentFilters = new List<IInputFilter>(Control?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(Element.MaxLength));

			Control?.SetFilters(currentFilters.ToArray());

			var currentControlText = Control?.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}
	}
}