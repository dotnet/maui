using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Internals;
using AImageButton = Android.Widget.ImageButton;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class SearchHandlerAppearanceTracker : IDisposable
	{
		SearchHandler _searchHandler;
		bool _disposed;
		AView _control;
		EditText _editText;
		InputTypes _inputType;
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;

		public SearchHandlerAppearanceTracker(IShellSearchView searchView)
		{
			_searchHandler = searchView.SearchHandler;
			_control = searchView.View;
			_searchHandler.PropertyChanged += SearchHandlerPropertyChanged;
			_searchHandler.FocusChangeRequested += SearchHandlerFocusChangeRequested;
			_editText = (_control as ViewGroup).GetChildrenOfType<EditText>().FirstOrDefault();
			_textColorSwitcher = new TextColorSwitcher(_editText.TextColors, false);
			_hintColorSwitcher = new TextColorSwitcher(_editText.HintTextColors, false);
			UpdateSearchBarColors();
			UpdateFont();
			UpdateHorizontalTextAlignment();
			UpdateVerticalTextAlignment();
			UpdateInputType();
		}

		protected virtual void SearchHandlerFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			e.Result = true;

			if (e.Focus)
			{
				_control?.RequestFocus();
				_control?.PostShowKeyboard();
			}
			else
			{
				_control.ClearFocus();
				_control.HideKeyboard();
			}
		}

		protected virtual void SearchHandlerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(SearchHandler.BackgroundColorProperty))
			{
				UpdateBackgroundColor();
			}
			else if (e.Is(SearchHandler.TextColorProperty))
			{
				UpdateTextColor();
			}
			else if (e.Is(SearchHandler.TextTransformProperty))
			{
				UpdateTextTransform();
			}
			else if (e.Is(SearchHandler.PlaceholderColorProperty))
			{
				UpdatePlaceholderColor();
			}
			else if (e.IsOneOf(SearchHandler.FontFamilyProperty, SearchHandler.FontAttributesProperty, SearchHandler.FontSizeProperty))
			{
				UpdateFont();
			}
			else if (e.Is(SearchHandler.CancelButtonColorProperty))
			{
				UpdateCancelButtonColor();
			}
			else if (e.Is(SearchHandler.KeyboardProperty))
			{
				UpdateInputType();
			}
			else if (e.Is(SearchHandler.HorizontalTextAlignmentProperty))
			{
				UpdateHorizontalTextAlignment();
			}
			else if (e.Is(SearchHandler.VerticalTextAlignmentProperty))
			{
				UpdateVerticalTextAlignment();
			}
		}

		void UpdateSearchBarColors()
		{
			UpdateBackgroundColor();
			UpdateTextColor();
			UpdateTextTransform();
			UpdatePlaceholderColor();
			UpdateCancelButtonColor();
		}

		void UpdateFont()
		{
			_editText.Typeface = _searchHandler.ToTypeface();
			_editText.SetTextSize(ComplexUnitType.Sp, (float)_searchHandler.FontSize);
		}

		void UpdatePlaceholderColor()
		{
			_hintColorSwitcher?.UpdateTextColor(_editText, _searchHandler.PlaceholderColor, _editText.SetHintTextColor);
		}

		void UpdateHorizontalTextAlignment()
		{
			_editText.UpdateHorizontalAlignment(_searchHandler.HorizontalTextAlignment, _control.Context.HasRtlSupport(), Xamarin.Forms.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateVerticalTextAlignment()
		{
			_editText.UpdateVerticalAlignment(_searchHandler.VerticalTextAlignment, Xamarin.Forms.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateTextTransform()
		{
			_editText.Text = _searchHandler.UpdateFormsText(_editText.Text, _searchHandler.TextTransform);
		}

		void UpdateBackgroundColor()
		{
			var linearLayout = (_control as ViewGroup).GetChildrenOfType<LinearLayout>().FirstOrDefault();
			linearLayout.SetBackgroundColor(_searchHandler.BackgroundColor.ToAndroid());
		}

		void UpdateCancelButtonColor()
		{
			//For now we are using the clear icon 
			//we should add a new cancel button that unfocus and hides keyboard
			UpdateClearIconColor();
		}

		void UpdateClearIconColor()
		{
			UpdateImageButtonIconColor(nameof(SearchHandler.ClearIcon), _searchHandler.CancelButtonColor);
		}

		void UpdateClearPlaceholderIconColor()
		{
			UpdateImageButtonIconColor(nameof(SearchHandler.ClearPlaceholderIcon), _searchHandler.TextColor);
		}

		void UpdateTextColor()
		{
			var textColor = _searchHandler.TextColor;
			_textColorSwitcher?.UpdateTextColor(_editText, textColor);
			UpdateImageButtonIconColor("SearchIcon", textColor);
			UpdateClearPlaceholderIconColor();
			//we need to set the cursor to
		}
		void UpdateImageButtonIconColor(string tagName, Color toColor)
		{
			var image = _control.FindViewWithTag(tagName) as AImageButton;
			if (image != null && image.Drawable != null)
			{
				if (!toColor.IsDefault)
					image.Drawable.SetColorFilter(toColor, FilterMode.SrcIn);
				else
					image.Drawable.ClearColorFilter();
			}
		}

		void UpdateInputType()
		{
			var keyboard = _searchHandler.Keyboard;

			_inputType = keyboard.ToInputType();
			bool isSpellCheckEnableSet = false;
			bool isSpellCheckEnable = false;
			// model.IsSet(InputView.IsSpellCheckEnabledProperty)
			if (!(keyboard is Internals.CustomKeyboard))
			{
				if (isSpellCheckEnableSet)
				{
					if ((_inputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!isSpellCheckEnable)
							_inputType = _inputType | InputTypes.TextFlagNoSuggestions;
					}
				}
			}
			_editText.InputType = _inputType;

			if (keyboard == Keyboard.Numeric)
			{
				_editText.KeyListener = GetDigitsKeyListener(_inputType);
			}
		}

		protected virtual global::Android.Text.Method.NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener
			// or to filter out input types you don't want to allow
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_searchHandler != null)
				{
					_searchHandler.FocusChangeRequested -= SearchHandlerFocusChangeRequested;
					_searchHandler.PropertyChanged -= SearchHandlerPropertyChanged;
				}
				_searchHandler = null;
				_control = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}