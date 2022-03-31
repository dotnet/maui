using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Provider;
using AndroidX.Core.Content.PM;

namespace Microsoft.Maui.ApplicationModel
{
	class AppInfoImplementation : IAppInfo
	{
		public string PackageName => Application.Context.PackageName;

		public string Name
		{
			get
			{
				var applicationInfo = Application.Context.ApplicationInfo;
				var packageManager = Application.Context.PackageManager;
				return applicationInfo.LoadLabel(packageManager);
			}
		}

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString
		{
			get
			{
				var pm = Application.Context.PackageManager;
				var packageName = Application.Context.PackageName;
				using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
				{
					return info.VersionName;
				}
			}
		}

		public string BuildString
		{
			get
			{
				var pm = Application.Context.PackageManager;
				var packageName = Application.Context.PackageName;
				using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
				{
#if __ANDROID_28__
					return PackageInfoCompat.GetLongVersionCode(info).ToString(CultureInfo.InvariantCulture);
#else
#pragma warning disable CS0618 // Type or member is obsolete
					return info.VersionCode.ToString(CultureInfo.InvariantCulture);
#pragma warning restore CS0618 // Type or member is obsolete
#endif
				}
			}
		}

		public void ShowSettingsUI()
		{
			var context = ActivityStateManager.Default.GetCurrentActivity(false) ?? Application.Context;

			var settingsIntent = new Intent();
			settingsIntent.SetAction(global::Android.Provider.Settings.ActionApplicationDetailsSettings);
			settingsIntent.AddCategory(Intent.CategoryDefault);
			settingsIntent.SetData(global::Android.Net.Uri.Parse("package:" + PackageName));

			var flags = ActivityFlags.NewTask | ActivityFlags.NoHistory | ActivityFlags.ExcludeFromRecents;

#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				flags |= ActivityFlags.LaunchAdjacent;
#endif
			settingsIntent.SetFlags(flags);

			context.StartActivity(settingsIntent);
		}

		public AppTheme RequestedTheme
			=> (Application.Context.Resources.Configuration.UiMode & UiMode.NightMask) switch
			{
				UiMode.NightYes => AppTheme.Dark,
				UiMode.NightNo => AppTheme.Light,
				_ => AppTheme.Unspecified
			};

		public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

		public LayoutDirection RequestedLayoutDirection
		{
			get
			{
				if (!OperatingSystem.IsAndroidVersionAtLeast(17))
					return LayoutDirection.LeftToRight;

				var config = Application.Context.Resources?.Configuration;
				if (config == null)
					return LayoutDirection.Unknown;

				return (config.LayoutDirection == Android.Views.LayoutDirection.Rtl) ? LayoutDirection.RightToLeft :
					LayoutDirection.LeftToRight;
			}
		}
	}
}
