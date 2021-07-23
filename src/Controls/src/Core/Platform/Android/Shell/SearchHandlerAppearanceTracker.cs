using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using AImageButton = Android.Widget.ImageButton;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Controls.Platform
{
	public class SearchHandlerAppearanceTracker : IDisposable
	{
		SearchHandler _searchHandler;
		bool _disposed;
		AView _control;
		AppCompatEditText _editText;
		InputTypes _inputType;
		IShellContext _shellContext;
		IMauiContext MauiContext => _shellContext.Shell.Handler.MauiContext;
		ColorStateList DefaultTextColors { get; set; }
		ColorStateList DefaultPlaceholderTextColors { get; set; }

		public SearchHandlerAppearanceTracker(IShellSearchView searchView, IShellContext shellContext)
		{
			_shellContext = shellContext;
			_searchHandler = searchView.SearchHandler;
			_control = searchView.View;
			_searchHandler.PropertyChanged += SearchHandlerPropertyChanged;
			_searchHandler.FocusChangeRequested += SearchHandlerFocusChangeRequested;
			_editText = (_control as ViewGroup).GetChildrenOfType<AppCompatEditText>().FirstOrDefault();
			DefaultTextColors = _editText.TextColors;
			DefaultPlaceholderTextColors = _editText.HintTextColors;
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
			else if (e.Is(SearchHandler.AutomationIdProperty))
			{
				UpdateAutomationId();
			}
		}

		void UpdateSearchBarColors()
		{
			UpdateBackgroundColor();
			UpdateTextColor();
			UpdateTextTransform();
			UpdatePlaceholderColor();
			UpdateCancelButtonColor();
			UpdateAutomationId();
		}

		void UpdateAutomationId()
		{
			AutomationPropertiesProvider
				.SetAutomationId(_editText, _searchHandler?.AutomationId);

		}

		void UpdateFont()
		{
			var fontManager = MauiContext.Services.GetRequiredService<IFontManager>();
			var font = Font.OfSize(_searchHandler.FontFamily, _searchHandler.FontSize).WithAttributes(_searchHandler.FontAttributes);

			_editText.Typeface = fontManager.GetTypeface(font);
			_editText.SetTextSize(ComplexUnitType.Sp, (float)_searchHandler.FontSize);
		}

		void UpdatePlaceholderColor()
		{
			_editText.UpdatePlaceholderColor(_searchHandler.PlaceholderColor, DefaultPlaceholderTextColors);			
		}

		void UpdateHorizontalTextAlignment()
		{
			_editText.UpdateHorizontalAlignment(_searchHandler.HorizontalTextAlignment, _control.Context.HasRtlSupport(), Microsoft.Maui.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateVerticalTextAlignment()
		{
			_editText.UpdateVerticalAlignment(_searchHandler.VerticalTextAlignment, Microsoft.Maui.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateTextTransform()
		{
			_editText.Text = _searchHandler.UpdateFormsText(_editText.Text, _searchHandler.TextTransform);
		}

		void UpdateBackgroundColor()
		{
			var linearLayout = (_control as ViewGroup).GetChildrenOfType<LinearLayout>().FirstOrDefault();
			linearLayout.SetBackgroundColor(_searchHandler.BackgroundColor.ToNative());
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
			_editText.UpdateTextColor(_searchHandler.TextColor, DefaultTextColors);
			UpdateImageButtonIconColor("SearchIcon", _searchHandler.TextColor);
			UpdateClearPlaceholderIconColor();
			//we need to set the cursor to
		}
		void UpdateImageButtonIconColor(string tagName, Color toColor)
		{
			var image = _control.FindViewWithTag(tagName) as AImageButton;
			if (image != null && image.Drawable != null)
			{
				if (toColor != null)
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
			if (!(keyboard is CustomKeyboard))
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