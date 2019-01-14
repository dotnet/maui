using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, SearchView>, SearchView.IOnQueryTextListener
	{
		EditText _editText;
		InputTypes _inputType;
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;

		public SearchBarRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use SearchBarRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SearchBarRenderer()
		{
			AutoPackage = false;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
		{
			((IElementController)Element).SetValueFromRenderer(SearchBar.TextProperty, newText);

			return true;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
		{
			((ISearchBarController)Element).OnSearchButtonPressed();
			ClearFocus(Control);
			return true;
		}

		protected override SearchView CreateNativeControl()
		{
			return new SearchView(Context);
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			if (!e.Focus)
			{
				Control.HideKeyboard();
			}

			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
			{
				// Post this to the main looper queue so it doesn't happen until the other focus stuff has resolved
				// Otherwise, ShowKeyboard will be called before this control is truly focused, and we will potentially
				// be displaying the wrong keyboard
				Control?.PostShowKeyboard();
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			base.OnElementChanged(e);

			SearchView searchView = Control;

			if (searchView == null)
			{
				searchView = CreateNativeControl();
				searchView.SetIconifiedByDefault(false);
				searchView.Iconified = false;
				SetNativeControl(searchView);
				_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

				if (_editText != null)
				{
					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(_editText.TextColors, useLegacyColorManagement);
					_hintColorSwitcher = new TextColorSwitcher(_editText.HintTextColors, useLegacyColorManagement);
				}

			}

			ClearFocus(searchView);
			UpdateInputType();
			UpdatePlaceholder();
			UpdateText();
			UpdateEnabled();
			UpdateCancelButtonColor();
			UpdateFont();
			UpdateAlignment();
			UpdateTextColor();
			UpdatePlaceholderColor();
			UpdateMaxLength();

			if (e.OldElement == null)
			{
				searchView.SetOnQueryTextListener(this);
				searchView.SetOnQueryTextFocusChangeListener(this);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == SearchBar.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButtonColor();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if(e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if(e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputType();
		}

		internal override void OnNativeFocusChanged(bool hasFocus)
		{
			if (hasFocus && !Element.IsEnabled)
				ClearFocus(Control);
		}

		void UpdateAlignment()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText == null)
				return;

			_editText.UpdateHorizontalAlignment(Element.HorizontalTextAlignment, Context.HasRtlSupport(), Xamarin.Forms.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateCancelButtonColor()
		{
			int searchViewCloseButtonId = Control.Resources.GetIdentifier("android:id/search_close_btn", null, null);
			if (searchViewCloseButtonId != 0)
			{
				var image = FindViewById<ImageView>(searchViewCloseButtonId);
				if (image != null && image.Drawable != null)
				{
					if (Element.CancelButtonColor != Color.Default)
						image.Drawable.SetColorFilter(Element.CancelButtonColor.ToAndroid(), PorterDuff.Mode.SrcIn);
					else
						image.Drawable.ClearColorFilter();
				}
			}
		}

		void UpdateEnabled()
		{
			SearchBar model = Element;
			SearchView control = Control;
			if (!model.IsEnabled)
			{
				ClearFocus(control);
				// removes cursor in SearchView
				control.SetInputType(InputTypes.Null);
			}
			else
				control.SetInputType(_inputType);

			if (_editText != null)
			{
				_editText.Enabled = model.IsEnabled;
			}
		}

		void ClearFocus(SearchView view)
		{
			try
			{
				view.ClearFocus();
			}
			catch (Java.Lang.UnsupportedOperationException)
			{
				// silently catch these as they happen in the previewer due to some bugs in Android
			}
		}

		void UpdateFont()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText == null)
				return;

			_editText.Typeface = Element.ToTypeface();
			_editText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdatePlaceholder()
		{
			Control.SetQueryHint(Element.Placeholder);
		}

		void UpdatePlaceholderColor()
		{
			_hintColorSwitcher?.UpdateTextColor(_editText, Element.PlaceholderColor, _editText.SetHintTextColor);
		}

		void UpdateText()
		{
			string query = Control.Query;
			if (query != Element.Text)
				Control.SetQuery(Element.Text, false);
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(_editText, Element.TextColor);
		}

		void UpdateMaxLength()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			var currentFilters = new List<IInputFilter>(_editText?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(Element.MaxLength));

			_editText?.SetFilters(currentFilters.ToArray());

			var currentControlText = Control.Query;

			if (currentControlText.Length > Element.MaxLength)
				Control.SetQuery(currentControlText.Substring(0, Element.MaxLength), false);
		}

		void UpdateInputType()
		{
			SearchBar model = Element;
			var keyboard = model.Keyboard;

			_inputType = keyboard.ToInputType();
			if (!(keyboard is Internals.CustomKeyboard))
			{
				if (model.IsSet(InputView.IsSpellCheckEnabledProperty))
				{
					if ((_inputType & InputTypes.TextFlagNoSuggestions) != InputTypes.TextFlagNoSuggestions)
					{
						if (!model.IsSpellCheckEnabled)
							_inputType = _inputType | InputTypes.TextFlagNoSuggestions;
					}
				}
			}
			Control.SetInputType(_inputType);

			if (keyboard == Keyboard.Numeric)
			{
				_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();
				if(_editText != null)
					_editText.KeyListener = GetDigitsKeyListener(_inputType);
			}
		}

		protected virtual NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Override this in a custom renderer to use a different NumberKeyListener
			// or to filter out input types you don't want to allow
			// (e.g., inputTypes &= ~InputTypes.NumberFlagSigned to disallow the sign)
			return LocalizedDigitsKeyListener.Create(inputTypes);
		}
	}
}