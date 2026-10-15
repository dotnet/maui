using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.HybridWebApps.UI;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials.Launcher;

/// <summary>Public HTTPS launch through the OS, never a guest, file or shell command.</summary>
/// <param name="launcher">The host's Essentials launcher API.</param>
public sealed class LauncherProvider(ILauncher launcher)
{
	[HybridAction("launcher.openUrl", MainThread = true, Interactive = true)]
	internal async Task<LaunchUrlResult> OpenUrl(string url)
	{
		if (!await launcher.OpenAsync(RemoteWebAddress.Validate(url)))
			throw new CommandException("launch_failed", "The native system did not accept the HTTPS URL.");
		return new(true);
	}
}

sealed record LaunchUrlResult(bool Accepted);
