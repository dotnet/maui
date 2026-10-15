using Microsoft.Maui.ApplicationModel;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakeAppInfo : IAppInfo
{
	public Exception? Error { get; set; }
	public string Name => Error is null ? "Test app" : throw Error;
	public string VersionString => "1.2";
	public string BuildString => "3";
	public string PackageName => "test.native.app";
	public Version Version => new(1, 2);
	public AppTheme RequestedTheme => AppTheme.Dark;
	public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;
	public LayoutDirection RequestedLayoutDirection => LayoutDirection.RightToLeft;
	public void ShowSettingsUI() => throw new InvalidOperationException("Not in the exposed subset.");
}
