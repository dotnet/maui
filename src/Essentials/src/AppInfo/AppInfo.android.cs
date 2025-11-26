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
		static readonly Lazy<string> _name = new Lazy<string>(() => Application.Context.ApplicationInfo.LoadLabel(Application.Context.PackageManager));
		static readonly Lazy<string> _packageName = new Lazy<string>(() => Application.Context.PackageName);
#pragma warning disable CS0618, CA1416, CA1422 // Deprecated in API 33: https://developer.android.com/reference/android/content/pm/PackageManager#getPackageInfo(java.lang.String,%20int)
		static readonly Lazy<PackageInfo> _packageInfo = new Lazy<PackageInfo>(() => Application.Context.PackageManager.GetPackageInfo(_packageName.Value, PackageInfoFlags.MetaData));
#pragma warning restore CS0618, CA1416, CA1422

		public string PackageName => _packageName.Value;

		public string Name => _name.Value;

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString => _packageInfo.Value.VersionName;

		public string BuildString => PackageInfoCompat.GetLongVersionCode(_packageInfo.Value).ToString(CultureInfo.InvariantCulture);

		public void ShowSettingsUI()
		{
			var context = ActivityStateManager.Default.GetCurrentActivity(false) ?? Application.Context;

			var settingsIntent = new Intent();
			settingsIntent.SetAction(global::Android.Provider.Settings.ActionApplicationDetailsSettings);
			settingsIntent.AddCategory(Intent.CategoryDefault);
			settingsIntent.SetData(global::Android.Net.Uri.Parse("package:" + PackageName));

			var flags = ActivityFlags.NewTask | ActivityFlags.NoHistory | ActivityFlags.ExcludeFromRecents;
			settingsIntent.SetFlags(flags);

			context.StartActivity(settingsIntent);
		}

		static AppTheme GetRequestedTheme()
		{
			var config = Application.Context.Resources?.Configuration;
			if (config == null)
				return AppTheme.Unspecified;

			return (config.UiMode & UiMode.NightMask) switch
			{
				UiMode.NightYes => AppTheme.Dark,
				UiMode.NightNo => AppTheme.Light,
				_ => AppTheme.Unspecified
			};
		}

		public AppTheme RequestedTheme => GetRequestedTheme();

		public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

		static LayoutDirection GetLayoutDirection()
		{
			var config = Application.Context.Resources?.Configuration;
			if (config == null)
				return LayoutDirection.Unknown;

			return (config.LayoutDirection == global::Android.Views.LayoutDirection.Rtl)
				? LayoutDirection.RightToLeft
				: LayoutDirection.LeftToRight;
		}

		public LayoutDirection RequestedLayoutDirection => GetLayoutDirection();
	}
}
