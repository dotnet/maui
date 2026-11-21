using System;
using System.Runtime.Versioning;
using UIKit;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Adds context flyout items to the edit menu of text inputs, like native apps extend the text menu instead
	/// of replacing it with a context menu: long press keeps selecting text and the items join Cut, Copy, Paste.
	/// </summary>
	[SupportedOSPlatform("ios13.0")]
	[SupportedOSPlatform("maccatalyst13.0.0")]
	static class ContextFlyoutEditMenu
	{
		// The edit menu can only be extended from iOS 16; before that text inputs keep only their own menu
		internal static bool IsSupported =>
			RuntimeFeature.AreMobileContextFlyoutsEnabled &&
			(OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16));

		/// <summary>
		/// The system's edit menu of a text input, followed by the items of the closest context flyout of the input
		/// or one of its parents. Without a flyout the system's menu is returned unchanged.
		/// </summary>
		/// <param name="textInput">The text input showing the edit menu.</param>
		/// <param name="menu">The menu the text input would show; <see langword="null"/> for the suggested actions.</param>
		/// <param name="suggestedActions">The actions UIKit suggests for the edit menu.</param>
		[SupportedOSPlatform("ios16.0")]
		[SupportedOSPlatform("maccatalyst16.0")]
		internal static UIMenu? AddItems(UIView textInput, UIMenu? menu, UIMenuElement[] suggestedActions)
		{
			if (!IsSupported || MauiUIContextMenuInteraction.FindClosest(textInput) is not MauiUIContextMenuInteraction interaction ||
				interaction.CreateMenu() is not UIMenu { Children.Length: > 0 } flyoutMenu)
				return menu;

			// The system's actions first, like the edit menus of native apps
			var systemActions = menu?.Children ?? suggestedActions;
			var children = new UIMenuElement[systemActions.Length + 1];
			Array.Copy(systemActions, children, systemActions.Length);
			children[systemActions.Length] = UIMenu.Create(string.Empty, null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, flyoutMenu.Children);

			return menu?.GetMenuByReplacingChildren(children) ?? UIMenu.Create(children);
		}

		/// <summary>
		/// Whether the view is (inside) a text input whose edit menu shows context flyout items, so a long press
		/// there keeps selecting text instead of opening the context menu.
		/// </summary>
		internal static bool IsTextInput(UIView? view, UIView? stopAt)
		{
			if (!IsSupported)
				return false;

			for (var current = view; current is not null; current = current.Superview)
			{
				switch (current)
				{
					case MauiTextField textField:
						return textField.Enabled;
					case MauiTextView textView:
						return textView.Editable || textView.Selectable;
					case UISearchTextField searchField when searchField.Delegate is MauiSearchBar.SearchEditorDelegate:
						return searchField.Enabled;
				}

				if (current == stopAt || current is UIWindow)
					break;
			}

			return false;
		}
	}
}
