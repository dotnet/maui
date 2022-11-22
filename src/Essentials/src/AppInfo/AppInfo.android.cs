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
#pragma warning disable 618
		static readonly Lazy<PackageInfo> _packageInfo = new Lazy<PackageInfo>(() => Application.Context.PackageManager.GetPackageInfo(_packageName.Value, PackageInfoFlags.MetaData));
#pragma warning restore 618
		static readonly Lazy<AppTheme> _requestedTheme = new Lazy<AppTheme>(GetRequestedTheme);
		static readonly Lazy<LayoutDirection> _layoutDirection = new Lazy<LayoutDirection>(GetLayoutDirection);

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
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				flags |= ActivityFlags.LaunchAdjacent;
			settingsIntent.SetFlags(flags);

			context.StartActivity(settingsIntent);
		}

		static AppTheme GetRequestedTheme() => (Application.Context.Resources.Configuration.UiMode & UiMode.NightMask) switch
		{
			UiMode.NightYes => AppTheme.Dark,
			UiMode.NightNo => AppTheme.Light,
			_ => AppTheme.Unspecified
		};

		public AppTheme RequestedTheme => _requestedTheme.Value;

		public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

		static LayoutDirection GetLayoutDirection()
		{
			var config = Application.Context.Resources?.Configuration;
			if (config == null)
				return LayoutDirection.Unknown;

			return (config.LayoutDirection == Android.Views.LayoutDirection.Rtl) ? LayoutDirection.RightToLeft :
				LayoutDirection.LeftToRight;
		}

		public LayoutDirection RequestedLayoutDirection => _layoutDirection.Value;
	}
}
