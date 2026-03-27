using System;
using CoreGraphics;
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

			switch (background)
			{
				case null:
					uiSearchBar.BarTintColor = UISearchBar.Appearance.BarTintColor;
					break;

				case SolidPaint solid:
					if (solid.Color == Colors.Transparent)
					{
						uiSearchBar.BackgroundImage = new UIImage();
						uiSearchBar.BarTintColor = UIColor.Clear;
					}
					else
					{
						uiSearchBar.BackgroundImage = null;
						uiSearchBar.BarTintColor = solid.Color.ToPlatform();
					}
					break;

				case GradientPaint gradientPaint:
					ViewExtensions.UpdateBackground(uiSearchBar, gradientPaint);
					break;
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

		// Tag used to identify the cancel button color overlay view added on iOS 26+.
		// Value 0x53424343 encodes "SBCC" (SearchBarCancelColor) to avoid collisions with system-assigned tags.
		const nint CancelButtonColorOverlayTag = unchecked((nint)0x53424343);

		public static void UpdateCancelButton(this UISearchBar uiSearchBar, ISearchBar searchBar)
		{
			uiSearchBar.ShowsCancelButton = searchBar.ShouldShowCancelButton();

			// We can't cache the cancel button reference because iOS drops it when it's not displayed
			// and creates a brand new one when necessary, so we have to look for it each time.
			// Exclude UIButton instances that are descendants of UITextField — those are the
			// text-clear button inside the search field, not the cancel button outside it.
			var cancelButton = uiSearchBar.FindDescendantView<UIButton>(
				btn => btn.FindParent(v => v is UITextField) == null);

			if (cancelButton == null)
			{
				// Cancel button is hidden — remove any overlay we previously added
				if (OperatingSystem.IsIOSVersionAtLeast(26))
					RemoveCancelButtonOverlay(uiSearchBar);
				return;
			}

			if (searchBar.CancelButtonColor != null)
			{
				var platformColor = searchBar.CancelButtonColor.ToPlatform();

				cancelButton.SetTitleColor(platformColor, UIControlState.Normal);
				cancelButton.SetTitleColor(platformColor, UIControlState.Highlighted);
				cancelButton.SetTitleColor(platformColor, UIControlState.Disabled);

				// On Mac, the cancel button is rendered as an icon (X mark) rather than text,
				// so TintColor must be used to apply the color to the icon.
				if (cancelButton.TraitCollection.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
				{
					cancelButton.TintColor = platformColor;
				}

				// On iOS 26+, UIKit overrides TintColor/UIButtonConfiguration on every layout
				// pass, making standard color APIs ineffective for the cancel button icon.
				// Place a colored UIImageView sibling on top of the cancel button to apply
				// the color outside UIButton's rendering pipeline.
				// Defer via DispatchAsync so the cancel button frame is valid after layout.
				// Capture the search bar (not the button) to always look up the current
				// cancel button instance — iOS 26 may recreate it during theme transitions.
				if (OperatingSystem.IsIOSVersionAtLeast(26))
				{
					var weakSearchBar = new WeakReference<UISearchBar>(uiSearchBar);
					var weakVirtualView = new WeakReference<ISearchBar>(searchBar);
					CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						if (!weakSearchBar.TryGetTarget(out var sb))
						{
							return;
						}

						if (!weakVirtualView.TryGetTarget(out var virtualSearchBar))
						{
							return;
						}

						// Re-evaluate the desired cancel button color; it may have been
						// changed or cleared since this callback was queued.
						var currentColor = virtualSearchBar.CancelButtonColor;
						if (currentColor is null)
						{
							RemoveCancelButtonOverlay(sb);
							return;
						}

						var currentButton = sb.FindDescendantView<UIButton>(
							btn => btn.FindParent(v => v is UITextField) == null);
						if (currentButton is not null)
						{
							ApplyCancelButtonOverlay(sb, currentButton, currentColor.ToPlatform());
						}
					});
				}
			}
			else if (OperatingSystem.IsIOSVersionAtLeast(26))
			{
				// CancelButtonColor was cleared — remove any overlay we previously added
				RemoveCancelButtonOverlay(uiSearchBar);
			}
		}

		// Schedules a deferred retry of ApplyCancelButtonOverlay on the main queue.
		// Used when the cancel button is not ready for layout (detached or zero-frame).
		static void ScheduleOverlayRetry(UISearchBar uiSearchBar, UIColor color, int retryCount)
		{
			var weakSB = new WeakReference<UISearchBar>(uiSearchBar);
			CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
			{
				if (!weakSB.TryGetTarget(out var sb)) return;
				var btn = sb.FindDescendantView<UIButton>(
					b => b.FindParent(v => v is UITextField) == null);
				if (btn != null)
					ApplyCancelButtonOverlay(sb, btn, color, retryCount + 1);
			});
		}

		static void ApplyCancelButtonOverlay(UISearchBar uiSearchBar, UIButton cancelButton, UIColor color, int retryCount = 0)
		{
			var parentView = cancelButton.Superview;
			if (parentView == null)
			{
				// Button was detached by UIKit (e.g. mid-transition during a theme change).
				// Retry with a fresh lookup so we always work with the current button instance.
				if (retryCount < 2)
					ScheduleOverlayRetry(uiSearchBar, color, retryCount);
				return;
			}

			// Remove any overlay from a previous call (e.g. color change or re-focus)
			uiSearchBar.ViewWithTag(CancelButtonColorOverlayTag)?.RemoveFromSuperview();

			// Find the UIImageView that UIButton uses to render the X icon.
			// We need its frame to determine the rendered image size.
			var iv = cancelButton.FindDescendantView<UIImageView>();
			if (iv == null)
				return;

			// Convert the icon's frame from its local coordinate space to the parent view.
			var iconFrameInParent = parentView.ConvertRectFromView(iv.Frame, iv.Superview);
			if (iconFrameInParent.Width <= 0 || iconFrameInParent.Height <= 0)
			{
				// The cancel button hasn't been laid out yet (e.g. on initial load when
				// CancelButtonColor is set via AppThemeBinding before the view appears).
				// Retry after the next layout pass (up to two times) so we get a valid frame.
				if (retryCount < 2)
					ScheduleOverlayRetry(uiSearchBar, color, retryCount);
				return;
			}

			// Render the xmark icon in the requested color using CoreGraphics.
			// AlwaysOriginal prevents UIKit from re-tinting the baked image.
			var xmarkImage = UIImage.GetSystemImage("xmark");
			if (xmarkImage == null)
				return;

			var imageSize = iconFrameInParent.Size;
			var renderer = new UIGraphicsImageRenderer(imageSize, new UIGraphicsImageRendererFormat
			{
				Opaque = false,
				Scale = 0,
			});

			var coloredImage = renderer.CreateImage(_ =>
			{
				xmarkImage.Draw(new CGRect(CGPoint.Empty, imageSize));
				var ctx = UIGraphics.GetCurrentContext();
				if (ctx != null)
				{
					ctx.SetBlendMode(CGBlendMode.SourceIn);
					ctx.SetFillColor(color.CGColor);
					ctx.FillRect(new CGRect(CGPoint.Empty, imageSize));
				}
			}).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

			// Add the overlay as the last (topmost) sibling of the cancel button.
			// Use Auto Layout constraints so the overlay stays centered over the X icon
			// on rotation, multitasking split view, or dynamic type changes.
			var overlay = new UIImageView
			{
				Image = coloredImage,
				ContentMode = UIViewContentMode.ScaleAspectFit,
				Tag = CancelButtonColorOverlayTag,
				UserInteractionEnabled = false,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			parentView.AddSubview(overlay);

			NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
			{
				overlay.CenterXAnchor.ConstraintEqualTo(cancelButton.CenterXAnchor),
				overlay.CenterYAnchor.ConstraintEqualTo(cancelButton.CenterYAnchor),
				overlay.WidthAnchor.ConstraintEqualTo(imageSize.Width),
				overlay.HeightAnchor.ConstraintEqualTo(imageSize.Height),
			});
		}

		static void RemoveCancelButtonOverlay(UISearchBar uiSearchBar)
		{
			// UIView.ViewWithTag searches the entire subtree recursively, so it finds the overlay
			// regardless of where it was placed in the search bar's view hierarchy.
			uiSearchBar.ViewWithTag(CancelButtonColorOverlayTag)?.RemoveFromSuperview();
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

		internal static UIButton? GetClearButton(this UISearchBar searchBar) =>
			searchBar.GetSearchTextField()?.ValueForKey(new NSString("clearButton")) as UIButton;
	}
}