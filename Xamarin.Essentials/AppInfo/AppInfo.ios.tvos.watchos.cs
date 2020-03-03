using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName() => GetBundleValue("CFBundleIdentifier");

        static string PlatformGetName() => GetBundleValue("CFBundleDisplayName") ?? GetBundleValue("CFBundleName");

        static string PlatformGetVersionString() => GetBundleValue("CFBundleShortVersionString");

        static string PlatformGetBuild() => GetBundleValue("CFBundleVersion");

        static string GetBundleValue(string key)
           => NSBundle.MainBundle.ObjectForInfoDictionary(key)?.ToString();

#if __IOS__ || __TVOS__
        static void PlatformShowSettingsUI() =>
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
#else
        static void PlatformShowSettingsUI() =>
            throw new FeatureNotSupportedException();
#endif

#if __IOS__ || __TVOS__
        static AppTheme PlatformRequestedTheme()
        {
            if (!Platform.HasOSVersion(13, 0))
                return AppTheme.Unspecified;

            var uiStyle = Platform.GetCurrentUIViewController()?.TraitCollection?.UserInterfaceStyle ??
                UITraitCollection.CurrentTraitCollection.UserInterfaceStyle;

            return uiStyle switch
            {
                UIUserInterfaceStyle.Light => AppTheme.Light,
                UIUserInterfaceStyle.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
#else
        static AppTheme PlatformRequestedTheme() =>
            AppTheme.Unspecified;
#endif
    }
}
