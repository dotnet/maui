using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, SearchView>, SearchView.IOnQueryTextListener
	{
		EditText _editText;
		InputTypes _inputType;
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;
		float _defaultHeight => Context.ToPixels(42);
		bool _isDisposed;

		public SearchBarRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
		{
			Internals.TextTransformUtilites.SetPlainText(Element, newText);

			return true;
		}

		bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
		{
			((ISearchBarController)Element).OnSearchButtonPressed();
			ClearFocus(Control);
			return true;
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			var sizerequest = base.GetDesiredSize(widthConstraint, heightConstraint);
			if (Forms.SdkInt == BuildVersionCodes.N && heightConstraint == 0 && sizerequest.Request.Height == 0)
			{
				sizerequest.Request = new Size(sizerequest.Request.Width, _defaultHeight);
			}
			return sizerequest;
		}

		protected override SearchView CreateNativeControl()
		{
			var context = (Context as ContextThemeWrapper)?.BaseContext ?? Context;
			return new SearchView(context);
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
			var isDesigner = Context.IsDesignerContext();

			if (searchView == null)
			{
				searchView = CreateNativeControl();
				searchView.SetIconifiedByDefault(false);
				// set Iconified calls onSearchClicked 
				// https://github.com/aosp-mirror/platform_frameworks_base/blob/6d891937a38220b0c712a1927f969e74bea3a0f3/core/java/android/widget/SearchView.java#L674-L680
				// which causes requestFocus. The designer does not support focuses.
				if (!isDesigner)
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

			if (!isDesigner)
				ClearFocus(searchView);
			UpdateInputType();
			UpdatePlaceholder();
			UpdateText();
			UpdateEnabled();
			UpdateCancelButtonColor();
			UpdateFont();
			UpdateHorizontalTextAlignment();
			UpdateVerticalTextAlignment();
			UpdateTextColor();
			UpdateCharacterSpacing();
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
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.IsOneOf(SearchBar.TextProperty, SearchBar.TextTransformProperty))
				UpdateText();
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButtonColor();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEnabled();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == SearchBar.VerticalOptionsProperty.PropertyName)
				UpdateVerticalTextAlignment();
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputType();
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateInputType();
		}

		internal override void OnNativeFocusChanged(bool hasFocus)
		{
			if (hasFocus && !Element.IsEnabled)
				ClearFocus(Control);
		}

		[PortHandler]
		void UpdateHorizontalTextAlignment()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText == null)
				return;

			_editText.UpdateHorizontalAlignment(Element.HorizontalTextAlignment, Context.HasRtlSupport(), Microsoft.Maui.TextAlignment.Center.ToVerticalGravityFlags());
		}

		void UpdateVerticalTextAlignment()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText == null)
				return;

			_editText.UpdateVerticalAlignment(Element.VerticalTextAlignment, Microsoft.Maui.TextAlignment.Center.ToVerticalGravityFlags());
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
						image.Drawable.SetColorFilter(Element.CancelButtonColor, FilterMode.SrcIn);
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
			view.ClearFocus();
		}

		[PortHandler]
		void UpdateFont()
		{
			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText == null)
				return;

			_editText.Typeface = Element.ToTypeface();
			_editText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		[PortHandler]
		void UpdatePlaceholder()
		{
			Control.SetQueryHint(Element.Placeholder);
		}

		void UpdatePlaceholderColor()
		{
			_hintColorSwitcher?.UpdateTextColor(_editText, Element.PlaceholderColor, _editText.SetHintTextColor);
		}

		[PortHandler]
		void UpdateText()
		{
			string query = Control.Query;
			var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			if (query != text)
				Control.SetQuery(text, false);
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			if (!Forms.IsLollipopOrNewer)
				return;

			_editText = _editText ?? Control.GetChildrenOfType<EditText>().FirstOrDefault();

			if (_editText != null)
			{
				_editText.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
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
			if (!(keyboard is CustomKeyboard))
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
				if (_editText != null)
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

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				if (Control.IsAlive())
				{
					Control.SetOnQueryTextListener(null);
					Control.SetOnQueryTextFocusChangeListener(null);
				}
			}

			base.Dispose(disposing);
		}
	}
}
