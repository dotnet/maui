using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Foundation;
#if __IOS__ || __TVOS__
using ObjCRuntime;
using UIKit;
#elif __MACOS__
using AppKit;
#endif

namespace Microsoft.Maui.Essentials.Implementations
{
	public class AppInfoImplementation : IAppInfo
	{
		public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

		public string PackageName => GetBundleValue("CFBundleIdentifier");

		public string Name => GetBundleValue("CFBundleDisplayName") ?? GetBundleValue("CFBundleName");

		public Version Version => Utils.ParseVersion(VersionString);

		public string VersionString => GetBundleValue("CFBundleShortVersionString");

		public string BuildString => GetBundleValue("CFBundleVersion");

		[UnsupportedOSPlatformGuard("ios13.0")]
		[UnsupportedOSPlatformGuard("tvos13.0")]
		private static readonly bool IsIosTvOs13OrBefore = (OperatingSystem.IsIOS() && !OperatingSystem.IsIOSVersionAtLeast(13, 0)) ||
			(OperatingSystem.IsTvOS() && !OperatingSystem.IsTvOSVersionAtLeast(13, 0));

		string GetBundleValue(string key)
		   => NSBundle.MainBundle.ObjectForInfoDictionary(key)?.ToString();

#if __IOS__ || __TVOS__
		public async void ShowSettingsUI()
			=> await Launcher.OpenAsync(UIApplication.OpenSettingsUrlString);
#elif __MACOS__
        public void ShowSettingsUI()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var prefsApp = ScriptingBridge.SBApplication.FromBundleIdentifier("com.apple.systempreferences");
                prefsApp.SendMode = ScriptingBridge.AESendMode.NoReply;
                prefsApp.Activate();
            });
        }
#else
		public void ShowSettingsUI() =>
			throw new FeatureNotSupportedException();
#endif

#if __IOS__ || __TVOS__
		public AppTheme RequestedTheme
		{
			get
			{
				if (IsIosTvOs13OrBefore)
					return AppTheme.Unspecified;

				var traits =
					MainThread.InvokeOnMainThread(() => Platform.GetCurrentUIViewController()?.TraitCollection) ??
					UITraitCollection.CurrentTraitCollection;

				var uiStyle = traits.UserInterfaceStyle;

				return uiStyle switch
				{
					UIUserInterfaceStyle.Light => AppTheme.Light,
					UIUserInterfaceStyle.Dark => AppTheme.Dark,
					_ => AppTheme.Unspecified
				};
			}
		}
#elif __MACOS__
        public AppTheme RequestedTheme
        {
			get
			{
				if (OperatingSystem.IsMacOSVersionAtLeast(10, 14))
				{
					var app = NSAppearance.CurrentAppearance?.FindBestMatch(new string[]
					{
						NSAppearance.NameAqua,
						NSAppearance.NameDarkAqua
					});

					if (string.IsNullOrEmpty(app))
						return AppTheme.Unspecified;

					if (app == NSAppearance.NameDarkAqua)
						return AppTheme.Dark;
				}
				return AppTheme.Light;
			}
        }
#else
		public AppTheme RequestedTheme =>
			AppTheme.Unspecified;
#endif

#if __IOS__ || __TVOS__
		public LayoutDirection RequestedLayoutDirection
		{
			get
			{
				var currentWindow = Platform.GetCurrentWindow(false);
				UIUserInterfaceLayoutDirection layoutDirection =
					currentWindow?.EffectiveUserInterfaceLayoutDirection ??
					UIApplication.SharedApplication.UserInterfaceLayoutDirection;

				return (layoutDirection == UIUserInterfaceLayoutDirection.RightToLeft) ?
					LayoutDirection.RightToLeft : LayoutDirection.LeftToRight;
			}
		}
#elif __MACOS__
		public bool IsDeviceUILayoutDirectionRightToLeft => 
			NSApplication.SharedApplication.UserInterfaceLayoutDirection == NSApplicationLayoutDirection.RightToLeft;
#endif

		internal static bool VerifyHasUrlScheme(string scheme)
		{
			var cleansed = scheme.Replace("://", string.Empty, StringComparison.Ordinal);
			var schemes = GetCFBundleURLSchemes().ToList();
			return schemes.Any(x => x != null && x.Equals(cleansed, StringComparison.OrdinalIgnoreCase));
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
