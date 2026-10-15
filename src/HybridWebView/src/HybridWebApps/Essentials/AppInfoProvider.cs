using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.AppInfo;

/// <summary>Application metadata from the host's existing Essentials API.</summary>
/// <param name="appInfo">The host's application metadata.</param>
public sealed class AppInfoProvider(IAppInfo appInfo)
{
	[HybridAction("app.info", MainThread = true)]
	internal AppInfoResult Info() => new(appInfo.Name, appInfo.VersionString, appInfo.BuildString);
}

sealed record AppInfoResult(string AppName, string Version, string Build);
