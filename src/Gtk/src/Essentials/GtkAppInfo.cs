using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IAppInfo"/> for GTK. Identity comes from the entry assembly, the theme from the XDG settings portal
/// with a fallback to <c>Gtk.Settings</c>.
/// </summary>
sealed class GtkAppInfo : IAppInfo
{
	readonly Func<AppTheme> themeReader;

	internal GtkAppInfo(AppIdentity identity, Func<AppTheme>? themeReader = null)
	{
		Identity = identity;
		this.themeReader = themeReader ?? ReadTheme;
	}

	internal AppIdentity Identity { get; }

	public string PackageName => Identity.ApplicationId;

	public string Name => Identity.Name;

	public string VersionString => Identity.VersionString;

	public Version Version => Identity.Version;

	public string BuildString => Identity.BuildString;

	public AppTheme RequestedTheme => themeReader();

	public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

	public LayoutDirection RequestedLayoutDirection
	{
		get
		{
			try
			{
				return global::Gtk.Widget.DefaultDirection == global::Gtk.TextDirection.Rtl
					? LayoutDirection.RightToLeft
					: LayoutDirection.LeftToRight;
			}
			catch (Exception)
			{
				return LayoutDirection.LeftToRight;
			}
		}
	}

	/// <summary>Linux has no per-app system settings page.</summary>
	/// <exception cref="FeatureNotSupportedException">Always.</exception>
	public void ShowSettingsUI() =>
		throw new FeatureNotSupportedException("There is no per-app system settings UI on Linux.");

	static AppTheme ReadTheme()
	{
		var theme = PortalSettings.ToAppTheme(PortalSettings.TryReadColorScheme());
		return theme != AppTheme.Unspecified ? theme : PortalSettings.ReadGtkSettingsTheme();
	}
}
