using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class TabbedViewExtensions
	{
		internal static void DisableiOS18ToolbarTabs(this UITabBarController tabBarController)
		{
			// Should apply to iOS and Catalyst
			if (OperatingSystem.IsMacCatalystVersionAtLeast(18))
			{
				tabBarController.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;
				tabBarController.Mode = UITabBarControllerMode.TabSidebar;
			}
			else if (OperatingSystem.IsIOSVersionAtLeast(18, 0) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			{
				tabBarController.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		internal static void UpdateiOS15TabBarAppearance(
			this UITabBar tabBar,
			ref UITabBarAppearance _tabBarAppearance,
			UIColor? defaultBarColor,
			UIColor? defaultBarTextColor,
			Color? selectedTabColor,
			Color? unselectedTabColor,
			Color? barBackgroundColor,
			Color? selectedBarTextColor,
			Color? unSelectedBarTextColor)
		{
			if (_tabBarAppearance == null)
			{
				_tabBarAppearance = new UITabBarAppearance();
				_tabBarAppearance.ConfigureWithDefaultBackground();
			}

			var effectiveBarColor = (barBackgroundColor == null) ? defaultBarColor : barBackgroundColor.ToPlatform();
			// Set BarBackgroundColor
			if (effectiveBarColor != null)
			{
				_tabBarAppearance.BackgroundColor = effectiveBarColor;
			}

			// Set BarTextColor

			var effectiveSelectedBarTextColor = (selectedBarTextColor == null) ? defaultBarTextColor : selectedBarTextColor.ToPlatform();
			var effectiveUnselectedBarTextColor = (unSelectedBarTextColor == null) ? defaultBarTextColor : unSelectedBarTextColor.ToPlatform();

			// Update colors for all variations of the appearance to also make it work for iPads, etc. which use different layouts for the tabbar
			// Also, set ParagraphStyle explicitly. This seems to be an iOS bug. If we don't do this, tab titles will be truncat...

			// Set SelectedTabColor
			if (selectedTabColor is not null)
			{
				var foregroundColor = selectedTabColor.ToPlatform();
				var titleColor = effectiveSelectedBarTextColor ?? foregroundColor;

				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}
			else
			{
				var foregroundColor = UITabBar.Appearance.TintColor;
				var titleColor = effectiveSelectedBarTextColor ?? foregroundColor;
				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}

			// Set UnselectedTabColor
			// On iOS 26+, UITabBarAppearance.Normal state (TitleTextAttributes, IconColor) is ignored
			// by the liquid glass tab bar for unselected items. Skip setting Normal state and use
			// UnselectedItemTintColor directly instead.
			// See: https://github.com/dotnet/maui/issues/32125, https://github.com/dotnet/maui/issues/34605
			bool isiOS26OrNewer = OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);

			if (!isiOS26OrNewer)
			{
				if (unselectedTabColor is not null)
				{
					var foregroundColor = unselectedTabColor.ToPlatform();
					var titleColor = effectiveUnselectedBarTextColor ?? foregroundColor;
					_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foregroundColor;

					_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foregroundColor;

					_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foregroundColor;
				}
				else
				{
					var foreground = UITabBar.Appearance.TintColor;
					var titleColor = effectiveUnselectedBarTextColor ?? foreground;
					_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foreground;

					_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foreground;

					_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
					_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foreground;
				}
			}

			// Set the TabBarAppearance
			// On iOS 26+, setting StandardAppearance may cause UIKit to ignore
			// UnselectedItemTintColor. Only set appearance for background color and
			// selected state; use direct properties for unselected state.
			tabBar.StandardAppearance = tabBar.ScrollEdgeAppearance = _tabBarAppearance;

			if (isiOS26OrNewer)
			{
				UIColor? effectiveUnselectedTint = null;

				if (unselectedTabColor is not null)
					effectiveUnselectedTint = unselectedTabColor.ToPlatform();
				else if (effectiveUnselectedBarTextColor is not null)
					effectiveUnselectedTint = effectiveUnselectedBarTextColor;

				tabBar.UnselectedItemTintColor = effectiveUnselectedTint;

				// Also ensure TintColor is set for selected items
				var effectiveSelectedTint = selectedTabColor?.ToPlatform() ?? effectiveSelectedBarTextColor;
				if (effectiveSelectedTint is not null)
					tabBar.TintColor = effectiveSelectedTint;

				// iOS 26 liquid glass strips tint colors during compositing.
				// Use pre-colored images with AlwaysOriginal to bypass the tint pipeline.
				tabBar.ApplyPreColoredImagesForIOS26(effectiveUnselectedTint, effectiveSelectedTint);
			}
		}

		/// <summary>
		/// On iOS 26+, the liquid glass tab bar's compositing pipeline strips TintColor from
		/// unselected tab icons and labels. This method bypasses the tint system by creating
		/// pre-colored copies of tab icons using AlwaysOriginal rendering mode, which bakes the
		/// color into the image pixel data. Also sets per-item title text attributes.
		/// Must be called on every layout pass because UIKit may reset these during layout.
		/// See: https://github.com/dotnet/maui/issues/32125, https://github.com/dotnet/maui/issues/34605
		/// </summary>
		[System.Runtime.Versioning.SupportedOSPlatform("ios26.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst26.0")]
		internal static void ApplyPreColoredImagesForIOS26(this UITabBar tabBar, UIColor? unselectedColor, UIColor? selectedColor)
		{
			if (tabBar.Items is null)
				return;

			foreach (var item in tabBar.Items)
			{
				if (item.Image is UIImage img && unselectedColor is not null)
				{
					// Retrieve or store the original template image so we always tint
					// from a clean alpha mask, avoiding quality degradation from
					// repeated AlwaysOriginal→Template round-trips.
					if (!s_originalTemplateImages.TryGetValue(item, out var template))
					{
						template = img.RenderingMode == UIImageRenderingMode.AlwaysOriginal
							? img.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)
							: img;
						s_originalTemplateImages.AddOrUpdate(item, template);
					}

					// Only re-tint if UIKit has reset the image (rendering mode won't
					// be AlwaysOriginal) or if this is the first application. This avoids
					// creating new UIImage instances on every layout pass.
					if (img.RenderingMode != UIImageRenderingMode.AlwaysOriginal)
					{
						item.Image = template.ApplyTintColor(unselectedColor)
							?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

						if (selectedColor is not null)
						{
							item.SelectedImage = template.ApplyTintColor(selectedColor)
								?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
						}
					}
				}

				// Note: SetTitleTextAttributes for Normal state is intentionally not called on iOS 26+.
				// Apple's Liquid Glass design system ignores the Normal state appearance to ensure visual
				// consistency with the glass material. Icon coloring via AlwaysOriginal rendering (above)
				// is the supported workaround. Text color customization for unselected tabs is not available
				// on iOS 26+. See: https://github.com/dotnet/maui/issues/32125

				if (selectedColor is not null)
				{
					item.SetTitleTextAttributes(
						new UIStringAttributes { ForegroundColor = selectedColor, ParagraphStyle = NSParagraphStyle.Default },
						UIControlState.Selected);
				}
			}
		}

		// Stores original template images per tab bar item so we always tint from
		// a clean alpha mask. Uses ConditionalWeakTable so entries are collected
		// when the UITabBarItem is garbage-collected.
		static readonly ConditionalWeakTable<UITabBarItem, UIImage> s_originalTemplateImages = new();

		internal static UIImage? AutoResizeTabBarImage(UITraitCollection traitCollection, UIImage image)
		{
			if (image == null || image.Size.Width <= 0 || image.Size.Height <= 0)
			{
				return null;
			}

			CGSize newSize = image.Size;

			//Define size constants according to Apple's guidelines
			//https://developer.apple.com/design/human-interface-guidelines/tab-bars#Target-dimensions
			const int regularWideWidth = 31, compactWideWidth = 23;
			const int regularTallHeight = 28, compactTallHeight = 20;
			const int regularSquareSize = 25, compactSquareSize = 18;

			bool isRegularTabBar = traitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Regular;
			if (image.Size.Width > image.Size.Height) //Wide
			{
				newSize.Width = isRegularTabBar ? regularWideWidth : compactWideWidth;
				newSize.Height = newSize.Width * image.Size.Height / image.Size.Width;
			}
			else if (image.Size.Width < image.Size.Height) //Tall
			{
				newSize.Height = isRegularTabBar ? regularTallHeight : compactTallHeight;
				newSize.Width = newSize.Height * image.Size.Width / image.Size.Height;
			}
			else //Square
			{
				newSize.Width = isRegularTabBar ? regularSquareSize : compactSquareSize;
				newSize.Height = newSize.Width;
			}

			var resizedImage = image.ResizeImageSource(newSize.Width, newSize.Height, new CGSize(image.Size.Width, image.Size.Height));

			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				return resizedImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
			}

			return resizedImage;
		}
	}
}
