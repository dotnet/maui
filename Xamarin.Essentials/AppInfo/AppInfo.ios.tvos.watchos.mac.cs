using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
#if __IOS__ || __TVOS__
using UIKit;
#endif

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
#elif __MACOS__
        static void PlatformShowSettingsUI()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var prefsApp = ScriptingBridge.SBApplication.FromBundleIdentifier("com.apple.systempreferences");
                prefsApp.SendMode = ScriptingBridge.AESendMode.NoReply;
                prefsApp.Activate();
            });
        }
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

        internal static bool VerifyHasUrlScheme(string scheme)
        {
            var cleansed = scheme.Replace("://", string.Empty);
            var schemes = GetCFBundleURLSchemes().ToList();
            return schemes.Any(x => x != null && x.Equals(cleansed, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static IEnumerable<string> GetCFBundleURLSchemes()
        {
            var schemes = new List<string>();

            NSObject nsobj = null;
            if (!NSBundle.MainBundle.InfoDictionary.TryGetValue((NSString)"CFBundleURLTypes", out nsobj))
                return schemes;

            var array = nsobj as NSArray;

            if (array == null)
                return schemes;

            for (nuint i = 0; i < array.Count; i++)
            {
                var d = array.GetItem<NSDictionary>(i);
                if (d == null || !d.Any())
                    continue;

                if (!d.TryGetValue((NSString)"CFBundleURLSchemes", out nsobj))
                    continue;

                var a = nsobj as NSArray;
                var urls = ConvertToIEnumerable<NSString>(a).Select(x => x.ToString()).ToArray();
                foreach (var url in urls)
                    schemes.Add(url);
            }

            return schemes;
        }

        static IEnumerable<T> ConvertToIEnumerable<T>(NSArray array)
            where T : class, ObjCRuntime.INativeObject
        {
            for (nuint i = 0; i < array.Count; i++)
                yield return array.GetItem<T>(i);
        }
    }
}
