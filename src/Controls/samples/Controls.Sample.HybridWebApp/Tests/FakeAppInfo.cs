using Microsoft.Maui.ApplicationModel;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakeAppInfo : IAppInfo
{
	public Exception? Error { get; set; }
	public string Name => Error is null ? "Test app" : throw Error;
	public string VersionString => "1.2";
	public string BuildString => "3";
	public string PackageName => throw new InvalidOperationException("Not in the exposed subset.");
	public Version Version => throw new InvalidOperationException("Not in the exposed subset.");
	public AppTheme RequestedTheme => throw new InvalidOperationException("Not in the exposed subset.");
	public AppPackagingModel PackagingModel => throw new InvalidOperationException("Not in the exposed subset.");
	public LayoutDirection RequestedLayoutDirection => throw new InvalidOperationException("Not in the exposed subset.");
	public void ShowSettingsUI() => throw new InvalidOperationException("Not in the exposed subset.");
}
