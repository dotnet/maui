#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class SearchHandlerAppearanceTracker : IDisposable
	{
		IFontManager _fontManager;
		UIColor _cancelButtonTextColorDefaultDisabled;
		UIColor _cancelButtonTextColorDefaultHighlighted;
		UIColor _cancelButtonTextColorDefaultNormal;
		UIColor _defaultTextColor;
		UIColor _defaultTintColor;
		UIColor _defaultClearIconTintColor;
		UIColor _defaultPlaceholderTintColor;
		bool _hasCustomBackground;
		UIColor _defaultBackgroundColor;
		SearchHandler _searchHandler;
		UISearchBar _uiSearchBar;
		UIToolbar _numericAccessoryView;
		bool _disposed;

		public SearchHandlerAppearanceTracker(UISearchBar searchBar, SearchHandler searchHandler, IFontManager fontManager)
		{
			_fontManager = fontManager;
			_searchHandler = searchHandler;
			_searchHandler.PropertyChanged += SearchHandlerPropertyChanged;
			_searchHandler.ShowSoftInputRequested += OnShowSoftInputRequested;
			_searchHandler.HideSoftInputRequested += OnHideSoftInputRequested;
			_searchHandler.FocusChangeRequested += SearchHandlerFocusChangeRequested;
			_uiSearchBar = searchBar;
			_uiSearchBar.OnEditingStarted += OnEditingStarted;
			_uiSearchBar.OnEditingStopped += OnEditingEnded;
			_uiSearchBar.TextChanged += OnTextChanged;
			_uiSearchBar.ShowsCancelButton = false;
			GetDefaultSearchBarColors(_uiSearchBar);
			var uiTextField = searchBar.FindDescendantView<UITextField>();
			UpdateSearchBarColors();
			UpdateSearchBarHorizontalTextAlignment(uiTextField);
			UpdateSearchBarVerticalTextAlignment(uiTextField);
			UpdateFont(uiTextField);
			UpdateKeyboard();
		}

		public void UpdateSearchBarColors()
		{
			var cancelButton = _uiSearchBar.FindDescendantView<UIButton>();
			var uiTextField = _uiSearchBar.FindDescendantView<UITextField>();

			UpdateTextColor(uiTextField);
			UpdateSearchBarPlaceholder(uiTextField);
			UpdateCancelButtonColor(cancelButton);
			UpdateSearchBarBackgroundColor(uiTextField);
			UpdateTextTransform(uiTextField);
		}

		internal void UpdateFlowDirection(Shell shell)
		{
			_uiSearchBar.UpdateFlowDirection(shell);

			// This UIToolbar variable is only initialized in case the platform is a Phone.
			_numericAccessoryView?.UpdateFlowDirection(shell);

			var uiTextField = _uiSearchBar.FindDescendantView<UITextField>();
			UpdateSearchBarHorizontalTextAlignment(uiTextField, shell);
		}

		void SearchHandlerFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			if (e.Focus)
				e.Result = _uiSearchBar.BecomeFirstResponder();
		}

		void SearchHandlerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(SearchHandler.BackgroundColorProperty))
			{
				UpdateSearchBarBackgroundColor(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.Is(SearchHandler.TextColorProperty))
			{
				UpdateTextColor(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.Is(SearchHandler.TextTransformProperty))
			{
				UpdateTextTransform(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.IsOneOf(SearchHandler.PlaceholderColorProperty, SearchHandler.PlaceholderProperty))
			{
				UpdateSearchBarPlaceholder(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.IsOneOf(SearchHandler.FontFamilyProperty, SearchHandler.FontAttributesProperty, SearchHandler.FontSizeProperty))
			{
				UpdateFont(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.Is(SearchHandler.CancelButtonColorProperty))
			{
				UpdateCancelButtonColor(_uiSearchBar.FindDescendantView<UIButton>());
			}
			else if (e.Is(SearchHandler.KeyboardProperty))
			{
				UpdateKeyboard();
			}
			else if (e.Is(SearchHandler.HorizontalTextAlignmentProperty))
			{
				UpdateSearchBarHorizontalTextAlignment(_uiSearchBar.FindDescendantView<UITextField>());
			}
			else if (e.Is(SearchHandler.VerticalTextAlignmentProperty))
			{
				UpdateSearchBarVerticalTextAlignment(_uiSearchBar.FindDescendantView<UITextField>());
			}
		}

		void GetDefaultSearchBarColors(UISearchBar searchBar)
		{
			_defaultTintColor = searchBar.BarTintColor;

			var cancelButton = searchBar.FindDescendantView<UIButton>();
			if (cancelButton != null)
			{
				_cancelButtonTextColorDefaultNormal = cancelButton.TitleColor(UIControlState.Normal);
				_cancelButtonTextColorDefaultHighlighted = cancelButton.TitleColor(UIControlState.Highlighted);
				_cancelButtonTextColorDefaultDisabled = cancelButton.TitleColor(UIControlState.Disabled);
			}
		}

		void UpdateFont(UITextField textField)
		{
			if (textField == null)
				return;


			textField.Font = _searchHandler.ToFont().ToUIFont(_fontManager);
		}

		void UpdateSearchBarBackgroundColor(UITextField textField)
		{
			if (textField == null)
				return;

			var backGroundColor = _searchHandler.BackgroundColor;

			if (!_hasCustomBackground && backGroundColor == null)
				return;

			var backgroundView = textField.Subviews[0];

			if (backGroundColor == null)
			{
				backgroundView.Layer.CornerRadius = 0;
				backgroundView.ClipsToBounds = false;
				backgroundView.BackgroundColor = _defaultBackgroundColor;
			}

			_hasCustomBackground = true;

			backgroundView.Layer.CornerRadius = 10;
			backgroundView.ClipsToBounds = true;
			if (_defaultBackgroundColor == null)
				_defaultBackgroundColor = backgroundView.BackgroundColor;

			UIColor backgroundColor = backGroundColor.ToPlatform();
			backgroundView.BackgroundColor = backgroundColor;
			textField.BackgroundColor = backgroundColor;
		}

		void UpdateCancelButtonColor(UIButton cancelButton)
		{
			if (cancelButton == null)
				return;

			var cancelColor = _searchHandler.CancelButtonColor;
			if (cancelColor == null)
			{
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultNormal, UIControlState.Normal);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultHighlighted, UIControlState.Highlighted);
				cancelButton.SetTitleColor(_cancelButtonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var cancelUIColor = cancelColor.ToPlatform();
				cancelButton.SetTitleColor(cancelUIColor, UIControlState.Normal);
				cancelButton.SetTitleColor(cancelUIColor, UIControlState.Highlighted);
				cancelButton.SetTitleColor(cancelUIColor, UIControlState.Disabled);
			}

			UpdateClearIconColor(cancelColor);
		}

		void UpdateSearchBarPlaceholder(UITextField textField)
		{
			if (textField == null)
				return;

			var formatted = (FormattedString)_searchHandler.Placeholder ?? string.Empty;
			var targetColor = _searchHandler.PlaceholderColor;
			var placeHolderColor = targetColor ?? Microsoft.Maui.Platform.ColorExtensions.PlaceholderColor.ToColor();
			textField.AttributedPlaceholder = formatted.ToNSAttributedString(_fontManager, defaultHorizontalAlignment: _searchHandler.HorizontalTextAlignment, defaultColor: placeHolderColor);

			//Center placeholder
			//var width = (_uiSearchBar.Frame.Width / 2) - textField.AttributedPlaceholder.Size.Width;

			//var paddingView = new UIImageView(new CGRect(0, 0, width, _uiSearchBar.Frame.Height));

			//textField.LeftView = paddingView;
			//var paddingView = UIView(frame: CGRect(x: 0, y: 0, width: width, height: searchBar.frame.height))

			//searchBarTextField.leftView = paddingView

			//searchBarTextField.leftViewMode = .unlessEditing
		}

		void UpdateTextTransform(UITextField textField)
		{
			if (textField == null)
				return;

			textField.Text = _searchHandler.UpdateFormsText(textField.Text, _searchHandler.TextTransform);
		}

		void UpdateTextColor(UITextField textField)
		{
			if (textField == null)
				return;

			_defaultTextColor = _defaultTextColor ?? textField.TextColor;
			var targetColor = _searchHandler.TextColor;

			textField.TextColor = targetColor?.ToPlatform() ?? _defaultTextColor;
			UpdateSearchBarTintColor(targetColor);
			UpdateSearchButtonIconColor(targetColor);
			UpdateClearPlaceholderIconColor(targetColor);
		}

		void UpdateSearchBarTintColor(Color targetColor)
		{
			_uiSearchBar.TintColor = targetColor?.ToPlatform() ?? _defaultTintColor;
		}

		void UpdateSearchButtonIconColor(Color targetColor)
		{
			var imageView = _uiSearchBar.FindDescendantView<UITextField>()?.LeftView as UIImageView;

			_defaultClearIconTintColor = _defaultClearIconTintColor ?? imageView.TintColor;

			SetSearchBarIconColor(imageView, targetColor, _defaultClearIconTintColor);
		}

		void UpdateClearPlaceholderIconColor(Color targetColor)
		{
			var uiTextField = _uiSearchBar.FindDescendantView<UITextField>();

			var uiButton = uiTextField.FindDescendantView<UIButton>();

			if (uiButton == null)
				return;

			_defaultPlaceholderTintColor = _defaultPlaceholderTintColor ?? uiButton?.ImageView?.TintColor;


			SetSearchBarIconColor(uiButton, targetColor, _defaultPlaceholderTintColor);
			uiButton.TintColor = targetColor?.ToPlatform() ?? _defaultPlaceholderTintColor;
		}

		void UpdateClearIconColor(Color targetColor)
		{
			var uiTextField = _uiSearchBar.FindDescendantView<UITextField>();

			var uiButton = uiTextField.ValueForKey(new NSString("clearButton")) as UIButton;

			if (uiButton == null)
				return;

			_defaultClearIconTintColor = _defaultClearIconTintColor ?? uiButton?.TintColor;

			SetSearchBarIconColor(uiButton, targetColor, _defaultClearIconTintColor);
		}

		void UpdateSearchBarHorizontalTextAlignment(UITextField textField, IView view = null)
		{
			if (textField == null)
				return;

			textField.TextAlignment = _searchHandler.HorizontalTextAlignment.ToPlatformHorizontal(textField.EffectiveUserInterfaceLayoutDirection);
		}

		void UpdateSearchBarVerticalTextAlignment(UITextField textField)
		{
			if (textField == null)
				return;

			textField.VerticalAlignment = _searchHandler.VerticalTextAlignment.ToPlatformVertical();
		}

		void UpdateKeyboard()
		{
			var keyboard = _searchHandler.Keyboard;
			_uiSearchBar.ApplyKeyboard(keyboard);

			// iPhone does not have an enter key on numeric keyboards
			if (DeviceInfo.Idiom == DeviceIdiom.Phone && (keyboard == Keyboard.Numeric || keyboard == Keyboard.Telephone))
			{
				_numericAccessoryView ??= CreateNumericKeyboardAccessoryView();
				_uiSearchBar.InputAccessoryView = _numericAccessoryView;
			}
			else
			{
				_uiSearchBar.InputAccessoryView = null;
			}

			_uiSearchBar.ReloadInputViews();
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			_searchHandler.SetIsFocused(false);
		}

		void OnEditingStarted(object sender, EventArgs e)
		{
			UpdateCancelButtonColor(_uiSearchBar.FindDescendantView<UIButton>());
			_searchHandler.SetIsFocused(true);
			//ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnTextChanged(object sender, UISearchBarTextChangedEventArgs e)
		{
			UpdateCancelButtonColor(_uiSearchBar.FindDescendantView<UIButton>());
		}


		void OnSearchButtonClicked(object sender, EventArgs e)
		{
			((ISearchHandlerController)_searchHandler).QueryConfirmed();
			_uiSearchBar.ResignFirstResponder();
		}

		void OnShowSoftInputRequested(object sender, EventArgs e)
		{
			_uiSearchBar?.BecomeFirstResponder();
		}

		void OnHideSoftInputRequested(object sender, EventArgs e)
		{
			_uiSearchBar?.ResignFirstResponder();
		}


		UIToolbar CreateNumericKeyboardAccessoryView()
		{
			var keyboardWidth = UIScreen.MainScreen.Bounds.Width;
			var accessoryView = new UIToolbar(new CGRect(0, 0, keyboardWidth, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var searchButton = new UIBarButtonItem(UIBarButtonSystemItem.Search, OnSearchButtonClicked);
			accessoryView.SetItems(new[] { spacer, searchButton }, false);

			return accessoryView;
		}

		static void SetSearchBarIconColor(UIImageView imageView, Color targetColor, UIColor defaultTintColor)
		{
			var icon = imageView?.Image;

			if (icon == null || (targetColor == null && defaultTintColor == null))
				return;

			var newIcon = icon.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
			imageView.TintColor = targetColor?.ToPlatform() ?? defaultTintColor;
			imageView.Image = newIcon;
		}

		static void SetSearchBarIconColor(UIButton button, Color targetColor, UIColor defaultTintColor)
		{
			var icon = button.ImageView?.Image;

			if (icon == null || (targetColor == null && defaultTintColor == null))
				return;

			var newIcon = icon.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
			button.SetImage(newIcon, UIControlState.Normal);
			button.SetImage(newIcon, UIControlState.Selected);
			button.SetImage(newIcon, UIControlState.Highlighted);
			button.TintColor = button.ImageView.TintColor = targetColor?.ToPlatform() ?? defaultTintColor;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_uiSearchBar != null)
				{
					_uiSearchBar.OnEditingStarted -= OnEditingStarted;
					_uiSearchBar.OnEditingStopped -= OnEditingEnded;
					_uiSearchBar.TextChanged -= OnTextChanged;
				}
				if (_searchHandler != null)
				{
					_searchHandler.FocusChangeRequested -= SearchHandlerFocusChangeRequested;
					_searchHandler.PropertyChanged -= SearchHandlerPropertyChanged;
					_searchHandler.ShowSoftInputRequested -= OnShowSoftInputRequested;
					_searchHandler.HideSoftInputRequested -= OnHideSoftInputRequested;
				}
				_searchHandler = null;
				_uiSearchBar = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
