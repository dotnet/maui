using System;
using System.Drawing;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class UINavigationBarExtensions
{
    public static void SetupDefaultNavigationBarAppearance(this UINavigationBar navBar)
    {
        if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
        {
            return;
        }

        // We will use UINavigationBar.Appearance to infer settings that
        // were already set to the navigation bar in older versions of
        // iOS.
        UINavigationBarAppearance navAppearance = navBar.StandardAppearance;

        if (navAppearance.BackgroundColor == null)
        {
            UIColor? backgroundColor = navBar.BarTintColor;

            navBar.StandardAppearance.BackgroundColor = backgroundColor;

            if (navBar.ScrollEdgeAppearance != null)
            {
                navBar.ScrollEdgeAppearance.BackgroundColor = backgroundColor;
            }

            if (navBar.CompactAppearance != null)
            {
                navBar.CompactAppearance.BackgroundColor = backgroundColor;
            }
        }

        if (navAppearance.BackgroundImage == null)
        {
            UIImage backgroundImage = navBar.GetBackgroundImage(UIBarMetrics.Default);

            navBar.StandardAppearance.BackgroundImage = backgroundImage;

            if (navBar.ScrollEdgeAppearance != null)
            {
                navBar.ScrollEdgeAppearance.BackgroundImage = backgroundImage;
            }

            if (navBar.CompactAppearance != null)
            {
                navBar.CompactAppearance.BackgroundImage = backgroundImage;
            }
        }

        if (navAppearance.ShadowImage == null)
        {
            UIImage? shadowImage = navBar.ShadowImage;
            UIColor clearColor = UIColor.Clear;

            navBar.StandardAppearance.ShadowImage = shadowImage;

            if (navBar.ScrollEdgeAppearance != null)
            {
                navBar.ScrollEdgeAppearance.ShadowImage = shadowImage;
            }

            if (navBar.CompactAppearance != null)
            {
                navBar.CompactAppearance.ShadowImage = shadowImage;
            }

            if (shadowImage != null && shadowImage.Size == SizeF.Empty)
            {
                navBar.StandardAppearance.ShadowColor = clearColor;

                if (navBar.ScrollEdgeAppearance != null)
                {
                    navBar.ScrollEdgeAppearance.ShadowColor = clearColor;
                }

                if (navBar.CompactAppearance != null)
                {
                    navBar.CompactAppearance.ShadowColor = clearColor;
                }
            }
        }

        UIImage? backIndicatorImage = navBar.BackIndicatorImage;
        UIImage? backIndicatorTransitionMaskImage = navBar.BackIndicatorTransitionMaskImage;

        if (backIndicatorImage != null && backIndicatorImage.Size == SizeF.Empty)
        {
            backIndicatorImage = GetEmptyBackIndicatorImage();
        }

        if (backIndicatorTransitionMaskImage != null && backIndicatorTransitionMaskImage.Size == SizeF.Empty)
        {
            backIndicatorTransitionMaskImage = GetEmptyBackIndicatorImage();
        }

        navBar.CompactAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
        navBar.StandardAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
        navBar.ScrollEdgeAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
    }

    public static UIImage GetEmptyBackIndicatorImage()
    {
        var rect = RectangleF.Empty;
        SizeF size = rect.Size;

        UIGraphics.BeginImageContext(size);
        CGContext? context = UIGraphics.GetCurrentContext();
        context?.SetFillColor(1, 1, 1, 0);
        context?.FillRect(rect);

        UIImage? empty = UIGraphics.GetImageFromCurrentImageContext();
        context?.Dispose();

        return empty;
    }
}
