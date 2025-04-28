using System;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		internal static UITextField? GetSearchTextField(this UISearchBar searchBar)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13))
			{
				return searchBar.SearchTextField;
			}

			// Search Subviews up to two levels deep
			// https://stackoverflow.com/a/58056700
			foreach (var child in searchBar.Subviews)
			{
				if (child is UITextField childTextField)
					return childTextField;

				foreach (var grandChild in child.Subviews)
				{
					if (grandChild is UITextField grandChildTextField)
						return grandChildTextField;
				}
			}

			return null;
		}

		internal static void UpdateBackground(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var background = searchBar.Background;

			if (background.IsTransparent())
			{
				if (uiSearchBar.BackgroundImage is null)
				{
					uiSearchBar.SetBackgroundImage(new UIKit.UIImage(), UIKit.UIBarPosition.Any, UIKit.UIBarMetrics.Default);
				}
				uiSearchBar.BackgroundColor = UIColor.Clear;
			}
			else
			{
				uiSearchBar.SetBackgroundImage(null, UIKit.UIBarPosition.Any, UIKit.UIBarMetrics.Default);

				if (background is SolidPaint solidPaint)
				{
					uiSearchBar.BarTintColor = solidPaint.Color.ToPlatform();
				}
				else if (background is GradientPaint gradientPaint)
				{
					ViewExtensions.UpdateBackground(uiSearchBar, gradientPaint);
				}
				else if (background == null)
				{
					uiSearchBar.BarTintColor = UISearchBar.Appearance.BarTintColor;
				}
			}
		}


		public static void UpdateIsEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UserInteractionEnabled = searchBar.IsEnabled;
		}

		public static void UpdateText(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.Text = searchBar.Text;
		}

		public static void UpdatePlaceholder(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			var placeholder = searchBar.Placeholder ?? string.Empty;
			var placeholderColor = searchBar.PlaceholderColor is Color color ? color.ToPlatform() : ColorExtensions.PlaceholderColor;
			textField.AttributedPlaceholder = new NSAttributedString(str: placeholder, foregroundColor: placeholderColor);
			textField.AttributedPlaceholder.WithCharacterSpacing(searchBar.CharacterSpacing);
		}

		public static void UpdateFont(this UISearchBar uiSearchBar, ITextStyle textStyle, IFontManager fontManager)
		{
			uiSearchBar.UpdateFont(textStyle, fontManager, null);
		}

		public static void UpdateFont(this UISearchBar uiSearchBar, ITextStyle textStyle, IFontManager fontManager, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			textField.UpdateFont(textStyle, fontManager);
		}

		public static void UpdateVerticalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UpdateVerticalTextAlignment(searchBar, null);
		}

		public static void UpdateVerticalTextAlignment(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			textField.VerticalAlignment = searchBar.VerticalTextAlignment.ToPlatformVertical();
		}

		public static void UpdateMaxLength(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var maxLength = searchBar.MaxLength;

			if (maxLength == -1)
				maxLength = int.MaxValue;

			var currentControlText = uiSearchBar.Text;

			if (currentControlText?.Length > maxLength)
				uiSearchBar.Text = currentControlText.Substring(0, maxLength);
		}

		public static void UpdateIsReadOnly(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.UserInteractionEnabled = !(searchBar.IsReadOnly || searchBar.InputTransparent);
		}

		internal static bool ShouldShowCancelButton(this ISearchBar searchBar) =>
			!string.IsNullOrEmpty(searchBar.Text);

		public static void UpdateCancelButton(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.ShowsCancelButton = searchBar.ShouldShowCancelButton();

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time
			var cancelButton = uiSearchBar.FindDescendantView<UIButton>();

			if (cancelButton == null)
				return;

			if (searchBar.CancelButtonColor != null)
			{
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Normal);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Highlighted);
				cancelButton.SetTitleColor(searchBar.CancelButtonColor.ToPlatform(), UIControlState.Disabled);

				if (cancelButton.TraitCollection.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
					cancelButton.TintColor = searchBar.CancelButtonColor.ToPlatform();
			}
		}

		internal static void UpdateSearchIcon(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField?.LeftView is not UIImageView iconView || iconView.Image is null)
				return;

			if (searchBar.SearchIconColor is not null)
			{
				iconView.Image = iconView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				iconView.TintColor = searchBar.SearchIconColor.ToPlatform();
			}

		}

		public static void UpdateIsTextPredictionEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField = null)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			if (searchBar.IsTextPredictionEnabled)
				textField.AutocorrectionType = UITextAutocorrectionType.Yes;
			else
				textField.AutocorrectionType = UITextAutocorrectionType.No;
		}

		public static void UpdateIsSpellCheckEnabled(this UISearchBar uiSearchBar, ISearchBar searchBar, UITextField? textField = null)
		{
			textField ??= uiSearchBar.GetSearchTextField();

			if (textField == null)
				return;

			if (searchBar.IsSpellCheckEnabled)
				textField.SpellCheckingType = UITextSpellCheckingType.Yes;
			else
				textField.SpellCheckingType = UITextSpellCheckingType.No;
		}

		public static void UpdateKeyboard(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			var keyboard = searchBar.Keyboard;

			uiSearchBar.ApplyKeyboard(keyboard);

			if (keyboard is not CustomKeyboard)
			{
				uiSearchBar.UpdateIsTextPredictionEnabled(searchBar);
				uiSearchBar.UpdateIsSpellCheckEnabled(searchBar);
			}

			uiSearchBar.ReloadInputViews();
		}

		public static void UpdateReturnType(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.ReturnKeyType = searchBar.ReturnType.ToPlatform();
		}
	}
}